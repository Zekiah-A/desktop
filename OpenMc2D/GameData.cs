using System.Text.Json.Serialization;
using OpenMc2D.Game;
using OpenMc2D.Game.Definitions;
using OpenMc2D.Networking;
using WatsonWebsocket;

namespace OpenMc2D;

public class GameData
{
    // Game runtime objects and definitions
    public HttpClient HttpClient { get; init; } = new();
    public WatsonWsClient CurrentServer { get; set; }
    public Type[] BlocksDefinitions { get; set; } // We can use Activator.CreateInstance() to create instances from these block types
    public Type[] ItemDefinitions { get; set; } // We can use Activator.CreateInstance() to create instances from these item types
    public Type[] EntityDefinitions { get; set; } // We can use Activator.CreateInstance() to create instances from these entity types
    
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
    public byte[] Skin { get; set; } = new byte[1008];
}