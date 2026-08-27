using Godot;
using System;

public partial class MiniMap : Control
{
	[Export] private SubViewport subViewport;
	[Export] private Camera2D miniCam;
	[Export] private TextureRect minimapImage;
	[Export] private float zoom;
	[Export] private uint canvasCullMask = 0b1;
	private PlayerScript player;

    public override void _Ready()
    {
        subViewport.World2D = GetViewport().World2D;

		miniCam.Enabled = true;
		miniCam.MakeCurrent();
		miniCam.Zoom = new Vector2(zoom, zoom);

		subViewport.CanvasCullMask = canvasCullMask;
		
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
		player = GameManager.Instance.Player;
	}
    public override void _PhysicsProcess(double delta)
	{
		miniCam.GlobalPosition = player.GlobalPosition;
		minimapImage.Texture = subViewport.GetTexture();
	}
}
