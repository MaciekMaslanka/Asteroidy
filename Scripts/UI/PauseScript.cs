using Godot;

public partial class PauseScript : Control
{
	[Export] private Button rescumeButton;
	[Export] private Button restartButton;
	[Export] private Button quitButton;
	
    public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		GameManager.Instance.GamePaused += ShowPauseMenu;
		GameManager.Instance.GameUnpaused += HidePauseMenu;

		rescumeButton.Pressed += RescumeGame;
		restartButton.Pressed += RestartGame;
		quitButton.Pressed += QuitGame;

		Visible = false;
	}

	private void ShowPauseMenu()
	{
		Visible = true;
	}
	private void HidePauseMenu()
	{
		Visible = false;
	}
	private void RescumeGame()
	{
		GameManager.Instance.UnpauseGame();
	}
	private void RestartGame()
	{
		GameManager.Instance.Unregister();
		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
	}
	private void QuitGame()
	{
		GetTree().Quit();
	}

    public override void _ExitTree()
    {
        if(GameManager.Instance != null)
		{
			GameManager.Instance.GamePaused -= ShowPauseMenu;
			GameManager.Instance.GameUnpaused -= HidePauseMenu;
		}
    }
}
