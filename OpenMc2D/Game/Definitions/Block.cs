using SFML.Graphics;

namespace OpenMc2D.Game.Definitions;

public class Block
{
	// Get-only as all instances of a block type have to have the same values for these properties
	public virtual bool Solid => true;
	public virtual Texture Texture => new Texture(0, 0);
	public virtual bool Climbable => false;
	public virtual float Viscosity => 0.0f;
	public virtual float BreakTime => 0.0f;
	
	/// <summary>
	/// Called to allow the block to implement it's own functionality upon being placed, such as custom place sounds.
	/// </summary>
	public virtual void Place(int x, int y)
	{
		
	}
	
	public virtual void Break(int x, int y)
	{
		
	}

	public virtual void Punch(int x, int y, Entity entity)
	{
		
	}

	public virtual void WalkOn(int x, int y, Entity entity)
	{

	}

	public virtual void Fall(int x, int y)
	{
		
	}
}
