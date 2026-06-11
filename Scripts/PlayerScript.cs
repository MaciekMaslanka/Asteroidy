using Godot;
using System;

public partial class PlayerScript : CharacterBody2D
{
	//rotacja
	[ExportCategory("Rotacja")]
	[Export] private float RotationSpeed = 5f;
	[Export] private float MaxRotationSpeed = 5f;
	[Export] private float RotationBreakingSpeed = 3f;
	private float currentRotation = 0f;

	//ruch
	[ExportCategory("Ruch")]
	[Export] private float Acceleration = 800f;
	[Export] private float MaxMovementSpeed = 300f;
	[Export] private float BrakingSpeed = 600f;

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
	private bool isDiggerActive = false;

	//narzedzie do strzelania

    public override void _Ready()
	{
		//ważne!!- nie zmieniać nazw nodeów, bo się spieprzy
		//narzedzia
		toolsContainer = GetNode<Node2D>("ToolContainer");
		toolRotationLimit = Mathf.DegToRad(toolRotationLimit);

		//digger
		diggerRange = -diggerRange;
		diggerRay = toolsContainer.GetNode<Node2D>("DiggingTool").GetNode<RayCast2D>("RayCast2D");
		diggerLine = toolsContainer.GetNode<Node2D>("DiggingTool").GetNode<Line2D>("Line2D");
	}

	public override void _PhysicsProcess(double delta)
	{
		DoMovement(delta);
		RotateTool(delta);
		HandleMouseInput(delta);

		if(diggingTimer > 0)
		{
			diggingTimer -= (float) delta;
		}
	}
	private void DoMovement(double delta)
	{
		float dt = (float)delta;

		//rotacja
		float leftRight = Input.GetAxis("rotateLeft", "rotateRight");

		if (leftRight != 0)
		//przyspieszanie rotacji
		{
			currentRotation += leftRight * RotationSpeed * dt;
			currentRotation = Math.Clamp(currentRotation, -MaxRotationSpeed, MaxRotationSpeed);
		}
		else if (currentRotation != 0)
		//hamowanie rotacji
		{
			currentRotation = Mathf.MoveToward(currentRotation, 0f, RotationBreakingSpeed*dt);
		}
		Rotation += currentRotation * dt;

		//ruch
		float throttle = Input.GetAxis("moveUp", "moveDown");

		Vector2 velocity = Velocity;

		if (throttle != 0f)
		{
			Vector2 thrustDirection = new Vector2(0, throttle).Rotated(Rotation);
			velocity += thrustDirection * Acceleration * dt;
		}
		else
		{
			velocity = velocity.MoveToward(Vector2.Zero, BrakingSpeed * dt);
		}

		if (velocity.Length() > MaxMovementSpeed)
		{
			velocity = velocity.Normalized() * MaxMovementSpeed;
		}

		Velocity = velocity;
		MoveAndSlide();
	}
	private void RotateTool(double delta)
	{
		float dt = (float) delta;
		Vector2 direction = GetGlobalMousePosition() - toolsContainer.GlobalPosition;
		float globalAngle = direction.Angle();
		float targetAngle = globalAngle - GlobalRotation + Mathf.Pi / 2;
		targetAngle = Mathf.Wrap(targetAngle, -Mathf.Pi, Mathf.Pi); //przylimituj kąt do +- 180 stopni żeby nie działy się funky rzeczy
		targetAngle = Mathf.Clamp(targetAngle, -toolRotationLimit, toolRotationLimit);
		toolsContainer.Rotation = Mathf.Lerp(toolsContainer.Rotation, targetAngle, toolRotationSpeed*dt);

		//rotacja w godocie to dziadostwo
	}
	private void HandleMouseInput(double delta)
	{
		switch(currentTool)
		{
			case ToolsEnum.DiggingTool:
				if(Input.IsActionPressed("mouseLeft"))
					ActivateDigger(delta);
				else
					DeactivateDigger(delta);
				break;
		}

	}
	private void ActivateDigger(double delta)
	{
		float dt = (float) delta;
		if(!isDiggerActive)
		{
			isDiggerActive = true;
			diggerRay.Enabled = true;
			diggerLine.Visible = true;
		}
		diggerRay.TargetPosition = new Vector2(0, diggerRange); //range lasera

		if(diggerRay.IsColliding())
		{
			Vector2 hitPoint = diggerRay.GetCollisionPoint();

			//tutaj kod kolizji
			if(diggerRay.GetCollider() is Asteroid asteroid)
			{
				if(diggingTimer <= 0)
				{
					asteroid.DigAt(hitPoint, 10f, 10);
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
	private void DeactivateDigger(double delta)
	{
		if(isDiggerActive)
		{
			isDiggerActive = false;
			diggerRay.Enabled = false;
			diggerLine.Visible = false;
		}
	}

}
