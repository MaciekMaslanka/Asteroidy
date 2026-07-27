using Godot;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

public partial class PlayerScript : RigidBody2D, IDamagable
{
	//sygnały
	[Signal]
	public delegate void ShieldChangedEventHandler(float currentShields, float maxShields);
	[Signal]
	public delegate void HealthChangedEventHandler(float currentHealth, float maxHealth);

	//hp
	[ExportCategory("HP")]
	[Export] float MaxHP = 500f;
	[Export] float MaxShields = 500f;
	[Export] float ShieldsRegenDelay = 20f;
	[Export] float ShieldsRegenRate = 20f;
	private float currentHP;
	private float currentShields;
	private float timeSinceLastHit = 0f;

	//rotacja
	[ExportCategory("Rotacja")]
	[Export] float RotationForce = 12f;
	[Export] private float MaxAngularVelocity = 4.5f;
    [Export] private float AngularDamping = 8f;

	//ruch
	[ExportCategory("Ruch")]
	[Export] private float ThrustForce = 850f;
    [Export] private float MaxLinearVelocity = 420f;
    [Export] private float LinearDamping = 1.2f;

	//narzedzia-wspolne
	[ExportCategory("Narzedzia")]
	[Export] private float toolRotationSpeed = 10f;
	[Export] private float toolRotationLimit = 135f; //potem zamieniana na radiany
	private enum ToolsEnum
	{
		None,
		DiggingTool,
		GunTool
	}
	private ToolsEnum currentTool = ToolsEnum.DiggingTool;
	Node2D toolsContainer = null;

	//narzedzie do kopania
	[ExportCategory("Kopanie")]
	[Export] private float diggerRange = 100f; //potem ujemny bo dziwne rzeczy się dzieją z raycastem
	[Export] private float diggerSpeed = 2f;
	private float diggingTimer = 0f;
	private RayCast2D diggerRay;
	private Line2D diggerLine;
	private Node2D diggerContainer;
	private bool isDiggerActive = false;

	//narzedzie do strzelania
	[ExportCategory("Strzelanie")]
	[Export] private float firingCd = 0.5f;
	private float firingTimer = 0f;
	private Node2D gunContainer;
	private Node2D bulletSpawn;
	[Export] PackedScene BulletScene;

	//eq
	[ExportCategory("EQ")]
	[Export] public Inventory Inventory {get; private set;}

	//inne
	private bool isInRadioactiveBiome;

    public override void _Ready()
	{
		//ważne!!- nie zmieniać nazw nodeów, bo się spieprzy
		//hp
		currentHP = MaxHP;
		currentShields = MaxShields;
		EmitSignal(SignalName.HealthChanged, currentHP, MaxHP);
		EmitSignal(SignalName.ShieldChanged, currentShields, MaxShields);

		//narzedzia
		toolsContainer = GetNode<Node2D>("ToolContainer");
		toolRotationLimit = Mathf.DegToRad(toolRotationLimit);

		//digger
		diggerRange = -diggerRange;
		diggerRay = toolsContainer.GetNode<RayCast2D>("DiggingTool/RayCast2D");
		diggerLine = toolsContainer.GetNode<Line2D>("DiggingTool/Line2D");
		diggerContainer = toolsContainer.GetNode<Node2D>("DiggingTool");

		//broń
		gunContainer = toolsContainer.GetNode<Node2D>("GunTool");
		bulletSpawn = toolsContainer.GetNode<Node2D>("GunTool/BulletsSpawn");

		diggerContainer.Visible = true;
		gunContainer.Visible = false;

		//ustawienia fizyki
		GravityScale = 0f;
		AngularDamp = AngularDamping;
		LinearDamp = LinearDamping;

		if(Inventory == null)
		{
			Inventory = new Inventory();
		}

		GameManager.Instance.PlayerEnteredRadioactiveBiome += OnRadioactiveBiomeEnter;
		GameManager.Instance.PlayerExitedRadioactiveBiome += OnRadioactiveBiomeExit;
		GameManager.Instance.RegisterPlayer(this);
		GameManager.Instance.RegisterInventory(Inventory);
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float) delta;
		HandleRotation(dt);
		HandleMovement(dt);
		RotateTool(dt);
		HandleToolChanges();
		HandleMouseInput(dt);
		HandleShieldsRegen(dt);

		if(diggingTimer > 0)
		{
			diggingTimer -= (float) delta;
		}
		if(firingTimer > 0)
		{
			firingTimer -= (float) delta;
		}
	}

	//-------------------------------------------------------------------------
	//damage
	public void TakeDamage(float amount)
	{
		timeSinceLastHit = 0f;
		if(currentShields > 0f)
		{
			TakeShieldsDamage(amount);
		}
		else
		{
			TakeHPDamage(amount);
		}
	}
	private void TakeShieldsDamage(float amount)
	{
		currentShields -= amount;
		if(currentShields <= 0f)
		{
			float overflowDamage = -currentShields;
			currentShields = 0f;

			TakeHPDamage(overflowDamage);
		}
		EmitSignal(SignalName.ShieldChanged, currentShields, MaxShields);
	}
	private void TakeHPDamage(float amount)
	{
		currentHP -= amount;
		EmitSignal(SignalName.HealthChanged, currentHP, MaxHP);
		if(currentHP <= 0f)
		{
			Die();
		}
	}
	private void Die()
	{
		GD.Print("Zdechłeś cwelu");
	}
	private void HandleShieldsRegen(float dt)
	{
		if(!isInRadioactiveBiome)
			timeSinceLastHit += dt;

		if(timeSinceLastHit >= ShieldsRegenDelay && currentShields < MaxShields)
		{
			currentShields += ShieldsRegenRate * dt;
			currentShields = Mathf.Min(currentShields, MaxShields);

			EmitSignal(SignalName.ShieldChanged, currentShields, MaxShields);
		}
	}

	//-------------------------------------------------------------------------
	//ruch
	private void HandleRotation(float dt)
	{
		float input = Input.GetAxis("rotateLeft", "rotateRight");
		if(input != 0)
		{
			AngularVelocity = Mathf.MoveToward(AngularVelocity, MaxAngularVelocity * input, dt*RotationForce);
		}
	}
	private void HandleMovement(float dt)
	{
		float throttle = Input.GetAxis("moveUp", "moveDown");

		if(throttle != 0)
		{
			Vector2 direction = new Vector2(0, throttle).Rotated(Rotation);
			ApplyForce(direction * ThrustForce);
		}

		if(LinearVelocity.Length() > MaxLinearVelocity)
		{
			LinearVelocity = LinearVelocity.Normalized() * MaxLinearVelocity;
		}
	}

	//-------------------------------------------------------------------------
	//tool
	private void RotateTool(float dt)
	{
		Vector2 direction = GetGlobalMousePosition() - toolsContainer.GlobalPosition;
		float globalAngle = direction.Angle();
		float targetAngle = globalAngle - GlobalRotation + Mathf.Pi / 2;
		targetAngle = Mathf.Wrap(targetAngle, -Mathf.Pi, Mathf.Pi); //przylimituj kąt do +- 180 stopni żeby nie działy się funky rzeczy
		targetAngle = Mathf.Clamp(targetAngle, -toolRotationLimit, toolRotationLimit);
		toolsContainer.Rotation = Mathf.Lerp(toolsContainer.Rotation, targetAngle, toolRotationSpeed*dt);

		//rotacja w godocie to dziadostwo
	}
	private void HandleToolChanges()
	{
		if(Input.IsActionJustPressed("diggingToolSelect"))
		{
			currentTool = ToolsEnum.DiggingTool;
			diggerContainer.Visible = true;
			gunContainer.Visible = false;
		}
		if(Input.IsActionJustPressed("gunToolSelect"))
		{
			currentTool = ToolsEnum.GunTool;
			diggerContainer.Visible = false;
			gunContainer.Visible = true;
		}
	}
	private void HandleMouseInput(float dt)
	{
		switch(currentTool)
		{
			case ToolsEnum.DiggingTool:
				if(Input.IsActionPressed("mouseLeft"))
					ActivateDigger(dt);
				else
					DeactivateDigger(dt);
				break;
			case ToolsEnum.GunTool:
				if(Input.IsActionPressed("mouseLeft"))
				{
					Shoot();
				}
				break;
		}

	}
	private void ActivateDigger(float dt)
	{
		if(!isDiggerActive)
		{
			isDiggerActive = true;
			diggerRay.Enabled = true;
			diggerLine.Visible = true;
		}
		diggerRay.TargetPosition = new Vector2(0, diggerRange); //range lasera
		diggerRay.ForceRaycastUpdate();

		if(diggerRay.IsColliding())
		{
			Vector2 hitPoint = diggerRay.GetCollisionPoint();

			if(diggerRay.GetCollider() is Asteroid asteroid)
			{
				if(diggingTimer <= 0)
				{
					asteroid.DigAt(hitPoint, 10f, 10);
					diggingTimer = 1 / diggerSpeed;
				}
			}
			else if (diggerRay.GetCollider() is OreScript ore)
			{
				if(diggingTimer <= 0)
				{
					ore.TakeDamage(diggerSpeed);
					diggingTimer = 1 / diggerSpeed;
				}
			}

			diggerLine.ClearPoints();
			diggerLine.AddPoint(Vector2.Zero);
			diggerLine.AddPoint(diggerLine.ToLocal(hitPoint));
		}
		else
		{
			diggerLine.ClearPoints();
			diggerLine.AddPoint(Vector2.Zero);
			diggerLine.AddPoint(new Vector2(0, diggerRange));
		}
	}
	private void DeactivateDigger(float dt)
	{
		if(isDiggerActive)
		{
			isDiggerActive = false;
			diggerRay.Enabled = false;
			diggerLine.Visible = false;
		}
	}
	private void Shoot()
	{
		if(firingTimer <= 0)
		{
			firingTimer = firingCd;
			var bullet = BulletScene.Instantiate<Bullet>();
			bullet.GlobalPosition = bulletSpawn.GlobalPosition;
			bullet.Rotation = toolsContainer.GlobalRotation - Mathf.Pi/2;
			bullet.AddCollisionExceptionWith(this);
			bullet.SetCollisionMaskValue(2, true); //kolizja z enemy
			GetTree().CurrentScene.AddChild(bullet);
		}
	}
	public void CollectItem(InvItem item, int amount)
	{
		Inventory.AddItem(item, amount);
	}
	//-------------------------------------------------------------------------
	//biome specific 
	private void OnRadioactiveBiomeEnter()
	{
		isInRadioactiveBiome = true;
		currentShields = 0f;
		timeSinceLastHit = 0f;
		EmitSignal(SignalName.ShieldChanged, currentShields, MaxShields);
	}
	private void OnRadioactiveBiomeExit()
	{
		isInRadioactiveBiome = false;
		EmitSignal(SignalName.ShieldChanged, currentShields, MaxShields);
	}
}