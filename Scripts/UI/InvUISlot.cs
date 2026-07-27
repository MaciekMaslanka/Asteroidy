using Godot;

public partial class InvUISlot : Panel
{
	[Export] private Sprite2D itemIcon;
	[Export] private Label amountLabel;
	public void UpdateSlot(InventorySlot slot)
	{
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
}
