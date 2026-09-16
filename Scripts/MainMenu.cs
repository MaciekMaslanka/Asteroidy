using Godot;

public partial class MainMenu : Control
{
	[Export] private Button playButton;
	[Export] private Button steeringButton;
	[Export] private Button exitButton;
	[Export] private PackedScene gameScene;
	[Export] private float transitionDuration = 0.25f;
	[Export] private ColorRect fadeRect;

	[Export] private Button closeSteeringButton;
	[Export] private Control steeringScreen;

    public override void _Ready()
    {
		GetTree().Paused = false;
		Input.MouseMode = Input.MouseModeEnum.Visible;

        playButton.Pressed += PlayGame;
		exitButton.Pressed += ExitGame;

		steeringButton.Pressed += () => SwitchSteeringScreen(true);
		closeSteeringButton.Pressed += () => SwitchSteeringScreen(false);

		steeringScreen.Visible = false;
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
}
