using Godot;
using System;

public partial class InvUISlot : Panel
{
	[Export] private Sprite2D itemIcon;
	[Export] private Label amountLabel;
	public void UpdateSlot(InvSlot slot)
	{
		if(slot.item == null)
		{
			itemIcon.Visible = false;
			amountLabel.Visible = false;
		}
		else
		{
			itemIcon.Visible = true;
			amountLabel.Visible = true;
			itemIcon.Texture = slot.item.Icon;
			amountLabel.Text = slot.amount.ToString();
		}
	}
}
