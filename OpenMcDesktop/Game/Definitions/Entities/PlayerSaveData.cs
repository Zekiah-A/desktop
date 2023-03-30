namespace OpenMcDesktop.Game.Definitions.Entities;

public class PlayerSaveData
{
    public byte Health { get; set; } = 0;
    public Item[] Inventory { get; set; } = new Item[37];
    public Item[] Items { get; set; } = new Item[6];
    public byte Selected { get; set; } = 0;
    public byte[] Skin { get; set; }
}