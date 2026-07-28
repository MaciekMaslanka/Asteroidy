using System.ComponentModel;
using Godot;

public partial class ItemContextMenu : PanelContainer
{
	[Export] private Label itemName;
	[Export] private Button useButton;
	[Export] private Button dropButton;

	private InventorySlot currentSlot;
    public override void _Ready()
	{
		useButton.Pressed += UseItem;
		dropButton.Pressed += DropItem;
		Close();
	}
	public void Open(InventorySlot slot, Vector2 mousePos)
	{
		if(slot == null) return;

		currentSlot = slot;
		itemName.Text = slot.Item.ItemName;

		if(currentSlot.Item.CanUse())
		{
			useButton.Visible = true;
		}
		if(currentSlot.Item.CanDrop())
		{
			dropButton.Visible = true;
		}
		GlobalPosition = mousePos - new Vector2(Size.X / 4, 0);
		Visible = true;
	}

	private void UseItem()
	{
		currentSlot.Item.Use();
		Close();
	}
	private void DropItem()
	{
		currentSlot.Item.Drop();
		Close();
	}
	public void Close()
	{
		Visible = false;
		useButton.Visible = false;
		dropButton.Visible = false;
	}
}
