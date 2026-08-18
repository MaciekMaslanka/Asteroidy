using System.ComponentModel;
using System.Reflection.Metadata;
using Godot;

public partial class Enemy : RigidBody2D, IDamagable
{
	public enum State { 
		Patrol,
		Search,
		Chase,
		Attack
	}

	[ExportCategory("HP")]
	[Export] private float MaxHealth = 1000f;
	private float currentHealth;
	private Vector2 hpBarOffset;
	private ProgressBar hpBar;

	[ExportCategory("AI")]
	[ExportGroup("Context Map")]
	[Export] private int contextMapResolution = 32;
	[Export] private float interestWeight = 1f;
	[Export] private float dangerWeight = 1.7f;
	[Export] private float contextMapRayLength = 500f;
	private CircleShape2D avoidanceShape;
	private ContextMap contextMap;

	[ExportGroup("Patrol")]
	[Export] private float minTargetDistance = 500f;
	[Export] private float maxTargetDistance = 2000f;
	[Export] private float targetMarginErr = 100f;
	[Export] private float newTargetTime = 30f;

	[ExportCategory("Movement")]
	[Export] private float thrust = 25000f;
	[Export] private float maxSpeed = 300f;

	//timery
	private float escapeTimer = 0f;
	private float newPatrolTargetTimer = 0f;

	//inne
	private PlayerScript player;
	private Vector2 targetPosition;
	private Vector2 desiredDirection;
	private Vector2 escapeDirection;
	private State currentState = State.Patrol;

	//debug
	[Export] private Sprite2D test;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;

		//ctx map
		contextMap = new(contextMapResolution);
		avoidanceShape = new CircleShape2D
		{
			Radius = 45f
		};

		//hp
		currentHealth = MaxHealth;
		hpBar = GetNode<ProgressBar>("HpBar");
		hpBar.MaxValue = 100;
		hpBar.Value = hpBar.MaxValue;
		hpBarOffset = hpBar.Position;
		hpBar.Visible = false;

		if(GameManager.Instance.Player != null)
		{
			Init();
		}
		else
		{
			GameManager.Instance.PlayerReady += Init;
		}
	}
	private void Init()
	{
		player = GameManager.Instance.Player;
	}
    public override void _PhysicsProcess(double delta)
	{
		float dt = (float) delta;

		//ucieczka po kolizji
		if(escapeTimer > 0f)
		{
			escapeTimer -= dt;
			desiredDirection = escapeDirection;
		}
		else
		{
			HandleState(dt);

			contextMap.Update(
				GlobalPosition, 
				desiredDirection, 
				GetWorld2D().DirectSpaceState, 
				contextMapRayLength,
				avoidanceShape,
				GetRid()
			);

			desiredDirection = contextMap.GetSteeringDirection(interestWeight, dangerWeight);
		}

		HandleRotation(dt);
		HandleMovement(dt);

		hpBar.Position = hpBarOffset.Rotated(-Rotation);
		hpBar.Rotation = -Rotation;
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
	//-------------------------------------------------------------------------------------------
	//stany
	private void HandleState(float dt)
	{
		HandlePatrol(dt);
	}
	private void HandlePatrol(float dt)
	{
		newPatrolTargetTimer += dt;

		if(newPatrolTargetTimer >= newTargetTime || GlobalPosition.DistanceTo(targetPosition) < targetMarginErr)
		{
			targetPosition = SelectPatrolTarget();
			newPatrolTargetTimer = 0f;
		}
		
		desiredDirection = (targetPosition-GlobalPosition).Normalized();
	}
	private void HandleSearch()
	{
		
	}
	private void HandleChase()
	{
		
	}
	private void HandleAttack()
	{
		
	}
	//-------------------------------------------------------------------------------------------
	//helpery do state
	private Vector2 SelectPatrolTarget()
	{
		for(int i=0; i<30; i++)
		{
			float angle = (float) GD.RandRange(Rotation-Mathf.Pi/2, Rotation+Mathf.Pi/2);
			float distance = (float) GD.RandRange(minTargetDistance, maxTargetDistance);

			Vector2 canidate = GlobalPosition + Vector2.Right.Rotated(angle) * distance;
			if(HasLineOfSight(canidate, true))
			{
				return canidate;
			}
		}
		return GlobalPosition;
	}
	private bool HasLineOfSight(Vector2 target, bool useShapeCast = false)
	{
		var spaceState = GetWorld2D().DirectSpaceState;
		
		if(useShapeCast)
		{
			var query = new PhysicsShapeQueryParameters2D
			{
				Shape = avoidanceShape,
				Transform = new Transform2D(0f, GlobalPosition),
				Motion = target - GlobalPosition,
				CollideWithBodies = true,
				CollisionMask = 0b101100, //kolizja z asteroidami, oreami i borderem
				Exclude = new Godot.Collections.Array<Rid> {GetRid()}
			};

			var result = spaceState.CastMotion(query);
			if(result[0] == 1f)
				return true;
			else
				return false;
		}
		else
		{
			var query = new PhysicsRayQueryParameters2D
			{
				From = GlobalPosition,
				To = target,
				CollideWithBodies = true,
				Exclude = new Godot.Collections.Array<Rid> {GetRid()}
			};

			var result = spaceState.IntersectRay(query);
			return result.Count == 0;
		}
	}
	//-------------------------------------------------------------------------------------------
	//Movement
	private void HandleRotation(float dt)
	{
		float targetRotation = desiredDirection.Angle();
		float angleError = Mathf.AngleDifference(Rotation, targetRotation);

		float torque = angleError * 300000f - AngularVelocity * 30000f;

		torque = Mathf.Clamp(torque, -1000000f, 1000000f);
		ApplyTorque(torque);
	}
	private void HandleMovement(float dt)
	{
		Vector2 forward = Vector2.Right.Rotated(Rotation);

		float angleDifference = Mathf.Abs(
			Mathf.AngleDifference(
				Rotation, 
				desiredDirection.Angle()
			)
		);

		float thrustFactor = Mathf.Cos(angleDifference);
		thrustFactor = Mathf.Max(0.30f, thrustFactor);

		if(LinearVelocity.Length() < maxSpeed)
		{
			ApplyForce(forward * thrust * thrustFactor);
		}
	}
	private void OnBodyEntered(Node body)
	{
		if(body is Asteroid asteroid)
		{
			Vector2 newEscapeDirection = (GlobalPosition - asteroid.GlobalPosition).Normalized();
			escapeDirection = newEscapeDirection;
			escapeTimer = 0.5f;
		}
	}
}