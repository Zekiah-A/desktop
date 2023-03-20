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
	public Vector2 Displacement = Vector2.Zero;
	public double Facing = 0;
	public double Age = 0;
	public Chunk? Chunk = null;
	public int Health = 0;

	public Vector2 Position = Vector2.Zero;
	public int BlocksWalked = 0;

	public virtual void Render(RenderWindow window)
	{

	}
}
