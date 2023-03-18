using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using OpenMcDesktop.Gui;
using OpenMcDesktop.Game;
using WatsonWebsocket;
using SFML.Graphics;

namespace OpenMcDesktop.Networking;

public class Connections
{
	public delegate void PacketHandler(ReadablePacket data);
	public readonly Dictionary<int, PacketHandler> PacketHandlers;

	private readonly GameData gameData;
	
	public Connections(GameData data)
    {
	    gameData = data;
	    PacketHandlers = new Dictionary<int, PacketHandler>
	    {
		    { 1, RubberPacket },
		    { 2, DimPacket },
		    { 3, ClockPacket },
		    { 8, BlockSetPacket },
		    { 16, ChunkPacket },
		    { 17, ChunkDeletePacket },
		    { 20, EntityPacket }
	    };
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
    /// Creates a GameData *Definitions array of types from the server's initial connection packs packet containing block IDs/definitions.
    /// </summary>
    /// <param name="definitions"></param>
    /// <returns></returns>
    private Type[] DecodePacksDefinition(IEnumerable<string> definitions)
    {
	    var types = new List<Type>();
	    foreach (var type in definitions)
	    {
		    var members = type.Split(" ");
		    var typeName = "OpenMcDesktop.Game.Definitions." +  members[0].ToPascalCase();

		    if (members.Length == 1)
		    {
			    var instance = Type.GetType(typeName);
			    if (instance != null)
			    {
				    types.Add(instance);
			    }
		    }
/*
			// TODO: Unfortunately we can not work with modified / custom definitions as of yet.
		    for (var i = 1; i < members.Length; i++)
		    {
			    var attempt = JsonSerializer.Deserialize<T>(members[i].Trim());

			    if (attempt is null)
			    {
				    types.Add(instance);
			    }
			    else
			    {
				    types.Add(attempt);
			    }
		    }
*/
	    }

	    return types.ToArray();
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
	    gameData.BlocksDefinitions = DecodePacksDefinition(blockDefinitions);
	    
	    var itemDefinitions = serverData.DataPacks[1].Split("\n");
	    gameData.ItemDefinitions = DecodePacksDefinition(itemDefinitions);

	    var entityDefinitions = serverData.DataPacks[2].Split("\n");
	    gameData.EntityDefinitions = DecodePacksDefinition(entityDefinitions);
	    
	    // Authenticate client fully with challenge & accept messages
	    var signature = ProcessChallenge(serverData.Challenge);
	    var packet = new byte[signature.Length + gameData.Skin.Length];
	    
	    gameData.Skin.CopyTo(packet, 0);
	    signature.CopyTo(packet, gameData.Skin.Length);
	    await gameData.CurrentServer.SendAsync(packet);
	    
	    void OnMessageReceived(object? sender, MessageReceivedEventArgs args)
	    {
		    Console.WriteLine("Packet: ");
		    foreach (var @byte in args.Data) Console.Write(@byte + " ");
		    Console.WriteLine("");

		    if (args.MessageType == WebSocketMessageType.Text)
		    {
			    ChatPacket(Encoding.UTF8.GetString(args.Data.ToArray()));
		    }
		    else
		    {
			    PacketHandlers.GetValueOrDefault(args.Data[0])?.Invoke((ReadablePacket) args.Data[1..].ToArray());
		    }
	    }

	    void OnSocketDisconnected(object? sender, EventArgs args)
	    {
		    
	    }
    }
    
    private void ChatPacket(string message)
    {
	    Console.WriteLine(message);
	    
    }

    private void RubberPacket(ReadablePacket data)
    {
	    
    }

    private void DimPacket(ReadablePacket data)
    {
	    var world = data.ReadString();
	    var globalX = data.ReadFloat();
	    var globalY = data.ReadFloat();
	    var ticks = data.ReadDouble();

	    Console.WriteLine($"{world}, {globalX}, {globalY}, {ticks}");
    }

    private void ClockPacket(ReadablePacket data)
    {
	    gameData.TickCount = data.ReadDouble();
    }

    private void ChunkPacket(ReadablePacket data)
    {
	    
    }

    private void ChunkDeletePacket(ReadablePacket data)
    {
	    
    }

    private void BlockSetPacket(ReadablePacket data)
    {
	    
    }

    private void EntityPacket(ReadablePacket data)
    {
	    
    }
}