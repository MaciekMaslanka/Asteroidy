using Godot;
using Vector2 = Godot.Vector2;

public partial class Enemy : RigidBody2D, IDamagable
{
	public enum State { Idle, Aggro}

	[ExportCategory("AI")]
	private State currentState = State.Idle;
	[Export] private float DetectionRange = 2000f;

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
	[Export] private float TargetPositionMarginError = 300f;

	[ExportCategory("Idle")]
	[Export] private float IdleThrust = 18000f;
	[Export] private float IdleMaxMoveSpeed = 240f;
	[Export] private float IdleDirectionChangeTime = 10f;
	private float idleMoveTimer = 0f;

	[ExportCategory("Aggro")]
	[Export] private float AggroThrust = 25000f;
	[Export] private float PreferredDistance = 650f;
	[Export] private float CircleStrength = 1.6f;
	[Export] private float AggroMaxSpeed = 320f;
	[Export] private float LoseAggroTime = 10f;
	private float loseAggroTimer = 0f;

	[ExportCategory("HP")]
	[Export] private float MaxHealth = 1000f;
	private float currentHealth;
	private Vector2 hpBarOffset;
	private ProgressBar hpBar;
	
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

		currentHealth = MaxHealth;
		hpBar = GetNode<ProgressBar>("HpBar");
		hpBar.MaxValue = 100;
		hpBar.Value = hpBar.MaxValue;
		hpBarOffset = hpBar.Position;
		hpBar.Visible = false;
	}
    public override void _PhysicsProcess(double delta)
	{
		float dt = (float) delta;

		UpdateState(dt);
		HandleBehaviour(dt);
		HandleRotation(dt);
		HandleMovement(dt);

		hpBar.Position = hpBarOffset.Rotated(-Rotation);
		hpBar.Rotation = -Rotation;
	}
	private void HandleRotation(float dt)
	{
		float targetRotation = desiredDirection.Angle();
		float angleErr = Mathf.AngleDifference(Rotation, targetRotation);

		float kp = 160000f;
		float kd = 38000f;

		float torque = angleErr * kp - AngularVelocity * kd;
		torque = Mathf.Clamp(torque, -850000f, 850000f);
		ApplyTorque(torque);
	}
	private void HandleMovement(float dt)
	{
		float maxSpeed = (currentState == State.Aggro) ? AggroMaxSpeed : IdleMaxMoveSpeed;

		float angleToDesired = Mathf.Abs(Mathf.AngleDifference(Rotation, desiredDirection.Angle()));
		Vector2 forward = Vector2.Right.Rotated(Rotation);

		if(angleToDesired < Mathf.DegToRad(30))
		{
			if(LinearVelocity.Length() < maxSpeed)
			{
				float thrustFactor = Mathf.Clamp(1f - angleToDesired / Mathf.Pi, 0.35f, 1f);
				ApplyForce(forward * currentThrust * thrustFactor);
			}
		}
	}
	private void UpdateState(float dt)
	{
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

		Vector2 toPlayer = player.GlobalPosition - GlobalPosition;
		float distanceToPlayer = toPlayer.Length();

		Vector2 predictedPos = player.GlobalPosition + player.LinearVelocity * 0.4f;
		Vector2 directionToPlayer = (predictedPos - GlobalPosition).Normalized();

		//krążenie wokół gracza
		if(distanceToPlayer < PreferredDistance * 1.4f)
		{
			Vector2 perpendicular = new Vector2(-directionToPlayer.Y, directionToPlayer.X);
			directionToPlayer = (directionToPlayer + perpendicular * CircleStrength).Normalized();
		}

		targetDirection = directionToPlayer;

		//ciąg
		if(distanceToPlayer > PreferredDistance)
		{
			currentThrust = AggroThrust;
		}
		else
		{
			currentThrust = AggroThrust * 0.65f;
		}

		//unikanie
		var spaceState = GetWorld2D().DirectSpaceState;
		contextMap.Update(GlobalPosition, targetDirection, spaceState, ContextMapRayDistance);
		desiredDirection = contextMap.GetSteeringDirection(InterestWeight, DangerWeight);

		//strzelanie
		shootTimer += dt;
		if(shootTimer >= ShootCooldown && distanceToPlayer < DetectionRange * 0.9 && CanSeePlayer())
		{
			shootTimer = 0f;
			ShootAt(predictedPos);
		}
	}
	private void HandleIdle(float dt)
	{
		idleMoveTimer += dt;
		if (GlobalPosition.DistanceTo(targetPosition) < TargetPositionMarginError || idleMoveTimer >= IdleDirectionChangeTime)
		{
			idleMoveTimer = 0;
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
	public void TakeDamage(float damage)
	{
		hpBar.Visible = true;
		currentHealth -= damage;
		if(currentHealth <= 0)
		{
			QueueFree();
			return;
		}
		hpBar.Value = currentHealth / MaxHealth * hpBar.MaxValue;
	}
}

/*
TODO:
-gówno
-upadek izraela
-poprawić unikanie i pozbyć się navagenta
*/
//this code is actual cancer