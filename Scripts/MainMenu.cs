using Godot;

public partial class MainMenu : Control
{
	[Export] private Button playButton;
	[Export] private Button exitButton;
	[Export] private PackedScene gameScene;

    public override void _Ready()
    {
        playButton.Pressed += PlayGame;
		exitButton.Pressed += ExitGame;
    }
	private void PlayGame()
	{
		GetTree().ChangeSceneToPacked(gameScene);
	}
	private void ExitGame()
	{
		GetTree().Quit();
	}
}
