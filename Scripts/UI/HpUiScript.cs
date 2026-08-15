using Godot;

public partial class HpUiScript : Control
{
	[Export] private TextureProgressBar hpBar;
	[Export] private TextureProgressBar shieldsBar;
	[Export] private Texture2D radioactiveForeground;
	[Export] private TextureRect radioactiveSign;
	private Texture2D normalForeGround;
	
	public override void _Ready()
	{
		normalForeGround = shieldsBar.TextureOver;

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
		GameManager.Instance.PlayerEnteredRadioactiveBiome += OnRadioactiveBiomeEnter;
		GameManager.Instance.PlayerExitedRadioactiveBiome += OnRadioactiveBiomeExit;
	}
	private void OnHealthChanged(float current, float max)
	{
		hpBar.Value = (current/max) * 100f;
	}
	private void OnShieldsChanged(float current, float max)
	{
		shieldsBar.Value = (current/max) * 100f;
	}
	private void OnRadioactiveBiomeEnter()
	{
		shieldsBar.TextureOver = radioactiveForeground;
		radioactiveSign.Visible = true;
	}
	private void OnRadioactiveBiomeExit()
	{
		shieldsBar.TextureOver = normalForeGround;
		radioactiveSign.Visible = false;
	}
}
