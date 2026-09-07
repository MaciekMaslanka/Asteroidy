using Godot;

public partial class MainMenu : Control
{
	[Export] private Button playButton;
	[Export] private Button exitButton;
	[Export] private PackedScene mainLVL;

    public override void _Ready()
    {
        playButton.Pressed += PlayGame;
		exitButton.Pressed += ExitGame;
    }
	private void PlayGame()
	{
		GetTree().ChangeSceneToPacked(mainLVL);
	}
	private void ExitGame()
	{
		GetTree().Quit();
	}
}
