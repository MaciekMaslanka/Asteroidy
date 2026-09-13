using Godot;

public partial class MainMenu : Control
{
	[Export] private Button playButton;
	[Export] private Button exitButton;
	[Export] private PackedScene gameScene;
	[Export] private float transitionDuration = 0.25f;
	[Export] private ColorRect fadeRect;

    public override void _Ready()
    {
		GetTree().Paused = false;

        playButton.Pressed += PlayGame;
		exitButton.Pressed += ExitGame;
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
}
