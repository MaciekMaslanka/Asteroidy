using Godot;
using System;

public partial class HpUiScript : Control
{
	[Export] private TextureProgressBar hpBar;
	[Export] private TextureProgressBar shieldsBar;
	
	public override void _Ready()
	{
		var player = GetTree().GetFirstNodeInGroup("Player") as PlayerScript;
		
		if(player != null)
		{
			player.HealthChanged += OnHealthChanged;
			player.ShieldChanged += OnShieldsChanged;
		}
	}
	private void OnHealthChanged(float current, float max)
	{
		hpBar.Value = (current/max) * 100f;
	}
	private void OnShieldsChanged(float current, float max)
	{
		shieldsBar.Value = (current/max) * 100f;
	}
}
