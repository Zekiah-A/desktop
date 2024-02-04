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
    public HttpClient HttpClient { get; } = new();
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


    // Game runtime objects and definitions
    // We can use Activator.CreateInstance() to create instances from these [thing] types
    public string[] BlockDefinitions { get; set; }
    public string[] ItemDefinitions { get; set; }
    public string[] EntityDefinitions { get; set; }
    // Maps the type of [thing] to the index of [thing] in [Thing]s
    public Dictionary<string, int> BlockIndex { get; set; }
    public Dictionary<string, int> ItemIndex { get; set; }
    public Dictionary<string, int> EntityIndex { get; set; }
    // Shared/template objects for all [thing] types that can be used to avoid creating thousands of identical block
    // instances for [thing] that have no variation (like grass), or to copy from for creating new [thing] instances.
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
}
