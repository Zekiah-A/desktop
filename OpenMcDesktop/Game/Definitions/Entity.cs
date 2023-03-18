using System.Numerics;

namespace OpenMcDesktop.Game.Definitions;

public abstract class Entity
{
	// Get only, as they are comm,on across all entities of an entity type
	private float Width => 0.5f;
	private float Height => 1.0f;
	private float HeadHeight => 0.5f;
	
	public Vector2 Position = Vector2.Zero;
	public Vector2 ChunkPosition = Vector2.Zero;
	public Vector2 Displacement = Vector2.Zero;
	public bool Alive = false;
	public int State = 0;
	public int Facing = 0;
	public int BlocksWalked = 0;
	public int Age = 0;
}
