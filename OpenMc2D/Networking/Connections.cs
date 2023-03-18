using System.IO.Compression;
using System.Net.WebSockets;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using OpenMc2D.Types;
using WatsonWebsocket;
using SFML.Graphics;

namespace OpenMc2D.Networking;

public class Connections
{
	private GameData gameData;
    public Connections(GameData data)
    {
	    gameData = data;
    }

    private static string GetWebsocketUri(string ip)
    {
	    if (!Regex.IsMatch(ip, @"\w+:\/\/"))
	    {
		    var unencrypted = Regex.IsMatch(ip, @"^(localhost|127.0.0.1|0.0.0.0|\[::1\])$");
		    ip = (unencrypted ? "ws://" : "wss://") + ip;
	    }
	    
	    if (!Regex.IsMatch(ip, @":\d+$"))
	    {
		    ip += ":27277";
	    }

	    return ip;
    }

    private async Task<Image> FetchImage(string uri)
    {
	    var response = await gameData.HttpClient.GetAsync(uri);
	    var data = await response.Content.ReadAsStreamAsync();
	    return new Image(data);
    }

    /// <summary>
    /// ServerList MOTD and server info initial query 
    /// </summary>
    public async Task<PreConnectData> PreConnect(string ip)
    {
	    var socket = new WatsonWsClient(new Uri($"{GetWebsocketUri(ip)}/{gameData.Name}" +
            $@"/{NetworkingHelpers.EncodeURIComponent(gameData.PublicKey)}" +
            $@"/{NetworkingHelpers.EncodeURIComponent(gameData.AuthSignature)}"));
	    var name = ip;
	    var motd = "Failed to connect";
	    var image = new Image(@"Resources/Brand/grass_icon.png");
	    var imageTask = new TaskCompletionSource<Image>();
	    var descriptionColour = new Color(255, 255, 255, 200);
	    var challenge = new byte[] {};
	    var packs = new string[] { };
	    var timeout = new Timer(_ =>
	    {
		    if (socket.Connected)
		    {
			    socket.Stop();
			    motd = "Server refused connection";
		    }
		    else
		    {
			    descriptionColour = new Color(255, 0, 0, 200);
			    motd = "Failed to connect (server timeout)";
		    }
		    imageTask.SetCanceled();
	    }, null, 5000, Timeout.Infinite);
	    
	    void OnSocketConnected(object? sender, EventArgs args)
	    {
		    timeout.Change(Timeout.Infinite, Timeout.Infinite);
	    }

	    void OnMessageReceived(object? sender, MessageReceivedEventArgs args)
	    {
		    var packet = (ReadablePacket) args.Data.ToArray();
		    name = packet.ReadString();
		    motd = packet.ReadString();
		    var imageUri = packet.ReadString();
		    var packsStream = new MemoryStream(packet.ReadByteArray());
		    challenge = packet.ReadByteArray();

		    Task.Run(async () =>
		    {
			    await using var inflater = new InflaterInputStream(packsStream);
			    using var uncompressed = new MemoryStream();
			    await inflater.CopyToAsync(uncompressed);
			    packs = Encoding.UTF8.GetString(uncompressed.ToArray()).Split('\0');
			    await packsStream.DisposeAsync();
			    
			    try
			    {
				    imageTask.SetResult(await FetchImage(imageUri));
			    }
			    catch (Exception)
			    {
				    imageTask.SetCanceled();
			    }
		    });

	    }

	    void OnSocketDisconnected(object? sender, EventArgs args)
	    {
		    descriptionColour = new Color(255, 0, 0, 200);
		    motd = "Server disconnected";
		    imageTask.SetCanceled();
	    }
	    
	    socket.ServerConnected += OnSocketConnected;
	    socket.MessageReceived += OnMessageReceived;
	    socket.ServerDisconnected += OnSocketDisconnected;
	    
	    await socket.StartAsync();
	    try
	    {
		    image = await imageTask.Task;
	    }
	    catch (Exception)
	    {
		    // Image will use default value
	    }
	    
	    socket.ServerConnected -= OnSocketConnected;
	    socket.MessageReceived -= OnMessageReceived;
	    socket.ServerDisconnected -= OnSocketDisconnected;

	    return new PreConnectData(socket, packs, challenge, new DisplayListItem(new Texture(image), name, motd)
	    {
		    DescriptionColour = descriptionColour
	    });
    }

    /// <summary>
    /// Processes challenge sent to us by server for account authorisation.
    /// </summary>
    public byte[] ProcessChallenge(byte[] challenge)
    {
	    var privateKeyData = Convert.FromBase64String(gameData.PrivateKey);
	    var rsa = RSA.Create();
	    rsa.ImportPkcs8PrivateKey(privateKeyData, out _);
	    var signature = rsa.SignData(challenge, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
	    return signature;
    }

    /// <summary>
    /// Actual connection to game server in order for us to start playing, will assign this server as the current server
    /// connection within game data, and controls the networking interactions for all other game processes.
    /// </summary>
    public async Task Connect(PreConnectData serverData)
    {
	    gameData.CurrentServer = serverData.Socket;
	    gameData.CurrentServer.MessageReceived += OnMessageReceived;
	    gameData.CurrentServer.MessageReceived += OnSocketDisconnected;
	    
	    // Apply data sent to us by server from packs to current client
	    var blockDefinitions = serverData.DataPacks[0].Split("\n");
	    var blocks = new List<Block>();
	    foreach (var blockType in blockDefinitions)
	    {
		    var members = blockType.Split(" ");
		    var typeName = members[0];

		    if (members.Length == 1)
		    {
			    blocks.Add(new Block(typeName));
			    continue;
		    }

		    for (var i = 1; i < members.Length; i++)
		    {
			    var attempt = JsonSerializer.Deserialize<Block>(members[i].Trim());

			    if (attempt is null)
			    {
				    blocks.Add(new Block(typeName));
			    }
			    else
			    {
				    attempt.Type = typeName;
				    blocks.Add(attempt);
			    }
		    }
	    }
	    gameData.Blocks = blocks.ToArray();
	    
	    var itemDefinitions = serverData.DataPacks[1].Split("\n");
	    gameData.Items = new Item[itemDefinitions.Length];
	    for (var i = 0; i < itemDefinitions.Length; i++)
	    {
		    gameData.Items[i] = new Item(itemDefinitions[i]);
	    }

	    var entityDefinitions = serverData.DataPacks[2].Split("\n");
	    var entities = new List<Entity>();
	    foreach (var entityType in entityDefinitions)
	    {
		    var members = entityType.Split(" ");
		    var typeName = members[0];

		    if (members.Length == 1)
		    {
			    entities.Add(new Entity(typeName));
			    continue;
		    }
		    
		    for (var i = 1; i < members.Length; i++)
		    {
			    var attempt = JsonSerializer.Deserialize<Entity>(members[i].Trim());

			    if (attempt is null)
			    {
				    entities.Add(new Entity(typeName));
			    }
			    else
			    {
				    attempt.Type = typeName;
				    entities.Add(attempt);
			    }
		    }
	    }
	    gameData.Entities = entities.ToArray();
	    
	    // Authenticate client fully with challenge & accept messages
	    var signature = ProcessChallenge(serverData.Challenge);
	    var packet = new byte[signature.Length + gameData.Skin.Length];
	    
	    gameData.Skin.CopyTo(packet, 0);
	    signature.CopyTo(packet, gameData.Skin.Length);
	    await gameData.CurrentServer.SendAsync(packet);
	    
	    void OnMessageReceived(object? sender, EventArgs args)
	    {
		    
	    }

	    void OnSocketDisconnected(object? sender, EventArgs args)
	    {
		    
	    }
    }
}