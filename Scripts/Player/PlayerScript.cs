using Godot;
using System;
using System.Collections.Generic;

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
	private float currentHp;
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
	private bool isSteeringLocked = false;

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

	//pickupowanie itemów
	[ExportCategory("Pickups")]
	[Export] private Area2D pickupDetector;
	private List<ItemDrop> pickableDrops = new();
	private ItemDrop selectedItemDrop;
	[Export] public ItemDropSpawner DropSpawner {private set; get;}

	//particle
	[ExportCategory("Particle")]
	[Export] private PackedScene hitParticles;
	[Export] private float particlesMinImpactSpeed = 400f;
	//inne
	private bool isInRadioactiveBiome;
	private Area2D enemyActivationArea;
	private Area2D enemyDeactivationArea;
	private EIndicatorsManager indicatorsManager;
	private MiniMap miniMap;

    public override void _Ready()
	{
		//ważne!!- nie zmieniać nazw nodeów, bo się spieprzy
		//hp
		currentHp = MaxHP;
		currentShields = MaxShields;
		EmitSignal(SignalName.HealthChanged, currentHp, MaxHP);
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
			throw new InvalidOperationException("Inventory nie zostało skonfigurowane");
		}

		GameManager.Instance.PlayerEnteredRadioactiveBiome += OnRadioactiveBiomeEnter;
		GameManager.Instance.PlayerExitedRadioactiveBiome += OnRadioactiveBiomeExit;
		GameManager.Instance.RegisterPlayer(this);
		GameManager.Instance.RegisterInventory(Inventory);

		pickupDetector.BodyEntered += OnPickupEnteredArea;
		pickupDetector.BodyExited += OnPickupExitedArea;

		enemyActivationArea = GetNode<Area2D>("EnemyActivationArea");
		enemyActivationArea.BodyEntered += ActivateEnemy;

		enemyDeactivationArea = GetNode<Area2D>("EnemyDeactivationArea");
		enemyDeactivationArea.BodyExited += DeactivateEnemy;


		if(GameManager.Instance.EnemyIndicatorsManager != null)
		{
			indicatorsManager = GameManager.Instance.EnemyIndicatorsManager;
		}
		else
		{
			GameManager.Instance.EIndicatorsManagerReady += () => 
				indicatorsManager = GameManager.Instance.EnemyIndicatorsManager;
		}

		if(GameManager.Instance.Minimap != null)
		{
			miniMap = GameManager.Instance.Minimap;
		}
		else
		{
			GameManager.Instance.MinimapReady += () => 
				miniMap = GameManager.Instance.Minimap;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float) delta;

		if(!isSteeringLocked)
		{
			HandleRotation(dt);
			HandleMovement(dt);
			RotateTool(dt);
			HandleToolChanges();
			HandleMouseInput(dt);
			HandleShieldsRegen(dt);
			HandleOtherInputs(dt);
		}
		
		

		HandlePickupIndicator();

		if(diggingTimer > 0)
			diggingTimer -= (float) delta;
		
		if(firingTimer > 0)
			firingTimer -= (float) delta;
	}
    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        for(int i=0; i < state.GetContactCount(); i++)
		{
			if(state.GetContactColliderObject(i) is Node2D collider)
			{
				Vector2 myVelocity = state.GetContactLocalVelocityAtPosition(i);
				Vector2 otherVelocity = state.GetContactColliderVelocityAtPosition(i);
				Vector2 relativeVelocity = myVelocity - otherVelocity;

				if(relativeVelocity.LengthSquared() >= particlesMinImpactSpeed * particlesMinImpactSpeed)
				{
					SpawnParticles(state.GetContactColliderPosition(i));
				}
			}
		}
    }

	//-------------------------------------------------------------------------
	//damage
	public bool Heal(float amount)
	{
		if(currentHp == MaxHP) 
			return false;

		currentHp = Mathf.Min(currentHp + amount, MaxHP);
		EmitSignal(SignalName.HealthChanged, currentHp, MaxHP);
		return true;
	}
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
		currentHp -= amount;
		if(currentHp <= 0f)
		{
			currentHp = 0f;
			Die();
		}
		EmitSignal(SignalName.HealthChanged, currentHp, MaxHP);
		
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

	public void LockSteering(bool newState = false)
	{
		isSteeringLocked = newState;
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
					SpawnParticles(hitPoint);
				}
			}
			else if (diggerRay.GetCollider() is OreScript ore)
			{
				if(diggingTimer <= 0)
				{
					ore.TakeDamage(diggerSpeed);
					diggingTimer = 1 / diggerSpeed;
					SpawnParticles(hitPoint);
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
	public int CollectItem(InvItem item, int amount)
	{
		return Inventory.AddItem(item, amount);
	}
	private void SpawnParticles(Vector2 pos)
	{
		var particles = hitParticles.Instantiate<HitSparks>();
		particles.GlobalPosition = pos;
		GetParent().AddChild(particles);
	}
	//-------------------------------------------------------------------------
	//pickups
	private void OnPickupEnteredArea(Node2D pickup)
	{
		if(pickup is ItemDrop itemdrop)
		{
			if(!pickableDrops.Contains(itemdrop))
			{
				pickableDrops.Add(itemdrop);
			}
		}
	}
	private void OnPickupExitedArea(Node2D pickup)
	{
		if(pickup is ItemDrop itemDrop)
		{
			itemDrop?.DisablePickupIndicator();
			pickableDrops.Remove(itemDrop);

			if(selectedItemDrop == itemDrop)
			{
				selectedItemDrop = null;
			}
		}
	}
	private ItemDrop GetItemUnderMouse()
	{
		foreach(var item in pickableDrops)
		{
			if(!IsInstanceValid(item)) 
				continue;

			if(item.IsMouseOver)
			{
				return item;
			}
		}
		return null;
	}
	private ItemDrop GetClosestItem()
	{
		ItemDrop closest = null;
		float closestDist = float.MaxValue;

		foreach(var pickup in pickableDrops)
		{
			if(!IsInstanceValid(pickup))
				continue;

			if(closest == null)
			{
				closest = pickup;
				closestDist = GlobalPosition.DistanceTo(closest.GlobalPosition);
				continue;
			}

			float distanceToPlayer = GlobalPosition.DistanceTo(pickup.GlobalPosition);
			if(distanceToPlayer < closestDist)
			{
				closest = pickup;
				closestDist = distanceToPlayer;
				continue;
			}
		}

		return closest;
	}
	private void HandlePickupIndicator()
	{
		pickableDrops.RemoveAll(drop => !IsInstanceValid(drop));
		
		if(pickableDrops.Count == 0)
		{
			selectedItemDrop?.DisablePickupIndicator();
			selectedItemDrop = null;
			return;
		}

		ItemDrop newClosest = GetItemUnderMouse();

		if(newClosest == null)
			newClosest = GetClosestItem();

		if(newClosest != selectedItemDrop)
		{
			selectedItemDrop?.DisablePickupIndicator();
			selectedItemDrop = newClosest;
			newClosest?.EnablePickupIndicator();
		}
	}
	private void HandleOtherInputs(float dt)
	{
		if(Input.IsActionJustPressed("pickupItem") && IsInstanceValid(selectedItemDrop))
		{
			if(selectedItemDrop.Pickup()) //jeśli itemek się usunie
			{
				pickableDrops.Remove(selectedItemDrop);
				selectedItemDrop = null;
			}
		}
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
	//-------------------------------------------------------------------------
	//enemy
	private void ActivateEnemy(Node2D body)
	{
		if(body is Enemy enemy)
		{
			enemy.Activate();
			indicatorsManager.AddEnemy(enemy);
			miniMap.AddEnemy(enemy);
		}
	}
	private void DeactivateEnemy(Node2D body)
	{
		if(body is Enemy enemy)
		{
			enemy.Deactivate();
			indicatorsManager.RemoveEnemy(enemy);
			miniMap.RemoveEnemy(enemy);
		}
	}
}