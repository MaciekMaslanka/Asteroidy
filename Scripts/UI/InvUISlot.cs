using Godot;

public partial class InvUISlot : Panel
{
	[Signal]
	public delegate void SlotPressedEventHandler(InventorySlot slot);

	[Export] private Sprite2D itemIcon;
	[Export] private Label amountLabel;
	private InventorySlot currentSlot;
	public void UpdateSlot(InventorySlot slot)
	{
		currentSlot = slot;
		if(slot.Item == null)
		{
			itemIcon.Visible = false;
			amountLabel.Visible = false;
		}
		else
		{
			itemIcon.Visible = true;
			amountLabel.Visible = true;
			itemIcon.Texture = slot.Item.Icon;
			amountLabel.Text = slot.Amount.ToString();
		}
	}

    public override void _GuiInput(InputEvent @event)
	{
		if(@event is InputEventMouseButton mouse &&
		mouse.ButtonIndex == MouseButton.Left &&
		mouse.Pressed)
		{
			if(currentSlot != null)
				EmitSignal(SignalName.SlotPressed, currentSlot);
		}
	}
}
