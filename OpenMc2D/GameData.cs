using System.Text.Json.Serialization;
using OpenMc2D.Networking;
using OpenMc2D.Types;
using WatsonWebsocket;

namespace OpenMc2D;

public class GameData
{
    // Game runtime objects
    public HttpClient HttpClient { get; init; } = new();
    public WatsonWsClient CurrentServer { get; set; }
    public Block[] Blocks { get; set; }
    public Item[] Items { get; set; }
    public Entity[] Entities { get; set; }
    public double Ticks { get; set; }
     
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