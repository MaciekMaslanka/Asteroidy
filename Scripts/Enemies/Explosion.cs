using Godot;

public partial class Explosion : AnimatedSprite2D
{
	[Export] private GpuParticles2D particles;
	public override void _Ready()
	{
		particles.Finished += QueueFree;
		Play("explosion");
		particles.Emitting = true;
	}
}
