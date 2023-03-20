using System.Text.Json.Serialization;
using OpenMcDesktop.Game;
using OpenMcDesktop.Game.Definitions;
using OpenMcDesktop.Networking;
using WatsonWebsocket;

namespace OpenMcDesktop;

public class GameData
{
    // Game runtime objects and definitions
    public HttpClient HttpClient { get; init; } = new();
    public WatsonWsClient CurrentServer { get; set; }
    public Type[] BlockDefinitions { get; set; } // We can use Activator.CreateInstance() to create instances from these block types
    public Type[] ItemDefinitions { get; set; } // We can use Activator.CreateInstance() to create instances from these item types
    public Type[] EntityDefinitions { get; set; } // We can use Activator.CreateInstance() to create instances from these entity types
    public Dictionary<Type, int> BlockIndex { get; set; } // Maps the type of block to the index of block in blocks
    public Dictionary<Type, int> ItemIndex { get; set; } // Maps the type of item to the index of item in items
    public Block[] Blocks { get; set; } // Shared objects for all block types that can be used to avoid creating thousands of identical block instances for blocks that have no variation (like grass, unlike chests)
    public Item[] Items { get; set; }  // Shared objects for all item types that can be used to avoid creating thousands of identical item instances for items that have no variation, like unstackable items with no unique qualities
        
    // Game world components
    public Dictionary<int, Chunk> Map { get; set; }
    public Dictionary<int, Entity> Entities { get; set; }
    public double TickCount { get; set; }
    public float TicksPerSecond { get; set;  }

    // Account and authorisation
    public string PublicKey { get; set; }
    public string PrivateKey { get; set; }
    public string AuthSignature { get; set; }
    public string Name { get; set; }
    
    // Game options
    public int FrameSleepMs { get; set; } = 16;
    public List<string> KnownServers { get; set; } = new();
    public byte[] Skin { get; set; } = new byte[1008]; // 28*12*3
}