using Godot;
using System;
using System.ComponentModel;
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
	[Export] private float AvoidStrength = 3f;

	[ExportGroup("RandomMovement")]
	[Export] private float TargetPositionMaxDistance = 4000f;
	[Export] private float TargetPositionMinDistance = 100f;
	[Export] private float TargetPositionMarginError = 15f;

	[ExportGroup("Idle")]
	[Export] private float IdleTurnSpeed = 1.5f;
	[Export] private float IdleThrust = 15000f;
	[Export] private float IdleMaxMoveSpeed = 240f;
	[Export] private float IdleDirectionChangeTime = 10f;

	[ExportGroup("Aggro")]
	[Export] private float AggroTurnSpeed = 4f;
	
	private float shootTimer = 0f;
	private float loseAggroTimer = 0f;
	private float idleMoveTimer = 0f;
	private Vector2 targetPosition = new();
	private Vector2 targetDirection = new();
	private float currentThrust = 0f;

	private RayCast2D leftRay;
	private RayCast2D rightRay;

	[ExportCategory("HP")]
	[Export] private float MaxHealth = 100f;
	private float currentHealth;
	private PlayerScript player;

	private RayCast2D visionRay;

	//debug
	[Export] private Sprite2D targetSpr;
	public override void _Ready()
	{
		SelectRandomTarget();
		rightRay = GetNode<RayCast2D>("Sensors/RightRay");
		leftRay = GetNode<RayCast2D>("Sensors/LeftRay");
		visionRay = GetNode<RayCast2D>("Sensors/VisionRay");

		player = (PlayerScript) GetTree().GetFirstNodeInGroup("Player");
		currentThrust = IdleThrust;
	}
    public override void _PhysicsProcess(double delta)
	{
		float dt = (float) delta;

		UpdateState(dt);
		HandleBehaviour(dt);
		HandleRotation(dt);
		HandleMovement(dt);
	}
	private void HandleRotation(float dt)
	{
		//unikanie
		Vector2 avoid = Vector2.Zero;
		if(leftRay.IsColliding())
		{
			float distance = leftRay.GetCollisionPoint().DistanceTo(GlobalPosition);
			float strength = 1 / (distance * distance);
			strength = Mathf.Clamp(strength, 0f, 3f);
			avoid += Vector2.Down.Rotated(Rotation) * strength;
		}
		if(rightRay.IsColliding())
		{
			float distance = rightRay.GetCollisionPoint().DistanceTo(GlobalPosition);
			float strength = 1 / (distance * distance);
			strength = Mathf.Clamp(strength, 0f, 3f);
			avoid += Vector2.Up.Rotated(Rotation) * strength;
		}

		float distToTarget = GlobalPosition.DistanceTo(targetPosition);
		float avoidFactor = Mathf.Clamp(distToTarget / 450f, 0.15f, 1f);

		Vector2 desiredDir = targetDirection.Normalized() + avoid.Normalized() * avoidFactor * AvoidStrength;

		float targetRotation = desiredDir.Angle();
		float angleErr = Mathf.AngleDifference(Rotation, targetRotation);

		float kp = 100000f;
		float kd = 20000f;

		float torque = angleErr * kp - AngularVelocity * kd;
		ApplyTorque(torque);
	}
	private void HandleMovement(float dt)
	{
		float angleErr = Mathf.Abs(Mathf.AngleDifference(Rotation, targetDirection.Angle()));
		Vector2 forward = Vector2.Right.Rotated(Rotation);
		if(angleErr < Mathf.DegToRad(30))
		{
			if(LinearVelocity.Length() < IdleMaxMoveSpeed)
			{
				float thrustFactor = Mathf.Clamp(1f-angleErr / Mathf.Pi, 0f, 1f);
				ApplyForce(forward * currentThrust * thrustFactor);
			}
		}
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
					targetDirection = Vector2.Right.Rotated((float) GD.RandRange(0, Mathf.Tau));
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
			HandleIdle(dt);
		}
	}
	private void HandleAggro(float dt)
	{
		if (player == null)
		{
			GD.PrintErr("Gracza nie widzi enemy");
			return;
		}
		//strzelanie
		shootTimer += dt;
		if(shootTimer >= ShootCooldown)
		{
			shootTimer = 0f;
			ShootAt(player.GlobalPosition);
		}
	}
	private void HandleIdle(float dt)
	{
		idleMoveTimer += dt;

		if(GlobalPosition.DistanceTo(targetPosition) < TargetPositionMarginError)
		{
			SelectRandomTarget();
			GD.Print("nowy pkt");
			targetSpr.GlobalPosition = targetPosition;
		}
		
		targetDirection = (targetPosition - GlobalPosition).Normalized();

		if(idleMoveTimer >= IdleDirectionChangeTime)
		{
			idleMoveTimer = 0f;
			IdleDirectionChangeTime = (float) GD.RandRange(2.5f, 5.0f);

			//ciąg
			currentThrust = GD.Randf() > 0.10f ? IdleThrust : 0f;
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
	private void SelectRandomTarget()
	{
		Vector2 randomLocalPoint;
		for(int i=0; i<100; i++)
		{
			float angle = (float) GD.RandRange(0, Mathf.Tau);
			float distance = (float) GD.RandRange(TargetPositionMinDistance, TargetPositionMaxDistance);
			randomLocalPoint = Vector2.Right.Rotated(angle) * distance;

			if(HasLineOfSight(ToGlobal(randomLocalPoint)))
			{
				targetPosition = ToGlobal(randomLocalPoint);
				return;
			}
		}
		targetPosition = GlobalPosition;
	}
	private bool HasLineOfSight(Vector2 targetGlobal)
	{
		var spaceState = GetWorld2D().DirectSpaceState;

		var query = new PhysicsRayQueryParameters2D
		{
			From = GlobalPosition,
			To = targetGlobal,
			CollideWithBodies = true
		};

		var result = spaceState.IntersectRay(query);
		return result.Count == 0;
	}
}
/*
TODO:
-gówno
-upadek izraela
-przerobić to żeby używało navigational shitu (do asteroid dodać ustawianie navigational obstacle)
*/
//this code is actual cancer