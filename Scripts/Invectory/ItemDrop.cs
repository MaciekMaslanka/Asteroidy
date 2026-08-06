using Godot;

public partial class ItemDrop : RigidBody2D
{
	[Export] private Sprite2D itemIcon;
	[Export] private PanelContainer pickupIndicator;
	[Export] private Label pickupIndicatorLabel;
	private Vector2 pickupIndicatorOffset;
	private InvItem item;
	private int quantity;
	public bool IsMouseOver {private set; get;} = false;

    public override void _Ready()
	{
		DisablePickupIndicator();
		pickupIndicatorOffset = pickupIndicator.Position;

		MouseEntered += () => IsMouseOver = true;
		MouseExited += () => IsMouseOver = false;

		Rotation = (float) GD.RandRange(0, Mathf.Tau);

		ApplyTorque((float) GD.RandRange(10f, 100f));

		Vector2 randomDirection = Vector2.Right.Rotated((float) GD.RandRange(0, Mathf.Tau));
		randomDirection *= (float) GD.RandRange(1f, 10f);
		ApplyImpulse(randomDirection);
	}
    public override void _PhysicsProcess(double delta)
    {
        pickupIndicator.Position = pickupIndicatorOffset.Rotated(-Rotation);
		pickupIndicator.Rotation = -Rotation;
    }
	public void SetItem(InvItem item, int quantity)
	{
		this.item = item;
		this.quantity = quantity;
		itemIcon.Texture = item.Icon;
	}
	public void EnablePickupIndicator()
	{
		pickupIndicator.Visible = true;
		pickupIndicatorLabel.Text = $"{item.ItemName} ({quantity})";
	}
	public void DisablePickupIndicator()
	{
		pickupIndicator.Visible = false;
	}
	public bool Pickup()
	{
		int remainingAmount = GameManager.Instance.Player.CollectItem(item, quantity);
		if(remainingAmount <= 0)
		{
			QueueFree();
			return true;
		}
		else
		{
			quantity = remainingAmount;
			return false;
		}
	}
}