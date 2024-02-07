using System.Resources;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenMcDesktop.Networking;
using OpenMcDesktop.Game;
using OpenMcDesktop.Game.Definitions;
using OpenMcDesktop.Gui;
using OpenMcDesktop.Mods;
using SFML.Graphics;
using WatsonWebsocket;

namespace OpenMcDesktop;

public class GameData
{
    // Global program objects
    public HttpClient HttpClient { get; }
    public WatsonWsClient CurrentServer { get; set; }
    public Storage Storage { get; set; }
    public ILogger Logger { get; set; }
    public ResourceManager Translations { get; set; }
    public IHost Host { get; set; }
    public ModLoader ModLoader { get; set; }
    public World? World { get; set; }
    public Connections? Connection { get; set; }
    public RenderWindow Window { get; set; }
    public View WorldLayer { get; set; } // Blocks, chunks terrain
    public View BackgroundLayer { get; set; } // Game sky, background effects, etc
    public View UiLayer { get; set; } // Interfaces, menus, etc
    public Page? CurrentPage { get; set; }
    public Page? ServersPage { get; set; }
    public TextureRect DirtBackgroundRect { get; set; } // Shared against many pages

    // Vanilla blocks/items/entities actuually used in the game (vanilla definitions requested by packs
    // + custom runtime blocks defined here).
    // Maps string block/item/entity name to integer index in blocks/items/entities
    public Dictionary<string, Block> BlockIndex { get; set; }
    public Dictionary<string, Item> ItemIndex { get; set; }
    public Dictionary<string, Entity> EntityIndex { get; set; }
    // Integer block/item/entitiy indexes
    public Block[] Blocks { get; set; }
    public Item[] Items { get; set; }
    public Entity[] Entities { get; set; }

    // Specific client properties
    public long MyPlayerId { get; set; }
    public Entity MyPlayer { get; set; }
    public byte MyPlayerKey { get; set; }

    // Account and authorisation
    public string PublicKey { get; set; }
    public string PrivateKey { get; set; }
    public string AuthSignature { get; set; }
    public string Name { get; set; }

    // Game options
    public List<string> KnownServers { get; set; } = new();
    public byte[] Skin { get; set; } = new byte[1008]; // 28*12*3
    public bool GenerateChunkVBOs { get; set; } = false;

    public GameData()
    {
        HttpClient = new();
    }
}
