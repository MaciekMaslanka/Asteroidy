using Godot;

public partial class Enemy : RigidBody2D, IDamagable
{
	//sygnały
	[Signal]
	public delegate void EnemyActivatedEventHandler(Enemy enemy);
	[Signal]
	public delegate void EnemyDeactivatedEventHandler(Enemy enemy);

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

	[ExportGroup("Chase")]
	[Export] private float chaseRange = 1500f;
	[Export] private float chasePredictionTime = 0.4f;

	[ExportGroup("Attack")]
	[Export] private float attackRange = 800f;
	[Export] private float preferedDistance = 650f;
	[Export] private float circleStrength = 1.6f;

	[ExportGroup("Search")]
	[Export] private float searchTime = 10f;
	[Export] private float searchRadius = 300f;

	[ExportCategory("Movement")]
	[Export] private float thrust = 25000f;
	[Export] private float maxSpeed = 300f;

	[ExportCategory("Dropy")]
	[Export] private float dropChance = 0.5f;
	[Export] private PackedScene dropItemScene;
	[Export] private InvItem[] possibleDrops;
	[Export] private int maxDropAmount = 5;

	//timery
	private float escapeTimer = 0f;
	private float newPatrolTargetTimer = 0f;
	private float searchTimer = 0f;

	//inne
	private PlayerScript player;
	private Vector2 targetPosition;
	private Vector2 desiredDirection;
	private Vector2 escapeDirection;
	private Vector2 lastKnownPlayerPosition;
	public State CurrentState {private set; get;} = State.Patrol;
	public bool SeesPlayer {private set; get;} = false;
	public bool IsOnScreen {private set; get;} = false;
	private VisibleOnScreenNotifier2D visibileNotifier;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;

		//ctx map
		contextMap = new(contextMapResolution);
		avoidanceShape = new CircleShape2D
		{
			Radius = 45f
		};

		targetPosition = SelectPatrolTarget();

		//hp
		currentHealth = MaxHealth;
		hpBar = GetNode<ProgressBar>("HpBar");
		hpBar.MaxValue = 100;
		hpBar.Value = hpBar.MaxValue;
		hpBarOffset = hpBar.Position;
		hpBar.Visible = false;

		Deactivate();

		if(GameManager.Instance.Player != null)
		{
			Init();
		}
		else
		{
			GameManager.Instance.PlayerReady += Init;
		}

		visibileNotifier = GetNode<VisibleOnScreenNotifier2D>("VisibleNotifier");
		visibileNotifier.ScreenEntered += () => IsOnScreen = true;
		visibileNotifier.ScreenExited += () => IsOnScreen = false;
	}
	private void Init()
	{
		player = GameManager.Instance.Player;
	}
    public override void _PhysicsProcess(double delta)
	{
		if(player == null)
			return;
			
		float dt = (float) delta;

		SeesPlayer = CanSeePlayer();

		UpdateState();

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
			DropItem();
			QueueFree();
			return;
		}
		hpBar.Value = currentHealth / MaxHealth * hpBar.MaxValue;
	}
	private void DropItem()
	{
		if(GD.Randf() > dropChance)
			return;

		if(dropItemScene == null)
			return;

		ItemDrop item = dropItemScene.Instantiate<ItemDrop>();

		int dropID = GD.RandRange(0, possibleDrops.Length - 1);
		int amount = GD.RandRange(1, maxDropAmount);
		item.SetItem(possibleDrops[dropID], amount);
		item.GlobalPosition = GlobalPosition;
		GetTree().CurrentScene.GetNode("ItemDrops").AddChild(item);
	}
	//-------------------------------------------------------------------------------------------
	//stany
	private void UpdateState()
	{
		if(player == null)
			return;

		if(SeesPlayer)
		{
			lastKnownPlayerPosition = player.GlobalPosition;
		}

		float distance = GlobalPosition.DistanceTo(player.GlobalPosition);

		switch(CurrentState)
		{
			case State.Patrol:
				if(SeesPlayer)
					CurrentState = State.Chase;
				break;

			case State.Chase:
				if(!SeesPlayer)
				{
					CurrentState = State.Search;
					searchTimer = 0f;
				}
				else if(distance <= attackRange)
				{
					CurrentState = State.Attack;
				}
				break;

			case State.Attack:
				if(!SeesPlayer)
				{
					CurrentState = State.Search;
				}
				else if (distance > attackRange * 1.2f)
				{
					CurrentState = State.Chase;
				}
				break;

			case State.Search:
				if(SeesPlayer)
				{
					CurrentState = State.Chase;
					searchTimer = 0f;
				}
				break;
		}
	}
	private void HandleState(float dt)
	{
		switch(CurrentState)
		{
			case State.Patrol:
				HandlePatrol(dt);
				break;
			case State.Search:
				HandleSearch(dt);
				break;
			case State.Chase:
				HandleChase();
				break;
			case State.Attack:
				HandleAttack(dt);
				break;
		}
		
	}
	private void HandlePatrol(float dt)
	{
		newPatrolTargetTimer += dt;

		if(newPatrolTargetTimer >= newTargetTime || GlobalPosition.DistanceTo(targetPosition) < targetMarginErr)
		{
			targetPosition = SelectPatrolTarget();
			newPatrolTargetTimer = 0f;
		}
		desiredDirection = (targetPosition - GlobalPosition).Normalized();
	}
	private void HandleSearch(float dt)
	{
		searchTimer += dt;

		desiredDirection = (lastKnownPlayerPosition - GlobalPosition).Normalized();

		if(searchTimer >= searchTime)
		{
			CurrentState = State.Patrol;
			searchTimer = 0f;
		}
	}
	private void HandleChase()
	{
		Vector2 predictedPosition = player.GlobalPosition + player.LinearVelocity * chasePredictionTime;
		targetPosition = predictedPosition;
		desiredDirection = (targetPosition - GlobalPosition).Normalized();
	}
	private void HandleAttack(float dt)
	{
		Vector2 toPlayer = player.GlobalPosition - GlobalPosition;
		float distance = toPlayer.Length();

		Vector2 direction = toPlayer.Normalized();

		if(distance < preferedDistance * 1.4f)
		{
			Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
			direction = (direction + perpendicular * circleStrength).Normalized();
		}
		desiredDirection = direction;
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
	private bool CanSeePlayer()
	{
		if(player == null) 
			return false;

		if(GlobalPosition.DistanceTo(player.GlobalPosition) > chaseRange)
			return false;

		var query = new PhysicsRayQueryParameters2D
		{
			From = GlobalPosition,
			To = player.GlobalPosition,
			CollideWithBodies = true,
			CollisionMask = 0b101101,
			Exclude = new Godot.Collections.Array<Rid> {GetRid()}
		};

		var result = GetWorld2D().DirectSpaceState.IntersectRay(query);

		if(result.Count == 0)
			return false;

		return result["collider"].AsGodotObject() is PlayerScript;
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
		thrustFactor = Mathf.Max(0f, thrustFactor);

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
	//-------------------------------------------------------------------------------------------
	//aktywacja / deaktywacja
	public void Activate()
	{
		CallDeferred(MethodName.SetPhysicsProcess, true);
		CallDeferred(MethodName.Set, "freeze", false);
		EmitSignal(SignalName.EnemyActivated, this);
	}
	public void Deactivate()
	{
		CallDeferred(MethodName.SetPhysicsProcess, false);
		CallDeferred(MethodName.Set, "freeze", true);
		EmitSignal(SignalName.EnemyDeactivated, this);
	}
}