using OpenMcDesktop.Game.Definitions.Blocks;
using OpenMcDesktop.Game.Definitions;
using SFML.Graphics;

namespace OpenMcDesktop.Game;

/// <summary>
/// Contains helper methods for interacting with game world, similar to client file
/// https://github.com/open-mc/client/blob/preview/iframe/world.js, except more involved within the actual game, and can
/// co-ordinate core game functions like chunk rendering.
/// </summary>
public class World
{
    public static Texture TerrainAtlas;
    public static Texture ItemsAtlas;
    public static int BlockTextureWidth = 16;
    public static int BlockTextureHeight = 16;
    
    // Game world components
    public Dictionary<int, Chunk> Map { get; set; }
    public Dictionary<int, Entity> Entities { get; set; }
    public double TickCount { get; set; }
    public float TicksPerSecond { get; set;  }

    private GameData gameData;

    static World()
    {
        TerrainAtlas = new Texture("Resources/Textures/terrain.png");
        ItemsAtlas = new Texture("Resources/Textures/items.png");
    }

    public World(GameData data)
    {
        gameData = data;
        Map = new Dictionary<int, Chunk>();
        Entities = new Dictionary<int, Entity>();
    }

    public Block GetBlock(int x, int y)
    {
        var chunkKey = (x >>> 6) + (y >>> 6) * 67108864;
        var chunk = Map.GetValueOrDefault(chunkKey);
        return chunk?.Tiles[(x & 63) + ((y & 63) << 6)] ?? gameData.Blocks[gameData.BlockIndex[typeof(Air)]];
    }

    public void SetBlock(int x, int y)
    {
        var chunkKey = (x >>> 6) + (y >>> 6) * 67108864;
        var chunk = Map.GetValueOrDefault(chunkKey);
        if (chunk is null)
        {
            return;
        }
    }

    public void AddEntity(Entity entity)
    {
        
    }

    public void RemoveEntity(Entity entity)
    {
        
    }

    public void Render(RenderWindow window)
    {
        foreach (var pair in Map)
        {
            pair.Value.Render(window);
        }
    }
}