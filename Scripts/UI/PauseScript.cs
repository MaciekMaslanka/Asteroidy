using Godot;

public partial class PauseScript : Control
{
    public override void _Ready()
	{
		GameManager.Instance.GamePaused += () => Visible = true;
		GameManager.Instance.GameUnpaused += () => Visible = false;
	}
}
