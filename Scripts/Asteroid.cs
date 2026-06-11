using Godot;
using System;


public partial class Asteroid : StaticBody2D
{
	[Export] float BaseRadius = 100f;
	[Export] int PointsAmount = 60;
	[Export] float Amplitude = 0.5f;
	[Export] float NoiseScale = 1f;
	[Export] float Frequency = 1f;
	[Export] int Octaves = 4;
	[Export] PackedScene DynamicAsteroidScene;

	Polygon2D body;
	Polygon2D background;
	CollisionPolygon2D collider;
	Vector2[] currentShape;

	public override void _Ready()
	{
		body = GetNode<Polygon2D>("Polygon2D");
		background = GetNode<Polygon2D>("Polygon2DBackground");
		collider = GetNode<CollisionPolygon2D>("CollisionPolygon2D");
		GenerateShape();
	}
	private void UpdateShape(Vector2[] points, bool UpdateBackground = false)
	{
		currentShape = points;
		body.Polygon = currentShape;
		collider.Polygon = currentShape;
		if(UpdateBackground)
			background.Polygon = currentShape;
	}
	private void GenerateShape()
	{
		FastNoiseLite noise = new();
		noise.Seed = GD.RandRange(0, 999999);
		noise.Frequency = Frequency;
		noise.FractalOctaves = 4;
		Vector2[] points = new Vector2[PointsAmount];

		for(int i=0; i<PointsAmount; i++)
		{
			float angle = Mathf.Tau * i / PointsAmount;

			float noiseValue = noise.GetNoise2D(
				Mathf.Cos(angle) * NoiseScale,
				Mathf.Sin(angle) * NoiseScale
			);

			float radius = BaseRadius * (1f + noiseValue * Amplitude);
			points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
		}
		UpdateShape(points, UpdateBackground: true);

		float textureRotation =  (float) GD.RandRange(0d, Math.Tau);
		body.TextureRotation = textureRotation;
		background.TextureRotation = textureRotation;
	}
	public void DigAt(Vector2 point, float radius = 10f, int segments = 10)
	{
		Vector2 localPoint = ToLocal(point);
		var cutter = CreateCutter(localPoint, radius, segments);

		var result = Geometry2D.ClipPolygons(currentShape, cutter);
		if(result.Count == 0)
			return;
		else if (result.Count == 1)
		{
			UpdateShape(result[0]);
		}
		else
		{
			for(int i=0; i<result.Count; i++)
			{
				
			}
		}

	}
	private Vector2[] CreateCutter(Vector2 center, float radius, int segments)
	{
		var points = new Vector2[segments];
		for(int i=0; i<segments; i++)
		{
			float angle = Mathf.Tau * i / segments;
			points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
		}
		return points;
	}
}
