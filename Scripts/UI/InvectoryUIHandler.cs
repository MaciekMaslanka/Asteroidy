using Godot;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;

public partial class InvectoryUIHandler : Control
{
	private bool isOpen = false;
	private Invectory inv;
	private Godot.Collections.Array<Node> slots;
	public override void _Ready()
	{
		inv = GD.Load<Invectory>("res://Resources/Invectory/PlayerInv.tres");
		slots = GetNode<GridContainer>("NinePatchRect/GridContainer").GetChildren();

		inv.UpdateInvectoryUI += UpdateSlots;

		UpdateSlots();
		CloseInv();
	}

	public override void _Process(double delta)
	{
		if(Input.IsActionJustPressed("openInv"))
		{
			if(isOpen)
				CloseInv();
			else
				OpenInv();
			
		}
	}
	private void CloseInv()
	{
		isOpen = false;
		Visible = false;
	}
	private void OpenInv()
	{
		isOpen = true;
		Visible = true;
	}
	private void UpdateSlots()
	{
		for(int i=0; i<slots.Count; i++)
		{
			var temp = (InvUISlot) slots[i];
			temp.UpdateSlot(inv.slots[i]);
		}
	}
}
