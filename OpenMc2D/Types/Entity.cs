using System.Text.Json.Serialization;

namespace OpenMc2D.Types;

public class Entity
{
    public string Type { get; set; }
    [JsonPropertyName("health")] public int Health { get; set; }
    [JsonPropertyName("inv")] public int[] Inventory { get; set; }
    [JsonPropertyName("dragging")] public int Dragging { get; set; }
    [JsonPropertyName("items")] public int[] Items { get; set; }
    [JsonPropertyName("selected")] public int Selected { get; set; }
    [JsonPropertyName("skin")] public int Skin { get; set; }
    
    public Entity(string type)
    {
        Type = type;
    }

    public Entity()
    {
        
    }
}