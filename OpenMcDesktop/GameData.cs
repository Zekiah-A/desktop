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
    public ModLoader ModLoader { get; set; }
    public World? World { get; set; }
    public View View { get; set; }
    public Page? CurrentPage { get; set; }

    // Game runtime objects and definitions
    public Type[] BlockDefinitions { get; set; } // We can use Activator.CreateInstance() to create instances from these block types
    public Type[] ItemDefinitions { get; set; } // We can use Activator.CreateInstance() to create instances from these item types
    public Type[] EntityDefinitions { get; set; } // We can use Activator.CreateInstance() to create instances from these entity types
    public Dictionary<Type, int> BlockIndex { get; set; } // Maps the type of block to the index of block in blocks
    public Dictionary<Type, int> ItemIndex { get; set; } // Maps the type of item to the index of item in items
    public Block[] Blocks { get; set; } // Shared objects for all block types that can be used to avoid creating thousands of identical block instances for blocks that have no variation (like grass, unlike chests)
    public Item[] Items { get; set; }  // Shared objects for all item types that can be used to avoid creating thousands of identical item instances for items that have no variation, like unstackable items with no unique qualities

    // Stuff about us, as a client specifically
    public long MyPlayerId { get; set; } = 0;
    public Entity MyPlayer { get; set;}
    public byte MyPlayerKey { get; set; } = 0;

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