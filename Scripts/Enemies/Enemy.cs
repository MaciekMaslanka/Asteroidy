using System.Formats.Tar;
using System.Numerics;
using Godot;
using Vector2 = Godot.Vector2;

public partial class Enemy : RigidBody2D
{
	public enum State { Idle, Aggro}

	[ExportCategory("AI")]
	private State currentState = State.Idle;
	[Export] private float DetectionRange = 2000f;
	[Export] private float LoseAggroTime = 5f;

	[ExportCategory("Context Map")]
	[Export] private int ContextMapResolution = 16;
	[Export] private float ContextMapRayDistance = 800f;
	[Export] private float InterestWeight = 1.0f;
	[Export] private float DangerWeight = 1.5f;
	private ContextMap contextMap;
	private Vector2 desiredDirection;

	[ExportCategory("Strzelanie")]
	[Export] private float ShootCooldown = 1.5f;
	[Export] private PackedScene BulletScene;
	private float shootTimer = 0f;

	[ExportCategory("RandomMovement")]
	[Export] private float TargetMaxDistance = 3000f;
	[Export] private float TargetMinDistance = 400f;
	[Export] private float TargetPositionMarginError = 200f;

	[ExportCategory("Idle")]
	[Export] private float IdleThrust = 18000f;
	[Export] private float IdleMaxMoveSpeed = 240f;
	[Export] private float IdleDirectionChangeTime = 10f;
	private float idleMoveTimer = 0f;

	[ExportCategory("Aggro")]
	[Export] private float AggroTurnSpeed = 4f;
	private float loseAggroTimer = 0f;

	[ExportCategory("HP")]
	[Export] private float MaxHealth = 1000f;
	private float currentHealth;
	
	//inne rzeczy (burdel)
	private Vector2 targetPosition = new();
	private Vector2 targetDirection = new();
	private float currentThrust = 0f;
	private PlayerScript player;
	private RayCast2D visionRay;

	public override void _Ready()
	{
		visionRay = GetNode<RayCast2D>("Sensors/VisionRay");

		contextMap = new(ContextMapResolution);

		player = (PlayerScript) GetTree().GetFirstNodeInGroup("Player");
		currentThrust = IdleThrust;

		SelectRandomTarget();
		targetDirection = (targetPosition - GlobalPosition).Normalized();
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
		float targetRotation = desiredDirection.Angle();
		GD.Print(Mathf.RadToDeg(targetRotation));
		float angleErr = Mathf.AngleDifference(Rotation, targetRotation);

		float kp = 100000f;
		float kd = 20000f;

		float torque = angleErr * kp - AngularVelocity * kd;
		ApplyTorque(torque);
	}
	private void HandleMovement(float dt)
	{
		float angleToDesired = Mathf.Abs(Mathf.AngleDifference(Rotation, desiredDirection.Angle()));
		Vector2 forward = Vector2.Right.Rotated(Rotation);

		float deviation = Mathf.Abs(Mathf.AngleDifference(desiredDirection.Angle(), targetDirection.Angle()));
		float dangerFactor = Mathf.Clamp(1f - (deviation / Mathf.Pi), 0.3f, 1f);

		if(angleToDesired < Mathf.DegToRad(30))
		{
			if(LinearVelocity.Length() < IdleMaxMoveSpeed)
			{
				float thrustFactor = Mathf.Clamp(1f - angleToDesired / Mathf.Pi, 0.3f, 1f);
				float finalThrust = currentThrust * thrustFactor * dangerFactor;
				ApplyForce(forward * finalThrust);
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
		if (GlobalPosition.DistanceTo(targetPosition) < TargetPositionMarginError)
		{
			SelectRandomTarget();
			targetDirection = (targetPosition - GlobalPosition).Normalized();
		}

		var spaceState = GetWorld2D().DirectSpaceState;
		contextMap.Update(GlobalPosition, targetDirection, spaceState, ContextMapRayDistance);

		desiredDirection = contextMap.GetSteeringDirection(InterestWeight, DangerWeight);
		currentThrust = IdleThrust;
	}
	private void SelectRandomTarget()
	{
		for (int i=0; i<50; i++)
		{
			float angle = (float) GD.RandRange(0, Mathf.Tau);
			float distance = (float) GD.RandRange(TargetMinDistance, TargetMaxDistance);

			Vector2 randomPoint =GlobalPosition + Vector2.Right.Rotated(angle) * distance;

			if(HasLineOfSight(randomPoint))
			{
				targetPosition = randomPoint;
				return;
			}
		}

		//jeśli nie znajdzie dobrego celu
		targetPosition = GlobalPosition;
	}
	private bool HasLineOfSight(Vector2 targetGlobal)
	{
		var spaceState = GetWorld2D().DirectSpaceState;

		var query = new PhysicsRayQueryParameters2D
		{
			From = GlobalPosition,
			To = targetGlobal,
			CollideWithBodies = true,
			Exclude = new Godot.Collections.Array<Rid> {GetRid()}
		};

		var result = spaceState.IntersectRay(query);
		return result.Count == 0;
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
/*
TODO:
-gówno
-upadek izraela
-poprawić unikanie i pozbyć się navagenta
*/
//this code is actual cancer