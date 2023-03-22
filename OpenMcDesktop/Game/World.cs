using OpenMcDesktop.Game.Definitions.Blocks;
using OpenMcDesktop.Game.Definitions;
using SFML.Graphics;

namespace OpenMcDesktop.Game;

/// <summary>
/// Contains helper methods for interacting with game world, similar to client file
/// https://github.com/open-mc/client/blob/preview/iframe/world.js.
/// </summary>
public class World
{
    public static Texture TerrainAtlas; // Used by Block definitions
    public static Texture ItemsAtlas; // Used by Items definitions
    public const int BlockTextureSize = 64;

    private GameData gameData;
    
    static World()
    {
        TerrainAtlas = new Texture("Resources/Textures/terrain.png");
        ItemsAtlas = new Texture("Resources/Textures/items.png");
    }

    public World(GameData data)
    {
        gameData = data;
    }

    public Block GetBlock(int x, int y)
    {
        var chunkKey = (x >>> 6) + (y >>> 6) * 67108864;
        var chunk = gameData.Map.GetValueOrDefault(chunkKey);
        return chunk?.Tiles[(x & 63) + ((y & 63) << 6)] ?? gameData.Blocks[gameData.BlockIndex[typeof(OpenMcDesktop.Game.Definitions.Blocks.Air)]];
    }

    public void SetBlock(int x, int y)
    {
        var chunkKey = (x >>> 6) + (y >>> 6) * 67108864;
        var chunk = gameData.Map.GetValueOrDefault(chunkKey);
        if (chunk is null)
        {
            return;
        }

        
	}

    public static void AddEntity(Entity entity)
    {
        
    }

    public static void RemoveEntity(Entity entity)
    {
        
    }
}