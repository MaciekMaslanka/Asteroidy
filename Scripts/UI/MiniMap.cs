using Godot;
using System.Collections.Generic;

public partial class MiniMap : Control
{
	[Export] private SubViewport subViewport;
	[Export] private Camera2D miniCam;
	[Export] private TextureRect minimapImage;
	[Export] private float zoom;
	[Export] private TextureRect playerIcon;
	[Export] private PackedScene enemyIconScene;
	private PlayerScript player;
	private Dictionary<Enemy, TextureRect> enemyIcons = new();
	private List<Enemy> deadEnemiesTrash = new();

    public override void _Ready()
    {
        subViewport.World2D = GetViewport().World2D;

		miniCam.Enabled = true;
		miniCam.MakeCurrent();
		miniCam.Zoom = new Vector2(zoom, zoom);

		GameManager.Instance.RegisterMinimap(this);

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
		if(player == null)
			return;
		
		miniCam.GlobalPosition = player.GlobalPosition;
		minimapImage.Texture = subViewport.GetTexture();
		playerIcon.Rotation = player.GlobalRotation - Mathf.Pi / 2;
		UpdateIndicators();
	}
	private void UpdateIndicators()
	{
		deadEnemiesTrash.Clear();

		foreach(var pair in enemyIcons)
		{
			Enemy enemy = pair.Key;
			TextureRect icon = pair.Value;

			if(!IsInstanceValid(enemy))
			{
				deadEnemiesTrash.Add(enemy);
				if(IsInstanceValid(icon))
				{
					icon.QueueFree();
				}
				continue;
			}

			Vector2 position = WorldToMinimap(enemy.GlobalPosition);
			icon.Position = position - icon.Size / 2f;
			icon.Rotation = enemy.GlobalRotation;
		}

		for(int i=0; i<deadEnemiesTrash.Count; i++)
		{
			enemyIcons.Remove(deadEnemiesTrash[i]);
		}
	}
	private Vector2 WorldToMinimap(Vector2 worldPosition)
	{
		Vector2 viewportSize = subViewport.Size;
		Vector2 displaySize = minimapImage.Size;

		Vector2 relativePosition = worldPosition - miniCam.GlobalPosition;

		relativePosition *= miniCam.Zoom;

		Vector2 viewportPosition = viewportSize / 2f + relativePosition;

		Vector2 scale = displaySize / viewportSize;
		
		return viewportPosition * scale;
	}
	public void AddEnemy(Enemy enemy)
	{
		if(!enemyIcons.ContainsKey(enemy))
		{
			TextureRect enemyIcon = enemyIconScene.Instantiate<TextureRect>();
			minimapImage.AddChild(enemyIcon);
			
			enemyIcons.Add(enemy, enemyIcon);
		}
	}
	public void RemoveEnemy(Enemy enemy)
	{
		if(enemyIcons.TryGetValue(enemy, out TextureRect icon))
		{
			if(IsInstanceValid(icon))
			{
				icon.QueueFree();
			}
			enemyIcons.Remove(enemy);
		}
	}
}
