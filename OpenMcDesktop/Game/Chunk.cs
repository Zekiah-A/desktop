using System.Numerics;
using OpenMcDesktop.Game.Definitions.Blocks;
using OpenMcDesktop.Game.Definitions;
using OpenMcDesktop.Networking;
using SFML.Graphics;
using SFML.System;

namespace OpenMcDesktop.Game;

public class Chunk
{
	public int X;
	public int Y;
	// The blocks present within this chunk
	public Block?[] Tiles;
	// A palette of all block types that are used in this chunk
	public List<Block> Palette;
	// A list of all entities belonging/within this chunk
	public List<Entity> Entities;
	public byte[] Biomes;

	private GameData gameData;
	private Dictionary<Block, VertexArray> generatedVBOs;
	
	public Chunk(ref ReadablePacket data, GameData gameData)
	{
		var x = data.ReadInt();
		var y = data.ReadInt();

		X = x << 6 >> 6;
		Y = y << 6 >> 6;
		Tiles = new Block[4096]; // Chunk size is 64x64
		Entities = new List<Entity>();
		Palette = new List<Block>();
		this.gameData = gameData;
		generatedVBOs = new Dictionary<Block, VertexArray>();

		// Chunks are 64x64, while chunk position is a 32 bit integer, therefore highest possible chunk position is
		// 67108863. That leads to 6 wasted bits in the x and y component of the chunk position (12 bits total). These
		// bits are then used for space saving to encode the palette length for this chunk, it also means that a palette
		// can not be above 2^12 in length as a chunk only has 4096 blocks.
		var paletteLength = (x >>> 26) + (y >>> 26) * 64 + 1;
		
		// Read and add all entities belonging to this chunk
		var entityId = data.ReadShort();
		while (entityId != 0)
		{
			if (gameData.Entities[entityId].CopyNew() is not { } entity)
			{
				continue;
			}

			entity.X = data.ReadShort() / 1024.0f + (x << 6);
			entity.Y = data.ReadShort() / 1024.0f + (y << 6);
			entity.Id = data.ReadUInt() + data.ReadUShort() * 4294967296;
			entity.Name = data.ReadString();
			entity.State = data.ReadShort();
			entity.Velocity = new Vector2(data.ReadFloat(), data.ReadFloat());
			entity.Facing = data.ReadFloat();
			entity.Age = data.ReadDouble();
			entity.Chunk = this;
			
			if (entity.SaveData is not null)
			{
				entity.SaveData = data.Read(entity.SaveData, entity.SaveDataType);
			}

			// We add all entities back to the global world
			gameData.World?.AddEntity(entity);
			Entities.Add(entity);
			entityId = data.ReadShort();
			Console.WriteLine("...");
		}

		Biomes = data.ReadBytes(10);

		for (var i = 0; i < paletteLength; i++)
		{
			Palette.Add(gameData.Blocks[data.ReadShort()]);
		}
		
		// Decode the blocks in this chunk using palette and place into tiles - the format of the tile data depends on
		// the number of things in the pal
		var tilesI = 0;
		switch (paletteLength)
		{
			case < 2:
				// Basically an Array.Fill except based
				for (var i = 0; i < 4096; i++)
				{
					Tiles[i] = Palette[0];
				}
				break;
			case 2:
				for (var j = 0; j < 512; j++)
				{
					var block = data.ReadByte(); // Index of block in palette
					Tiles[tilesI] = Palette[block & 1];
					Tiles[tilesI + 1] = Palette[(block >> 1) & 1];
					Tiles[tilesI + 2] = Palette[(block >> 2) & 1];
					Tiles[tilesI + 3] = Palette[(block >> 3) & 1];
					Tiles[tilesI + 4] = Palette[(block >> 4) & 1];
					Tiles[tilesI + 5] = Palette[(block >> 5) & 1];
					Tiles[tilesI + 5] = Palette[(block >> 6) & 1];
					Tiles[tilesI + 6] = Palette[(block >> 7) & 1];
					tilesI += 8;
				}

				break;
			case <= 4:
				for (var i = 0; i < 1024; i++)
				{
					var block = data.ReadByte(); // Index of block in palette
					Tiles[tilesI++] = Palette[block & 3];
					Tiles[tilesI++] = Palette[(block >> 2) & 3];
					Tiles[tilesI++] = Palette[(block >> 4) & 3];
					Tiles[tilesI++] = Palette[(block >> 6)];
				}

				break;
			case <= 16:
				for (var i = 0; i < 2048; i++)
				{
					var block = data.ReadByte(); // Index of block in palette
					Tiles[tilesI++] = Palette[block & 15];
					Tiles[tilesI++] = Palette[block >> 4];
				}

				break;
			case <= 256:
				for (var i = 0; i < 4096; i++)
				{
					Tiles[i] = Palette[data.ReadByte()];
				}

				break;
			default:
				for (var j = 0; j < 6144; j++)
				{
					var block2 = data.ReadByte();
					Tiles[tilesI] = Palette[data.ReadByte() + ((block2 & 0x0F) << 8)];
					Tiles[tilesI] = Palette[data.ReadByte() + ((block2 & 0xF0) << 4)];
					tilesI++;
				}
				break;
		}
		
		// Decode SaveData containing blocks, such as chests
		for (var i = 0; i < 4096; i++)
		{
			var block = Tiles[i];
			if (block is null)
			{
				var airIndex = gameData.BlockIndex[nameof(Air)];
				Tiles[i] = gameData.Blocks[airIndex];
				return;
			}

			if (block.SaveData is not null)
			{
				block.SaveData = data.Read(block.SaveData, block.SaveDataType);
			}
		}

		if (gameData.GenerateChunkVBOs)
		{
			GenerateChunkVBO();
		}
	}

	public void GenerateChunkVBO()
	{
		foreach (var blockType in Palette)
		{
			if (blockType == gameData.Blocks[gameData.BlockIndex[nameof(Air)]])
			{
				continue;
			}
			
			var vertices = new List<Vertex>();
			
			for (var x = 0; x < 64; x++)
			{
				for (var y = 0; y < 64; y++)
				{
					if (Tiles[x | (y << 6)] == blockType)
					{
						// Top left vertex, at the X and Y
						vertices.Add(new Vertex(new Vector2f(16 * x, 16 * y), new Vector2f(0, 0)));
						// Top right vertex, at X + 16 and Y
						vertices.Add(new Vertex(new Vector2f(16 * x + 16, 16 * y), new Vector2f(16, 0)));
						// Bottom right vertex, at X + 16 and Y + 1 6
						vertices.Add(new Vertex(new Vector2f(16 * x + 16, 16 * y + 16), new Vector2f(16, 16)));
						// Bottom left vertex, at X and Y + 16
						vertices.Add(new Vertex(new Vector2f(16 * x, 16 * y + 16), new Vector2f(0, 16)));
					}
				}
			}

			var result = new VertexArray();
			result.PrimitiveType = PrimitiveType.Quads;
			foreach (var vertex in vertices)
			{
				result.Append(vertex);
			}

			generatedVBOs.Add(blockType, result);
		}
		
	}
	
	// TODO: Implement our own sprite batching algorithm using vertex array to try and optimise drawing performance to the maximum
	public void Render(RenderWindow window, View worldLayer, RenderStates states)
	{
		if (gameData.GenerateChunkVBOs && generatedVBOs.Count != 0)
		{
			foreach (var blockVertexPair in generatedVBOs)
			{
				states.Texture = blockVertexPair.Key.InstanceTexture;
				window.Draw(blockVertexPair.Value, states);
			}
		}
		else
		{
			using var blockSprite = new Sprite();

			for (var x = 0; x < 64; x++)
			{
				for (var y = 0; y < 64; y++)
				{
					var block = Tiles[x | (y << 6)];
					if (block is null or Air)
					{
						continue;
					}
				
					blockSprite.Texture =  block.InstanceTexture;
					blockSprite.Position = new Vector2f(X + x * World.BlockTextureWidth, Y + (63 - y) * World.BlockTextureHeight);
					blockSprite.Draw(window, states);
				}
			}
		}
	}
}
