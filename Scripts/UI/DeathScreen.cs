using Godot;

public partial class DeathScreen : Control
{
	[Export] private float transitionDuration = 0.5f;
	[Export] private Button restartButton;
	[Export] private Button quitButton;
	private PlayerScript connectedPlayer;
	private Tween deathTween;

    public override void _Ready()
    {
		Visible = false;
		Modulate = Colors.Transparent;

		restartButton.Pressed += RestartGame;
		quitButton.Pressed += ExitGame;

        if(GameManager.Instance.Player != null)
			Init();
		else
			GameManager.Instance.PlayerReady += Init;
    }
	private void Init()
	{
		connectedPlayer = GameManager.Instance.Player;
		connectedPlayer.PlayerDied += HandlePlayerDeath;
	}
	private void HandlePlayerDeath()
	{
		Visible = true;

		deathTween?.Kill();

		deathTween = CreateTween();
		deathTween.TweenProperty(this, "modulate", Colors.White, transitionDuration);
	}
	private void RestartGame()
	{
		GameManager.Instance.RestartGame();
	}
	private void ExitGame()
	{
		GameManager.Instance.ExitGame();
	}
    public override void _ExitTree()
    {
		if(GameManager.Instance != null)
        	GameManager.Instance.PlayerReady -= Init;

		if(IsInstanceValid(connectedPlayer))
			connectedPlayer.PlayerDied -= HandlePlayerDeath;
    }
}
