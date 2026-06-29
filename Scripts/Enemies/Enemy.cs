using Godot;
using System;
using System.Numerics;
using Vector2 = Godot.Vector2;

public partial class Enemy : RigidBody2D
{
	public enum State { Idle, Aggro}
	[ExportCategory("AI")]
	private State currentState = State.Idle;
	[Export] private float DetectionRange = 2000f;
	[Export] private float LoseAggroTime = 4f;

	[ExportCategory("Strzelanie")]
	[Export] private float ShootCooldown = 1.5f;
	[Export] private PackedScene BulletScene;

	[ExportCategory("Ruch")]
	[Export] private float IdleTurnSpeed = 1.5f;
	[Export] private float IdleThrust = 15000f;
	[Export] private float IdleMaxMoveSpeed = 240f;
	[Export] private float IdleDirectionChangeTime = 3.5f;
	[Export] private float AggroTurnSpeed = 4f;
	private float shootTimer = 0f;
	private float loseAggroTimer = 0f;
	private float idleMoveTimer = 0f;
	private float targetRotation = 0f;
	private float currentThrust = 0f;

	[ExportCategory("HP")]
	[Export] private float MaxHealth = 100f;
	private float currentHealth;
	private PlayerScript player;

	private RayCast2D visionRay;
	public override void _Ready()
	{
		targetRotation = (float) GD.RandRange(0, Mathf.Tau);
		player = (PlayerScript) GetTree().GetFirstNodeInGroup("Player");
		visionRay = GetNode<RayCast2D>("RayCast2D");
		currentThrust = IdleThrust;
	}
	public override void _Process(double delta)
	{
		float dt = (float) delta;

		
	}
    public override void _PhysicsProcess(double delta)
	{
		float dt = (float) delta;

		UpdateState(dt);
		HandleBehaviour(dt);

		if(currentState == State.Idle)
		{
			HandleIdlePhysics(dt);
		}
		GD.Randomize();
	}
	private void UpdateState(float dt)
	{
		//do testu
		currentState = State.Idle;
		return;
		//faktyczny kod
		bool canSeePlayer = CanSeePlayer();

		if(currentState == State.Idle)
		{
			if(canSeePlayer)
			{
				currentState = State.Aggro;
				loseAggroTimer = 0f;
			}
		}
		else if (currentState == State.Aggro)
		{
			if(canSeePlayer)
			{
				loseAggroTimer = 0f;
			}
			else
			{
				loseAggroTimer += dt;
				if(loseAggroTimer >= LoseAggroTime)
				{
					currentState = State.Idle;
					targetRotation = (float) GD.RandRange(0, Mathf.Tau);
				}
			}
		}
	}
	private bool CanSeePlayer()
	{
		if (player == null)
		{
			GD.PrintErr("Enemy nie widzi gracza");
			return false;
		}

		float distance = GlobalPosition.DistanceTo(player.GlobalPosition);
		if(distance > DetectionRange) return false;

		visionRay.TargetPosition = ToLocal(player.GlobalPosition);
		visionRay.ForceRaycastUpdate();

		if(visionRay.IsColliding())
		{
			var collider = visionRay.GetCollider();
			return collider is PlayerScript;
		}

		return false;
	}
	private void HandleBehaviour(float dt)
	{
		if(currentState == State.Aggro)
		{
			HandleAggro(dt);
		}
		else
		{
			HandleIdleLogic(dt);
		}
	}
	private void HandleAggro(float dt)
	{
		if (player == null)
		{
			GD.PrintErr("Gracza nie widzi enemy");
			return;
		}

		Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
		targetRotation = direction.Angle();

		Rotation = Mathf.RotateToward(Rotation, targetRotation, AggroTurnSpeed * dt);

		//strzelanie
		shootTimer += dt;
		if(shootTimer >= ShootCooldown)
		{
			shootTimer = 0f;
			ShootAt(player.GlobalPosition);
		}
	}
	private void HandleIdleLogic(float dt)
	{
		idleMoveTimer += dt;

		if(idleMoveTimer >= IdleDirectionChangeTime)
		{
			idleMoveTimer = 0f;
			IdleDirectionChangeTime = (float) GD.RandRange(2.5f, 5.0f);

			float randomOffset = (float) GD.RandRange(-1.2f, 1.2f);
			targetRotation = Rotation + randomOffset;
			GD.Print(Mathf.RadToDeg(targetRotation));
			currentThrust = GD.Randf() > 0.35f ? IdleThrust : 0f;
			//do naprawy (KURWA JEBANA)
		}

		Rotation = Mathf.RotateToward(Rotation, targetRotation, IdleTurnSpeed * dt);
	}
	private void HandleIdlePhysics(float dt)
	{
		if(currentThrust > 0f)
		{
			Vector2 forward = Vector2.Right.Rotated(Rotation);
			ApplyForce(forward * currentThrust);
		}
		if(LinearVelocity.Length() > IdleMaxMoveSpeed)
		{
			LinearVelocity = LinearVelocity.Normalized() * IdleMaxMoveSpeed;
		}
	}
	private void ShootAt(Vector2 targetGlobalPos)
	{
		Bullet bullet = BulletScene.Instantiate<Bullet>();
		bullet.GlobalPosition = GlobalPosition;
		Vector2 direction = (targetGlobalPos - GlobalPosition).Normalized();
		bullet.Rotation = direction.Angle();
		bullet.AddCollisionExceptionWith(this);
		GetTree().CurrentScene.AddChild(bullet);
	}
}
