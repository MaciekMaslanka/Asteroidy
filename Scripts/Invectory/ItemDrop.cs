using System.Dynamic;
using Godot;

public partial class ItemDrop : RigidBody2D
{
	[Export] private Sprite2D itemIcon;
	[Export] private PanelContainer pickupIndicator;
	private InvItem item;
	private int quantity;
	public bool IsMouseOver {private set; get;} = false;

    public override void _Ready()
	{
		MouseEntered += () => {
			IsMouseOver = true;
			GD.Print("mouse over");
		};
		MouseExited += () =>
		{
			IsMouseOver = false;
			GD.Print("mouse nicht over");
		};

		Rotation = (float) GD.RandRange(0, Mathf.Tau);

		ApplyTorque((float) GD.RandRange(10f, 100f));

		Vector2 randomDirection = Vector2.Right.Rotated((float) GD.RandRange(0, Mathf.Tau));
		randomDirection *= (float) GD.RandRange(1f, 10f);
		ApplyImpulse(randomDirection);
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
	}
	public void DisablePickupIndicator()
	{
		pickupIndicator.Visible = false;
	}
}