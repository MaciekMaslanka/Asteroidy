using Godot;

public partial class PauseScript : Control
{
	[Export] private Button rescumeButton;
	[Export] private Button restartButton;
	[Export] private Button steeringButton;
	[Export] private Button quitToMenuButton;
	[Export] private Button quitToDesktopButton;

	[Export] private Control steeringScreen;
	[Export] private Button closeSteeringScreenButton;
    public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		GameManager.Instance.GamePaused += ShowPauseMenu;
		GameManager.Instance.GameUnpaused += HidePauseMenu;

		rescumeButton.Pressed += RescumeGame;
		restartButton.Pressed += RestartGame;
		steeringButton.Pressed += () => SwitchSteeringScreen(true);
		quitToMenuButton.Pressed += ReturnToMenu;
		quitToDesktopButton.Pressed += QuitGame;

		closeSteeringScreenButton.Pressed += () => SwitchSteeringScreen(false);

		Visible = false;
		steeringScreen.Visible = false;
	}

	private void ShowPauseMenu()
	{
		Visible = true;
	}
	private void HidePauseMenu()
	{
		Visible = false;
		SwitchSteeringScreen(false);
	}
	private void RescumeGame()
	{
		GameManager.Instance.UnpauseGame();
	}
	private void RestartGame()
	{
		GameManager.Instance.RestartGame();
	}
	private void SwitchSteeringScreen(bool newState)
	{
		steeringScreen.Visible = newState;
	}
	private void ReturnToMenu()
	{
		GameManager.Instance.QuitToMenu();
	}
	private void QuitGame()
	{
		GameManager.Instance.ExitGame();
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
