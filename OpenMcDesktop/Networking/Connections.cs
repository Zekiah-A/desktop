using System.Net.WebSockets;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Microsoft.Extensions.Logging;
using OpenMcDesktop.Gui;
using OpenMcDesktop.Game;
using OpenMcDesktop.Game.Definitions;
using OpenMcDesktop.Game.Definitions.Entities;
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
    private readonly Page gameGuiPage;
    private readonly Page serverLoadingPage;
    private bool initialPacket;
    private Label serverLoadingLabel;
    public Hotbar GameHotbar;
    public ChatBox GameChat;

    public Connections(GameData data)
    {
        gameData = data;
        gameGuiPage = new Page();
        serverLoadingPage = new Page();
        packetHandlers = new Dictionary<int, PacketHandler>
        {
            { 1, RubberPacket },
            { 2, DimensionPacket },
            { 3, ClockSyncPacket },
            { 8, BlockSetPacket },
            { 16, ChunkPacket },
            { 17, ChunkDeletePacket },
            { 20, EntityPacket }
        };

        // Game GUI page
        int GameHotbarHeight() => (int) (22 / 182.0f * gameData.Window.GetView().Size.X * 0.4f);
        int GameHotbarWidth() => (int) (gameData.Window.GetView().Size.X * 0.4f);

        GameChat = new ChatBox(
            Control.BoundsZero,
            () => (int) (gameData.Window.GetView().Size.Y / 2 - GameHotbarHeight() - 32),
            () => (int) (gameData.Window.GetView().Size.X / 2),
            () => (int) (gameData.Window.GetView().Size.Y / 2));
        gameGuiPage.Children.Add(GameChat);

        GameHotbar = new Hotbar(
            () => (int) (gameData.Window.GetView().Size.X / 2 - GameHotbarWidth() / 2.0f),
            () => (int) (gameData.Window.GetView().Size.Y - GameHotbarHeight() - 8),
            GameHotbarWidth,
            GameHotbarHeight);
        gameGuiPage.Children.Add(GameHotbar);

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
    private (string[], Dictionary<string, int>, T[]) DecodePacksDefinition<T>(string[] definitions, string typeNamespace)
    {
        var names = new List<string>();
        var indexes = new Dictionary<string, int>();
        var sharedInstances = new List<T>();

        for (var i = 0; i < definitions.Length; i++)
        {
            var members = definitions[i].Split(" ");
            var name = members[0].ToPascalCase();
            var typeName = "OpenMcDesktop.Game.Definitions." + typeNamespace + "." + name;

            if (members.Length >= 1)
            {
                var type = Type.GetType(typeName);
                if (type != null)
                {
                    var instance = (T) Activator.CreateInstance(type)!;

                    names.Add(name);
                    if (!indexes.Keys.Contains(name))
                    {
                        indexes.Add(name, i);
                        sharedInstances.Add(instance);
                    }
                }
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

        return (names.ToArray(), indexes, sharedInstances.ToArray());
    }

    /// <summary>
    /// Actual connection to game server in order for us to start playing, will assign this server as the current server
    /// connection within game data, and controls the networking interactions for all other game processes.
    /// </summary>
    public async Task Connect(PreConnectData serverData)
    {
        gameData.CurrentPage = serverLoadingPage;
        gameData.CurrentServer = serverData.Socket;
        gameData.CurrentServer.ServerDisconnected += OnSocketDisconnected;
        gameData.CurrentServer.ServerConnected += OnSocketConnected;
        gameData.CurrentServer.MessageReceived += OnMessageReceived;

        // Apply data sent to us by server from packs to current client
        var blockDefinitions = serverData.DataPacks[0].Split("\n");
        (gameData.BlockDefinitions, gameData.BlockIndex, gameData.Blocks) = DecodePacksDefinition<Block>(blockDefinitions, "Blocks");

        var itemDefinitions = serverData.DataPacks[1].Split("\n");
        (gameData.ItemDefinitions, gameData.ItemIndex, gameData.Items) = DecodePacksDefinition<Item>(itemDefinitions, "Items");

        var entityDefinitions = serverData.DataPacks[2].Split("\n");
        (gameData.EntityDefinitions, gameData.EntityIndex, gameData.Entities) = DecodePacksDefinition<Entity>(entityDefinitions, "Entities");

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
            if (!initialPacket)
            {
                gameData.CurrentPage = gameGuiPage;
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
            gameData.CurrentPage = serverLoadingPage;
            serverLoadingLabel.Accent = LabelAccent.Error;
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
                GameChat.Messages.Add(new ChatBoxItem(message[2..], colour, shadow, decoration));
                break;
        }
    }

    private void RubberPacket(ref ReadablePacket data)
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
    private void DimensionPacket(ref ReadablePacket data)
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

    /// <summary>
    /// Game ticks occur at 20TPS, width ticks maintained clientside, server sends 1 clock sync packet per second
    /// that ensures the server's ticks are aligned with that on the client (prevents a desync).
    /// </summary>
    private void ClockSyncPacket(ref ReadablePacket data)
    {
        if (gameData.World is not null)
        {
            gameData.World.TickCount = data.ReadDouble();
        }
    }

    /// <summary>
    /// A packet containing chunk data, from which a chunk object can be constructed
    /// </summary>
    private void ChunkPacket(ref ReadablePacket data)
    {
        if (gameData.World is null)
        {
            return;
        }

        var chunk = new Chunk(ref data, gameData);
        var chunkKey = (chunk.X & 67108863) + (chunk.Y & 67108863) * 67108864;
        gameData.World.Map.TryAdd(chunkKey, chunk);
        gameData.World.CameraPosition = new Vector2f(chunk.X * 64, chunk.Y * 64);

        Console.WriteLine("CHUNK X: {0}, CHUNK Y: {1}", chunk.X, chunk.Y);

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

    private void ChunkDeletePacket(ref ReadablePacket data)
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
            gameData.World.Map.TryRemove(chunkKey, out _);
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
                gameData.World?.SetBlock(x, y, (int) id);
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
                    action |= 256;
                    var entityType = gameData.EntityDefinitions[data.ReadShort()];
                    //entity = (Entity?) Activator.CreateInstance(entityType);
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
            if ((action & 2) != 0)
            {
                entity.Y = data.ReadDouble();
                gameData.World?.MoveEntity(entity);
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
                gameData.World?.MoveEntity(entity);
            }
            if ((action & 32) != 0)
            {
                entity.Velocity = entity.Velocity with { Y = data.ReadFloat() };
                gameData.World?.MoveEntity(entity);
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
                gameData.World?.AddEntity(entity);
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
