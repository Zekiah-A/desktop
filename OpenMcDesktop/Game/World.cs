using System.Runtime.CompilerServices;
using OpenMcDesktop.Game.Definitions.Blocks;
using OpenMcDesktop.Game.Definitions;
using SFML.Graphics;
using SFML.System;

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
    public string Dimension = "overworld";
    public Dictionary<int, Chunk> Map { get; set; }
    public Dictionary<long, Entity> Entities { get; set; }
    public double TickCount { get; set; } = 0;
    public float TicksPerSecond { get; set; } = 0;

    // These are in world units
    public Vector2f CameraCentre { get => CameraPosition + CameraSize / 2; set => CameraPosition = value + CameraSize / 2; }
    public Vector2f CameraPosition { get; set; } = new Vector2f(0, 16); // Where the camera is in the world
    public Vector2f CameraSize { get; set; } = new Vector2f(28, 16); // How many (blocks) across and up/down camera can see
    public int CameraZoomLevel { get; set; } = 1;
    public int[] CameraZoomRealBlockSizes { get; set; }  = { 32, 64, 128,256 };
    
    private GameData gameData;

    static World()
    {
        TerrainAtlas = new Texture("Resources/Textures/terrain.png");
        ItemsAtlas = new Texture("Resources/Textures/items.png");
    }

    public World(GameData data, string dimension)
    {
        Dimension = dimension;
        gameData = data;
        Map = new Dictionary<int, Chunk>();
        Entities = new Dictionary<long, Entity>();
    }

    public Block GetBlock(int x, int y)
    {
        var chunkKey = (x >>> 6) + (y >>> 6) * 67108864;
        var chunk = Map.GetValueOrDefault(chunkKey);
        return chunk?.Tiles[(x & 63) + ((y & 63) << 6)] ?? gameData.Blocks[gameData.BlockIndex[typeof(Air)]];
    }

    public void SetBlock(int x, int y, int blockId)
    {
        var chunkKey = (x >>> 6) + (y >>> 6) * 67108864;
        var chunk = Map.GetValueOrDefault(chunkKey);
        if (chunk is not null)
        {
            chunk.Tiles[x & 63 + (y & 63 << 6)] = gameData.Blocks[blockId];
        }
    }

    public void AddEntity(Entity entity)
    {
        Entities.Add(entity.Id, entity);
        if (entity.Id == gameData.MyPlayerId)
        {
            gameData.MyPlayer = entity;
            CameraCentre = new Vector2f((float) gameData.MyPlayer.X, (float) gameData.MyPlayer.Y);
        }
    }

    public void MoveEntity(Entity entity)
    {
        // Chunk that the entity now is in
        var newChunk = Map.GetValueOrDefault((((int) Math.Floor(entity.X)) >>> 6) + (((int) Math.Floor(entity.Y)) >>> 6) * 67108864);
        if (newChunk != entity.Chunk)
        {
            entity.Chunk?.Entities.Remove(entity);
            entity.Chunk = newChunk;
            entity.Chunk?.Entities.Add(entity);
        }
    }

    public void RemoveEntity(Entity entity)
    {
        Entities.Remove(entity.Id);
        if (entity == gameData.MyPlayer)
        {
            gameData.MyPlayerId = -1;
        }
        
        entity.Chunk?.Entities.Remove(entity);
    }

    /// <summary>
    /// Turns a given position in block world co-ordinates to screen co-ordinates, so that the rendering position can be determined.
    /// </summary>
    public Vector2f WorldToScreen(Vector2f blockXy)
    {
        // Get the block co-ords out of world space as a fraction relative to the BL of our camera
        var cameraBottomLeft = new Vector2f(CameraPosition.X, CameraPosition.Y - CameraSize.Y);
        var relative = blockXy - cameraBottomLeft;
        var fraction = new Vector2f(relative.X / CameraSize.X, relative.Y / CameraSize.Y);
        
        // Get co-ords into screen space, ↑y within the world increases, while ↓y in screen increases, so we invert fraction on Y.
        var tlFraction = new Vector2f(fraction.X, 1 - fraction.Y);
        return new Vector2f(tlFraction.X * gameData.View.Size.X, tlFraction.Y * gameData.View.Size.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AdjustCameraSize()
    {
        // 64px screen width and height at standard zoom
        var realBlockSize = CameraZoomRealBlockSizes[Math.Clamp(CameraZoomLevel, 0, CameraZoomRealBlockSizes.Length - 1)];
        // At fullscreen, 1920 / n = realBlockSize, 1080 / n realBlockSize, so to keep blocks square, we just have to find N for X and Y
        CameraSize = new Vector2f(gameData.View.Size.X / realBlockSize, gameData.View.Size.Y / realBlockSize);
    }

    public Vector2f ScreenToWorld(int screenX, int screenY)
    {
        throw new NotImplementedException();
    }

    public void Render(RenderWindow window, View view)
    {
        AdjustCameraSize();
        var currentMovement = new Vector2f(0, 0);
        
        // 1024 px real chunk width 64 * 16
        lock (Map)
        {
            foreach (var chunk in Map.Values)
            {
                var movement = currentMovement = WorldToScreen(new Vector2f(chunk.X * 64, chunk.Y * 64)) - currentMovement;
                view.Move(movement);
                window.SetView(view);
                chunk.Render(window);
                view.Move(-movement);
            }
        }
    }
}