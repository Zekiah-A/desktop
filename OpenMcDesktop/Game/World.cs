using OpenMcDesktop.Game.Definitions;
using SFML.Graphics;

namespace OpenMcDesktop.Game;

/// <summary>
/// Contains helper methods for interacting with game world, similar to client file
/// https://github.com/open-mc/client/blob/preview/iframe/world.js.
/// </summary>
public static class World
{
    public static GameData GameData; // Injected by Program.cs
    public static Texture TerrainAtlas; // Used by Block definitions
    public static Texture ItemsAtlas; // Used by Items definitions
    public const int BlockTextureSize = 64;
    
    static World()
    {
        TerrainAtlas = new Texture("Resources/Textures/terrain.png");
        ItemsAtlas = new Texture("Resources/Textures/items.png");
    }

    public static Block GetBlock(int x, int y)
    {
        return null;
    }

    public static void SetBlock(int x, int y)
    {
        
    }

    public static void AddEntity(Entity entity)
    {
        
    }

    public static void RemoveEntity(Entity entity)
    {
        
    }
}