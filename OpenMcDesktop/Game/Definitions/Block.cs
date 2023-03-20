using SFML.Graphics;

namespace OpenMcDesktop.Game.Definitions;

public abstract class Block
{
	// Static properties - All instances of, for example, block type grass will have the same texture, so only one needs
	// to ever exist for all instances of this type. However, an instance copy is needed because this can only be accessed at compiletime.
	public static Texture Texture => new Texture(0, 0);
	
	// Get-only as all instances of a block type have to have the same values for these properties, InstanceTexture reflects
	// the static Texture property belonging to any block derived type, HOWEVER, it is completely safe against upcasting due to
	// being a common property that is simply overridden. So we do not need to worry about making multiple, i.e, render methods for different
	// block types.
	public virtual Texture InstanceTexture => Texture;
	public virtual bool Solid => true;
	public virtual bool Climbable => false;
	public virtual float Viscosity => 0.0f;
	public virtual float BreakTime => 0.0f;
	public virtual Tool Tool => Tool.None;
	
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
