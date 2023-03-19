using System.Numerics;

namespace OpenMcDesktop.Game.Definitions;

public abstract class Entity
{
	// Get only, as they are comm,on across all entities of an entity type
	private float Width => 0.5f;
	private float Height => 1.0f;
	private float HeadHeight => 0.5f;
	
	public long Id = 0;
	public string Name = "";
	public int State = 0;
	public Vector2 Displacement = Vector2.Zero;
	public double Facing = 0;
	public double Age = 0;
	public Chunk? Chunk = null;

	public Vector2 Position = Vector2.Zero;
	public bool Alive = false;
	public int BlocksWalked = 0;
}
