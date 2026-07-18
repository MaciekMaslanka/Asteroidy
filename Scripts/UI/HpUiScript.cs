using Godot;
using System;

public partial class HpUiScript : Control
{
	[Export] private TextureProgressBar hpBar;
	[Export] private TextureProgressBar shieldsBar;
	
	public override void _Ready()
	{
		if(GameManager.Instance.Player != null)
		{
			Init();
		}
		else
		{
			GameManager.Instance.PlayerReady += Init;
		}
	}
	private void Init()
	{
		var player = GameManager.Instance.Player;
		player.HealthChanged += OnHealthChanged;
		player.ShieldChanged += OnShieldsChanged;
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
