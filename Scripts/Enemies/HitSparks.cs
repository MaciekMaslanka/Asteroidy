using Godot;
using System;

public partial class HitSparks : Node2D
{
	[Export] private GpuParticles2D particles;

    public override void _Ready()
    {
        particles.Emitting = true;
		particles.Finished += () => QueueFree();
    }
}
