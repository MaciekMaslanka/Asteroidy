using Godot;
using System;
using System.Numerics;
using Vector2 = Godot.Vector2;

public partial class Enemy : RigidBody2D
{
	[Export] private float MovementRandomizerDelay = 3f;
	[Export] private float RotationRate = 5f;
	[Export] private PackedScene bulletScene;
	private float movementRandomizerTimer = 0f;
	private float currentRotationTarget = 0f;
	public override void _Ready()
	{
	}
	public override void _Process(double delta)
	{
		float dt = (float) delta;
		HandleMovement(dt);
	}
	private void HandleMovement(float dt)
	{
		movementRandomizerTimer += dt;
		if(movementRandomizerTimer >= MovementRandomizerDelay)
		{
			movementRandomizerTimer = 0;
			MovementRandomizerDelay += (float) GD.RandRange(-0.4f, 0.4f);

			currentRotationTarget = (float) GD.RandRange(0, Mathf.Tau);

			PlayerScript player = (PlayerScript) GetTree().GetFirstNodeInGroup("Player");
			ShootAt(player.GlobalPosition);
		}
		Rotation = Mathf.RotateToward(Rotation, currentRotationTarget, RotationRate*dt);
	}
	private void ShootAt(Vector2 targetGlobalPos)
	{
		Bullet bullet = bulletScene.Instantiate<Bullet>();
		bullet.GlobalPosition = GlobalPosition;
		Vector2 direction = (targetGlobalPos - GlobalPosition).Normalized();
		bullet.Rotation = direction.Angle();
		bullet.AddCollisionExceptionWith(this);
		GetTree().CurrentScene.AddChild(bullet);
	}
}
