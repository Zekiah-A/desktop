using System.Text.Json.Serialization;
using OpenMc2D.Networking;
using WatsonWebsocket;

namespace OpenMc2D;

public class GameData
{
    // Game runtime objects
    public HttpClient HttpClient { get; init; } = new();
    public Item[] ItemIds { get; init; }
    public WatsonWsClient CurrentServer { get; set; }
    
    // Account and authorisation
    [JsonInclude]
    public string PublicKey { get; set; }
    [JsonInclude]
    public string PrivateKey { get; set; }
    [JsonInclude]
    public string AuthSignature { get; set; }
    [JsonInclude]
    public string Name { get; set; }
    
    // Game options
    [JsonInclude]
    public int FrameSleepMs { get; set; } = 16;
}