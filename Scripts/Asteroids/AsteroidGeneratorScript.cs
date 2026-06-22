using Godot;
using System;

public partial class AsteroidGeneratorScript : Node2D
{
	public override void _Ready()
	{
		Vector2 position = new Vector2(-500, 0);
		GenerateAstoroid(position);
	}
	private void GenerateAstoroid(Vector2 pos)
	{
		var asteroidScene = GD.Load<PackedScene>("res://Scenes/Asteroid.tscn");
		Asteroid asteroid = asteroidScene.Instantiate<Asteroid>();

		asteroid.Position = pos; 
		AddChild(asteroid);
	}
	public override void _Process(double delta)
	{
		
	}
}
