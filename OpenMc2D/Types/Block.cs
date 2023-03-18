using System.Text.Json.Serialization;

namespace OpenMc2D.Types;

public class Block
{
    public string Type { get; set; }
    [JsonPropertyName("items")] public int[] Items { get; set; }
    [JsonPropertyName("name")] public int Name { get; set; }
    
    public Block(string type)
    {
        Type = type;
    }

    public Block()
    {
        
    }
}