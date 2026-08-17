using Godot;

public partial class Enemy : RigidBody2D, IDamagable
{
	public enum State { Idle, Aggro}

	[ExportCategory("HP")]
	[Export] private float MaxHealth = 1000f;
	private float currentHealth;
	private Vector2 hpBarOffset;
	private ProgressBar hpBar;

	[ExportCategory("AI")]
	[ExportGroup("Context Map")]
	[Export] private int contextMapResolution = 32;
	[Export] private float interestWeight = 1f;
	[Export] private float dangerWeight = 3f;
	[Export] private float contextMapRayLength = 1000f;
	[Export] private CircleShape2D avoidanceShape;
	private ContextMap contextMap;

	[ExportCategory("Movement")]
	[Export] private float thrust = 25000f;
	[Export] private float maxSpeed = 300f;
	//inne
	private PlayerScript player;
	private Vector2 desiredDirection;
	private State currentState = State.Idle;

	public override void _Ready()
	{
		contextMap = new(contextMapResolution);
		avoidanceShape = new();
		avoidanceShape.Radius = 45f;

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

		desiredDirection = (player.GlobalPosition - GlobalPosition).Normalized();

		contextMap.Update(
			GlobalPosition, 
			desiredDirection, 
			GetWorld2D().DirectSpaceState, 
			contextMapRayLength,
			avoidanceShape,
			GetRid()
		);
		desiredDirection = contextMap.GetSteeringDirection(interestWeight, dangerWeight);

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
		currentState = State.Aggro;
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
		thrustFactor = Mathf.Max(0, thrustFactor);

		if(LinearVelocity.Length() < maxSpeed)
		{
			ApplyForce(forward * thrust * thrustFactor);
		}
	}
}