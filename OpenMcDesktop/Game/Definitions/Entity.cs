using System.Numerics;
using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions;

public abstract class Entity
{
	// Get only, as they are comm,on across all entities of an entity type
	public virtual float Width => 0.5f;
	public virtual float Height => 1.0f;
	public virtual float HeadHeight => 0.5f;
	public virtual bool Alive => false;

	public long Id = 0;
	public string Name = "";
	public int State = 0;
	public double Age = 0;
	public Chunk? Chunk = null;
	public int Health = 0;
	public int BlocksWalked = 0;

	public double X = 0;
	public double Y = 0;
	public Vector2 Velocity = Vector2.Zero;
	public float Facing = 0;

	public Entity()
	{
		
	}
	
	public virtual void Render(RenderWindow window)
	{

	}
}
