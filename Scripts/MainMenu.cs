using System.ComponentModel;
using Godot;

public partial class MainMenu : Control
{
	[Export] private Label highScoreLabel;
	[Export] private Button playButton;
	[Export] private Button steeringButton;
	[Export] private Button exitButton;
	[Export] private PackedScene gameScene;
	[Export] private float transitionDuration = 0.25f;
	[Export] private ColorRect fadeRect;

	[Export] private Button closeSteeringButton;
	[Export] private Control steeringScreen;

	[Export] private LanguagesManager languagesManager;

    public override void _Ready()
    {
		GetTree().Paused = false;
		Input.MouseMode = Input.MouseModeEnum.Visible;

        playButton.Pressed += PlayGame;
		exitButton.Pressed += ExitGame;

		steeringButton.Pressed += () => SwitchSteeringScreen(true);
		closeSteeringButton.Pressed += () => SwitchSteeringScreen(false);

		steeringScreen.Visible = false;

		languagesManager.LanguageChanged += UpdateHighScoreLabel;
		UpdateHighScoreLabel();
    }
	private void PlayGame()
	{
		Input.MouseMode = Input.MouseModeEnum.Hidden;

		var tween = CreateTween();
		tween.TweenProperty(fadeRect, "color:a", 1f, transitionDuration);
		tween.Finished += () => GetTree().ChangeSceneToPacked(gameScene);
	}
	private void ExitGame()
	{
		GetTree().Quit();
	}
	private void SwitchSteeringScreen(bool newState)
	{
		steeringScreen.Visible = newState;
	}
	private void UpdateHighScoreLabel()
	{
		if(GameManager.Instance?.HighScore > 0)
		{
			highScoreLabel.Text = $"{Tr("UI_HIGH_SCORE")} {GameManager.Instance.HighScore}";
			highScoreLabel.Visible = true;
		}
		else
		{
			highScoreLabel.Visible = false;
		}
	}
    public override void _ExitTree()
    {
        if(languagesManager != null)
		{
			languagesManager.LanguageChanged -= UpdateHighScoreLabel;
		}
    }
}
