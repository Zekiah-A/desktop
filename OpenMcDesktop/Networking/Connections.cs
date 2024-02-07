using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Microsoft.Extensions.Logging;
using OpenMcDesktop.Gui;
using OpenMcDesktop.Game;
using OpenMcDesktop.Game.Definitions;
using WatsonWebsocket;
using SFML.Graphics;
using SFML.System;
using Item = OpenMcDesktop.Game.Definitions.Item;
using System.Globalization;
using DataProto;

namespace OpenMcDesktop.Networking;

/// <summary>
/// This class handles the current session's networking, a mix between https://github.com/open-mc/client/blob/main/iframe/ipc.js
/// and https://github.com/open-mc/client/blob/main/iframe/incomingPacket.js. It manages maintaining server connection, and delegates
/// events to and from the game renderer/input handler (World.cs)
/// </summary>
public partial class Connections
{
    private delegate void PacketHandler(ref ReadablePacket data);
    private readonly Dictionary<int, PacketHandler> packetHandlers;
    private readonly GameData gameData;
    private readonly Page serverLoadingPage;
    private bool initialPacket;
    private Label serverLoadingLabel;
    private readonly World world;

    public Connections(GameData data)
    {
        gameData = data;
        serverLoadingPage = new Page();
        packetHandlers = new Dictionary<int, PacketHandler>
        {
            { 1, RubberPacket },
            { 2, DimensionPacket },
            { 3, ClockSyncPacket },
            { 4, ServerPacket },
            //{ 5, ConfigPacket },
            { 8, BlockSetPacket },
            { 16, ChunkPacket },
            { 17, ChunkDeletePacket },
            //{ 19, WorldPacket },
            { 20, EntityPacket }
            //{ 64, OffsetBigIntPacket }
        };

        // Setup session world renderer
        world = new World(gameData);
        gameData.World = world;

        // Server connecting loading screen
        serverLoadingPage.Children.Add(gameData.DirtBackgroundRect);
        serverLoadingLabel = new Label("Connecting to server...", 24, LabelAccent.Default);
        serverLoadingLabel.Bounds.StartX = () => (int) (gameData.Window.GetView().Size.X / 2 - serverLoadingLabel.GetWidth() / 2);
        serverLoadingLabel.Bounds.StartY = () => (int) (gameData.Window.GetView().Size.Y / 2);
        serverLoadingPage.Children.Add(serverLoadingLabel);
        var backButton = new Button("Back to main menu",
            () => (int) (gameData.Window.GetView().Center.X - gameData.Window.GetView().Center.X / 4),
            () => (int) (gameData.Window.GetView().Size.Y / 2 + 60),
            () => (int) (gameData.Window.GetView().Size.X / 4),
            () => (int) (0.03 * gameData.Window.GetView().Size.X));
        backButton.OnMouseUp += (_, _) =>
        {
            gameData.CurrentPage = gameData.ServersPage;
        };
        serverLoadingPage.Children.Add(backButton);
    }

    private static string GetWebsocketUri(string ip)
    {
        if (!IpEncryptionRegex().IsMatch(ip))
        {
            var unencrypted = LoopbackDeviceRegex().IsMatch(ip);
            ip = (unencrypted ? "ws://" : "wss://") + ip;
        }
        if (!IpPortRegex().IsMatch(ip))
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
    public async Task<PreConnectData> PreConnect(string ip, ServerListItem targetItem)
    {
        var socket = new WatsonWsClient(new Uri($"{GetWebsocketUri(ip)}/{gameData.Name}" +
            $@"/{NetworkingHelpers.EncodeUriComponent(gameData.PublicKey)}" +
            $@"/{NetworkingHelpers.EncodeUriComponent(gameData.AuthSignature)}"));

        var name = ip;
        var motd = "Failed to connect";
        var imageTask = new TaskCompletionSource<Image>();
        var descriptionColour = new Color(255, 255, 255, 200);
        var challenge = Array.Empty<byte>();
        var packs = Array.Empty<string>();
        var timeout = new Timer(_ =>
        {
            descriptionColour = new Color(255, 0, 0, 200);
            motd = "Failed to connect (server timeout)";
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
            targetItem.Texture = new Texture(await imageTask.Task);
        }
        catch (TaskCanceledException) { /* Image will use default value */ }

        socket.ServerConnected -= OnSocketConnected;
        socket.MessageReceived -= OnMessageReceived;
        socket.ServerDisconnected -= OnSocketDisconnected;

        targetItem.WebviewWindowName = name;
        targetItem.Name = name;
        targetItem.Description = motd;
        targetItem.DescriptionColour = descriptionColour;
        targetItem.WebviewUri = ip;
        return new PreConnectData(socket, packs, challenge);
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
    private (Dictionary<string, T>, T[]) DecodePacksDefinition<T>(string[] definitions, string typeNamespace)
    {
        var names = new List<string>();
        var indexes = new Dictionary<string, T>();
        var sharedInstances = new List<T>();

        for (var i = 0; i < definitions.Length; i++)
        {
            var members = definitions[i].Split(" ");
            var name = members[0];
            Console.WriteLine(definitions[i]);

            if (members.Length >= 1)
            {

            }
            /*if (members.Length == 2)
		    {
			    // Read block save data, such as the items present inside a chest
			    var attempt = JsonSerializer.Deserialize<object>(members[1], new JsonSerializerOptions()
			    {
				    UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode
			    });
			    if (attempt is null)
			    {
				    types.Add();
			    }
			    else
			    {
				    types.Add(attempt);
			    }

		    }*/
        }

        return (indexes, sharedInstances.ToArray());
    }

    /// <summary>
    /// Actual connection to game server in order for us to start playing, will assign this server as the current server
    /// connection within game data, and controls the networking interactions for all other game processes.
    /// </summary>
    public async Task Connect(PreConnectData serverData)
    {
        //gameData.CurrentPage = serverLoadingPage;
        gameData.CurrentServer = serverData.Socket;
        gameData.CurrentServer.ServerDisconnected += OnSocketDisconnected;
        gameData.CurrentServer.ServerConnected += OnSocketConnected;
        gameData.CurrentServer.MessageReceived += OnMessageReceived;
        gameData.CurrentServer.Logger = (message) => gameData.Logger.LogInformation(message);

        // Apply data sent to us by server from packs to current client
        var blockDefinitions = serverData.DataPacks[0].Split("\n");
        (gameData.BlockIndex, gameData.Blocks) = DecodePacksDefinition<Block>(blockDefinitions, "Blocks");

        var itemDefinitions = serverData.DataPacks[1].Split("\n");
        (gameData.ItemIndex, gameData.Items) = DecodePacksDefinition<Item>(itemDefinitions, "Items");

        var entityDefinitions = serverData.DataPacks[2].Split("\n");
        (gameData.EntityIndex, gameData.Entities) = DecodePacksDefinition<Entity>(entityDefinitions, "Entities");

        await gameData.ModLoader.ExecutePack(serverData.DataPacks[3]);

        // Authenticate client fully with challenge & accept messages
        var signature = ProcessChallenge(serverData.Challenge);
        var packet = new byte[sizeof(ushort) + gameData.Skin.Length + signature.Length];

        packet[0] = (byte) (StaticData.ProtocolVersion >> 8);
        packet[1] = (byte) StaticData.ProtocolVersion;
        gameData.Skin.CopyTo(packet, sizeof(ushort));
        signature.CopyTo(packet, sizeof(ushort) + gameData.Skin.Length);
        await gameData.CurrentServer.SendAsync(packet);

        void OnSocketConnected(object? sender, EventArgs args)
        {
            // TODO: BUG: This function never seems to call
        }

        void OnMessageReceived(object? sender, MessageReceivedEventArgs args)
        {
            if (!initialPacket && world is not null)
            {
                gameData.CurrentPage = world.GameGuiPage;
                gameData.Logger.LogInformation("Successfully connected to server");
                initialPacket = true;
            }

            Console.Write("Packet: ");
            foreach (var @byte in args.Data) Console.Write(@byte + " ");
            Console.WriteLine();

            if (args.MessageType == WebSocketMessageType.Text)
            {
                StringPacket(Encoding.UTF8.GetString(args.Data.ToArray()));
            }
            else
            {
                var data = new ReadablePacket(args.Data.ToArray());
                var code = data.ReadByte();
                packetHandlers.GetValueOrDefault(code)?.Invoke(ref data);
            }
        }

        void OnSocketDisconnected(object? sender, EventArgs args)
        {
            //gameData.CurrentPage = serverLoadingPage;
            serverLoadingLabel.SetAccent(LabelAccent.Error);
            serverLoadingLabel.Text = "Connection to server closed unexpectedly!";
            gameData.Logger.LogError("Connection to server closed unexpectedly!");
        }
    }

    private void StringPacket(string message)
    {
        var negative = message[0] == '-';
        var stringCode = negative ? message[1].ToString() : message[..2];

        if (!int.TryParse(stringCode, NumberStyles.AllowHexSpecifier, null, out var style))
        {
            gameData.Logger.LogWarning("Rejected invalid string packet '{message}', no style code found", message);
            return;
        }
        if (negative)
        {
            style = -style;
        }

        switch (style)
        {
            case -1: // TODO: Call onerror instead
                serverLoadingLabel.Text = message[4..];
                gameData.CurrentPage = serverLoadingPage;
                break;
            case -2: // TODO: Call onpending instead
                gameData.Logger.LogInformation("pending: {message}", message[2..]);
                break;
            case -3: // TODO: Call wait for connection
                gameData.Logger.LogInformation("wait: {message}", message[2..]);
                break;
            default: // Chat message
                var colour = StaticData.TextColours[style & 15];
                var shadow = StaticData.TextShadows[style & 15];
                var decoration = StaticData.TextDecorations[style >> 4];
                world!.GameChat.Messages.Add(new ChatBoxItem(message[2..], colour, shadow, decoration));
                break;
        }
    }

    private void RubberPacket(ref ReadablePacket data)
    {
        gameData.MyPlayerId = data.ReadUInt() + data.ReadUShort() * 4294967296;
        var playerEntity = world?.Entities.GetValueOrDefault(gameData.MyPlayerId);
        if (playerEntity is not null && playerEntity != gameData.MyPlayer)
        {
            world?.AddEntity(playerEntity);
        }

        gameData.MyPlayerKey = data.ReadByte();
        if (world is not null)
        {
            world.TicksPerSecond = data.ReadFloat();
        }
    }

    /// <summary>
    /// A packet containing information about the current world and position that the player is located within
    /// </summary>
    private void DimensionPacket(ref ReadablePacket data)
    {
        var dimension = data.ReadString();
        var gravityX = data.ReadFloat();
        var gravityY = data.ReadFloat();
        var ticks = data.ReadDouble();

        world.Dimension = dimension;
        world.Gravity = new Vector2f(gravityX, gravityY);
        world.TickCount = ticks;
    }

    /// <summary>
    /// Game ticks occur at 20TPS, width ticks maintained clientside, server sends 1 clock sync packet per second
    /// that ensures the server's ticks are aligned with that on the client (prevents a desync).
    /// </summary>
    private void ClockSyncPacket(ref ReadablePacket data)
    {
        if (world is not null)
        {
            world.TickCount = data.ReadDouble();
        }
    }

    /// <summary>
    /// Provides information for use in serverlist / tab player list
    /// </summary>
    private void ServerPacket(ref ReadablePacket data)
    {
        var title = data.ReadString();
        var description = data.ReadString();
        world.GameServerMenu.Title = title;
        world.GameServerMenu.Description = description;

        if (data.Left == 0)
        {
            // No players
            return;
        }
        for (var playerCount = data.ReadFlexInt(); playerCount > 0; playerCount--)
        {
            var name = data.ReadString();
            var skinData = data.ReadBytes(192); // 8 (W) * 8 (H) * 3 (RGB)
            var health = data.ReadByte();
            var ping = data.ReadShort();
            var skinImage = new Image(8, 8);

            for (var i = 0; i < 192; i += 3)
            {
                skinImage.SetPixel(((uint) i / 3) % 8, (uint) i / 24,
                    new Color(skinData[i], skinData[i + 1], skinData[i + 2]));
            }
            world.GameServerMenu.Players.Add(new PlayerInfo(new Texture(skinImage), name, health, ping));
        }
    }

    /// <summary>
    /// A packet containing chunk data, from which a chunk object can be constructed
    /// </summary>
    private void ChunkPacket(ref ReadablePacket data)
    {
        return; // TODO: Fix this
        if (world is null)
        {
            return;
        }

        var chunk = new Chunk(ref data, gameData);
        /*var chunkKey = (chunk.X & 67108863) + (chunk.Y & 67108863) * 67108864;
        world.Map.TryAdd(chunkKey, chunk);
        world.CameraPosition = new Vector2f(chunk.X * 64, chunk.Y * 64);

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
        }*/
    }

    private void ChunkDeletePacket(ref ReadablePacket data)
    {
        if (world is null)
        {
            return;
        }

        while (data.Left > 0)
        {
            var chunkX = data.ReadInt();
            var chunkY = data.ReadInt();
            var chunkKey = (chunkX & 67108863) + (chunkY & 67108863) * 67108864;
            world.Map.TryRemove(chunkKey, out _);
        }
    }

    private void BlockSetPacket(ref ReadablePacket data)
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
                // TODO: Handle this block event
                var x = data.ReadInt();
                var y = data.ReadInt();
                var id = data.ReadUInt();
                world?.SetBlock(x, y, (int) id);
            }
            else
            {
                var x = data.ReadInt();
                var y = data.ReadInt();
                var blockId = data.ReadShort();
                world?.SetBlock(x, y, blockId);
            }
        }
    }

    /// <summary>
    /// This packet handles anything to do with entities, for instance an entity movement,
    /// change in state, or other such data will be distributed to the client via this packet.
    /// </summary>
    private void EntityPacket(ref ReadablePacket data)
    {
        return; // TODO: Patch

        while (data.Left > 0)
        {
            var action = (int) data.ReadByte();
            if (action == 0)
            {
                var type = data.ReadByte();
                if (type == 0)
                {
                    var target = world?.Entities.GetValueOrDefault(data.ReadUInt() + data.ReadUShort() * 4294967296);
                    if (target is not null)
                    {
                        world?.RemoveEntity(target);
                    }
                }
                else
                {
                    var target = world?.Entities.GetValueOrDefault(data.ReadUInt() + data.ReadUShort() * 4294967296);
                    // entity?.TriggerEvent(type)
                }
                continue;
            }

            var entityId = data.ReadUInt() + data.ReadUShort() * 4294967296;
            var entity = world?.Entities.GetValueOrDefault(entityId);
            if (entity is null)
            {
                // We create a new entity, it does not yet have a chunk or position
                if ((action & 128) != 0)
                {
                    action |= 256;
                    var entityType = gameData.Entities[data.ReadShort()];
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
                world?.MoveEntity(entity);
            }
            if ((action & 2) != 0)
            {
                entity.Y = data.ReadDouble();
                world?.MoveEntity(entity);
            }
            if ((action & 4) != 0)
            {
                entity.Name = data.ReadString();
            }
            if ((action & 8) != 0)
            {
                entity.State = data.ReadShort();
            }
            if ((action & 16) != 0)
            {
                entity.Velocity = entity.Velocity with { X = data.ReadFloat() };
                world?.MoveEntity(entity);
            }
            if ((action & 32) != 0)
            {
                entity.Velocity = entity.Velocity with { Y = data.ReadFloat() };
                world?.MoveEntity(entity);
            }
            if ((action & 64) != 0)
            {
                entity.Facing = data.ReadFloat();
            }
            if ((action & 128) != 0)
            {
                entity.SaveData = data.Read(entity.SaveData, entity.SaveDataType);
            }
            if ((action & 256) != 0)
            {
                world?.AddEntity(entity);
                // TODO: Fire placed event on entity
            }
        }
    }

    [GeneratedRegex(":\\d+$")]
    private static partial Regex IpPortRegex();
    [GeneratedRegex("\\w+:\\/\\/")]
    private static partial Regex IpEncryptionRegex();
    [GeneratedRegex("^(localhost|127.0.0.1|0.0.0.0|\\[::1\\])$")]
    private static partial Regex LoopbackDeviceRegex();
}
