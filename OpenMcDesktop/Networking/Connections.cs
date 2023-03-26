using System.Net.WebSockets;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using OpenMcDesktop.Gui;
using OpenMcDesktop.Game;
using OpenMcDesktop.Game.Definitions;
using OpenMcDesktop.Game.Definitions.Entities;
using WatsonWebsocket;
using SFML.Graphics;
using SFML.System;
using Item = OpenMcDesktop.Game.Definitions.Item;

namespace OpenMcDesktop.Networking;

/// <summary>
/// This class handles all networking within the game, a mix between https://github.com/open-mc/client/blob/main/iframe/ipc.js
/// and https://github.com/open-mc/client/blob/main/iframe/incomingPacket.js.
/// </summary>
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
            $@"/{NetworkingHelpers.EncodeUriComponent(gameData.PublicKey)}" +
            $@"/{NetworkingHelpers.EncodeUriComponent(gameData.AuthSignature)}"));
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
    private (Type[], Dictionary<Type, int>, T[]) DecodePacksDefinition<T>(string[] definitions, string typeNamespace)
    {
	    var types = new List<Type>();
		var indexes = new Dictionary<Type, int>();
		var sharedInstances = new List<T>();
		
	    for (var i = 0; i < definitions.Length; i++)
		{
		    var members = definitions[i].Split(" ");
		    var typeName = "OpenMcDesktop.Game.Definitions." + typeNamespace + "." +  members[0].ToPascalCase();

		    if (members.Length == 1)
		    {
			    var type = Type.GetType(typeName);
			    if (type != null)
			    {
				    var instance = (T) Activator.CreateInstance(type)!;

				    types.Add(type);
				    if (!indexes.Keys.Contains(type))
				    {
					    indexes.Add(type, i);
					    sharedInstances.Add(instance);
				    }
			    }
		    }

			// TODO: Unfortunately we can not work with modified / custom definitions as of yet.
		    /*for (var i = 1; i < members.Length; i++)
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
		    }*/
		}

		return (types.ToArray(), indexes, sharedInstances.ToArray());
	}

    /// <summary>
    /// Actual connection to game server in order for us to start playing, will assign this server as the current server
    /// connection within game data, and controls the networking interactions for all other game processes.
    /// </summary>
    public async Task Connect(PreConnectData serverData)
    {
	    gameData.CurrentServer = serverData.Socket;
	    gameData.CurrentServer.MessageReceived += OnMessageReceived;
	    gameData.CurrentServer.ServerDisconnected += OnSocketDisconnected;
	    
	    // Apply data sent to us by server from packs to current client
	    var blockDefinitions = serverData.DataPacks[0].Split("\n");
	    (gameData.BlockDefinitions, gameData.BlockIndex, gameData.Blocks) = DecodePacksDefinition<Block>(blockDefinitions, "Blocks");
	    
	    var itemDefinitions = serverData.DataPacks[1].Split("\n");
	    (gameData.ItemDefinitions, gameData.ItemIndex, gameData.Items) = DecodePacksDefinition<Item>(itemDefinitions, "Items");

	    var entityDefinitions = serverData.DataPacks[2].Split("\n");
	    (gameData.EntityDefinitions, _, _) = DecodePacksDefinition<Entity>(entityDefinitions, "Entities");

	    await gameData.ModLoader.ExecutePack(serverData.DataPacks[3]);
	    
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
		    Console.WriteLine("Connection to server closed unexpectedly!");
	    }
    }
    
    private void ChatPacket(string message)
    {
	    Console.WriteLine(message);
    }

    private void RubberPacket(ReadablePacket data)
    {
		gameData.MyPlayerId = data.ReadUInt() + data.ReadUShort() * 4294967296;
		var playerEntity = gameData.World?.Entities.GetValueOrDefault(gameData.MyPlayerId);
		if (playerEntity is not null && playerEntity != gameData.MyPlayer)
		{
			gameData.World?.AddEntity(playerEntity);
		}

		gameData.MyPlayerKey = data.ReadByte();
		if (gameData.World is not null)
		{
			gameData.World.TicksPerSecond = data.ReadFloat();
		}
    }

    /// <summary>
    /// A packet containing information about the current world and position that the player is located within
    /// </summary>
    private void DimPacket(ReadablePacket data)
    {
	    var dimension = data.ReadString();
	    var gravityX = data.ReadFloat();
	    var gravityY = data.ReadFloat();
	    var ticks = data.ReadDouble();

	    gameData.World = new World(gameData, dimension)
	    {
		    Gravity = new Vector2f(gravityX, gravityY),
		    TickCount = ticks
	    };
    }

    private void ClockPacket(ReadablePacket data)
    {
	    if (gameData.World is not null)
	    {
		    gameData.World.TickCount = data.ReadDouble();
	    }
    }

    /// <summary>
    /// A packet containing chunk data, from which a chunk object can be constructed
    /// </summary>
    private void ChunkPacket(ReadablePacket data)
    {
	    if (gameData.World is null)
	    {
		    return;
	    }

	    var chunk = new Chunk(data, gameData);
	    var chunkKey = (chunk.X & 67108863) + (chunk.Y & 67108863) * 67108864;
	    gameData.World.Map.Add(chunkKey, chunk);

	    // Read chunk entities
	    while (data.Left > 0)
	    {
		    var playerEntity = (Entity) Activator.CreateInstance(typeof(Player))!;
		    playerEntity.X = data.ReadDouble();
			playerEntity.Y = data.ReadDouble();
			playerEntity.Id = data.ReadUInt() + data.ReadShort() * 4294967296;
			playerEntity.Name = data.ReadString();
			playerEntity.Velocity = new Vector2(data.ReadFloat(), data.ReadFloat());
			playerEntity.Facing = data.ReadFloat();
			playerEntity.Age = data.ReadDouble();
			playerEntity.Chunk = chunk;

			chunk.Entities.Add(playerEntity);
	    }
    }

    private void ChunkDeletePacket(ReadablePacket data)
    {
	    if (gameData.World is null)
	    {
		    return;
	    }

	    while (data.Left > 0)
	    {
		    var chunkX = data.ReadInt();
		    var chunkY = data.ReadInt();
		    var chunkKey = (chunkX & 67108863) + (chunkY & 67108863) * 67108864;
		    gameData.World.Map.Remove(chunkKey);
	    }
    }

    private void BlockSetPacket(ReadablePacket data)
    {
	    while (data.Left > 0)
	    {
		    var type = data.ReadByte();

			if (type == 255)
			{

			}
			// It is a block event, such as a door being opened
		    else if (type > 0)
		    {
				var x = data.ReadInt();
				var y = data.ReadInt();
				var id = data.ReadUInt();
				// TODO: Handle this block event
		    }
		    else
		    {
				var x = data.ReadInt();
				var y = data.ReadInt();
				var blockId = data.ReadShort();
			    gameData.World?.SetBlock(x, y, blockId);
		    }
	    }
    }

	/// <summary>
	/// This packet handles anything to do with entities, for instance an entity movement,
	/// change in state, or other such data will be distributed to the client via this packet.
	/// </summary>
    private void EntityPacket(ReadablePacket data)
    {
		while (data.Left > 0)
		{
			var action = data.ReadByte();
			if (action == 0)
			{
				var type = data.ReadByte();
				if (type == 0)
				{
					var target = gameData.World?.Entities.GetValueOrDefault(data.ReadUInt() + data.ReadUShort() * 4294967296);
					if (target is not null)
					{
						gameData.World?.RemoveEntity(target);
					}
				}
				else
				{
					var target = gameData.World?.Entities.GetValueOrDefault(data.ReadUInt() + data.ReadUShort() * 4294967296);
					// entity?.TriggerEvent(type)
				}
				continue;
			}

			var entityId = data.ReadUInt() + data.ReadUShort() * 4294967296;
			var entity = gameData.World?.Entities.GetValueOrDefault(entityId);
			if (entity is null)
			{
				// We create a new entity, it does not yet have a chunk or position
				if ((action & 128) != 0)
				{
					var entityType = gameData.EntityDefinitions[data.ReadShort()];
					entity = (Entity?) Activator.CreateInstance(entityType);
					if (entity is not null)
					{
						entity.Id = entityId;
						entity.X = 1e100;
						entity.Y = 1e100;
					}
					else
					{
						continue;
					}
				}
				else
				{
					continue;
				}
			}

			if ((action & 1) != 0)
			{
				entity.X = data.ReadDouble();
				gameData.World?.MoveEntity(entity);
			}
			else if((action & 2) != 0)
			{
				entity.Y = data.ReadDouble();
				gameData.World?.MoveEntity(entity);
			}	
			else if ((action & 4) != 0)
			{
				entity.Name = data.ReadString();
			}
			else if ((action & 8) != 0)
			{
				entity.State = data.ReadShort();
			}
			else if ((action & 16) != 0)
			{
				entity.Velocity = entity.Velocity with { X = data.ReadFloat() };
				gameData.World?.MoveEntity(entity);
			}
			else if ((action & 32) != 0)
			{
				entity.Velocity = entity.Velocity with { Y = data.ReadFloat() };
				gameData.World?.MoveEntity(entity);
			}
			else if ((action & 64) != 0)
			{
				entity.Facing = data.ReadFloat();
			}
			else if ((action & 128) != 0)
			{
				// TODO: Implement entity SaveData
			}
			else if ((action & 256) != 0)
			{
				gameData.World?.AddEntity(entity);
				// TODO: Fire placed event on entity
			}
		}
    }
}