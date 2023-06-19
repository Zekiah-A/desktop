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
	public object? SaveData = null; // TODO: Make dictionary, we can't keep using types and objects as it is too static
	public Type SaveDataType = typeof(object);

	public double X = 0;
	public double Y = 0;
	public Vector2 Velocity = Vector2.Zero;
	public float Facing = 0;
	
	public virtual void Render(RenderWindow window)
	{

	}
	
	/// <summary>
	/// Deep copies an entity, usually from a template instance such as those provided in the entity definitions to apply
	/// specific modifications, such as a certain health or effect, etc.
	/// </summary>
	/// <returns>Newly created deep clone of this entity instance</returns>
	public Entity CopyNew()
	{
		var newEntity = (Entity) MemberwiseClone();
		// TODO: Perhaps ditch inheritance so this can be implemented properly
		return newEntity;
	}
}
