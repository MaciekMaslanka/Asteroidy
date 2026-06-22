using Godot;
using System;
using System.IO.IsolatedStorage;

public partial class InvUIHandler : Control
{
	private bool isOpen = false;
	public override void _Ready()
	{
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
}
