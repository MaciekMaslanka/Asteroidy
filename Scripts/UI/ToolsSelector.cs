using Godot;
using Godot.Collections;

public partial class ToolsSelector : Control
{
	[Export] private NinePatchRect gunRect;
	[Export] private NinePatchRect diggerRect;
	private Dictionary<ToolsEnum, NinePatchRect> toolRects;

	[Export] private Texture2D normalBg;
	[Export] private Texture2D selectedBg;

	private PlayerScript connectedPlayer;
	private NinePatchRect selectedRect;

    public override void _Ready()
    {
		toolRects = new Dictionary<ToolsEnum, NinePatchRect>
		{
			{ToolsEnum.DiggingTool, diggerRect},
			{ToolsEnum.GunTool, gunRect}
		};

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
		connectedPlayer = GameManager.Instance.Player;
		connectedPlayer.ToolChanged += SetNewToolDisplay;
		
		SetNewToolDisplay(GameManager.Instance.Player.CurrentTool);
	}
	private void SetNewToolDisplay(ToolsEnum newTool)
	{
		if(selectedRect != null)
			selectedRect.Texture = normalBg;

		selectedRect = toolRects[newTool];
		toolRects[newTool].Texture = selectedBg;
	}
    public override void _ExitTree()
    {
        if(GameManager.Instance != null)
		{
			GameManager.Instance.PlayerReady -= Init;
		}

		if(connectedPlayer != null)
		{
			connectedPlayer.ToolChanged -= SetNewToolDisplay;
		}
    }
}
