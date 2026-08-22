using Godot;

public partial class EnemyGun : Sprite2D
{
	[Export] private float rotationSpeed = 5f;
	[Export] private float maxAngle = 135f;

	[ExportCategory("Shoot the shields")]
	[Export] private float cooldown = 1.5f;
	[Export] private PackedScene bulletScene;

	private PlayerScript player;
	private Enemy enemy;
	private Marker2D muzzle;

	private float shootTimer = 0f;

    public override void _Ready()
    {
        enemy = GetParent<Enemy>();
		muzzle = GetNode<Marker2D>("Muzzle");

		enemy.EnemyActivated += () => CallDeferred(MethodName.SetPhysicsProcess, true);
		enemy.EnemyDeactivated += () => CallDeferred(MethodName.SetPhysicsProcess, false);

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
		if(player == null)
			return;

		float dt = (float) delta;

		HandleRotation(dt);

		shootTimer -= dt;

		if(shootTimer <= 0f && CanShoot())
		{
			Shoot();
			shootTimer = cooldown;
		}
	}
	private void HandleRotation(float dt)
	{
		float targetAngle = 0f;

		if(enemy.SeesPlayer)
		{
			Vector2 toPlayer = player.GlobalPosition - GlobalPosition;
			
			float globalAngle = toPlayer.Angle();
			targetAngle = Mathf.AngleDifference(enemy.GlobalRotation, globalAngle);
			targetAngle = Mathf.Clamp(targetAngle, -Mathf.DegToRad(maxAngle), Mathf.DegToRad(maxAngle));
		}
		Rotation = Mathf.Lerp(Rotation, targetAngle, rotationSpeed * dt);
	}

	private bool CanShoot()
	{
		Vector2 toPlayer = player.GlobalPosition - muzzle.GlobalPosition;

		float angle = Mathf.Abs(Mathf.AngleDifference(muzzle.GlobalRotation, toPlayer.Angle()));

		return angle < Mathf.DegToRad(5f) && enemy.SeesPlayer;
	}
	private void Shoot()
	{
		Bullet bullet = bulletScene.Instantiate<Bullet>();

		bullet.GlobalPosition = muzzle.GlobalPosition;
		bullet.GlobalRotation = muzzle.GlobalRotation;

		bullet.AddCollisionExceptionWith(enemy);

		GetTree().CurrentScene.AddChild(bullet);
	}
}
