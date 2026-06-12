using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class Asteroid : RigidBody2D
{
	[Export] float BaseRadius = 100f;
	[Export] int PointsAmount = 60;
	[Export] float Amplitude = 0.5f;
	[Export] float NoiseScale = 1f;
	[Export] float Frequency = 1f;
	[Export] int Octaves = 4;
	[Export] public PackedScene AsteroidScene { get; set; }

	[ExportCategory("Smoothing")]
	[Export] float minDistance = 12f;
	[Export] float maxAngleDeviation = 0.35f;
	Polygon2D body;
	Polygon2D background;
	CollisionPolygon2D collider;
	Vector2[] currentShape;

	public override void _Ready()
	{
		body = GetNode<Polygon2D>("Polygon2D");
		background = GetNode<Polygon2D>("Polygon2DBackground");
		collider = GetNode<CollisionPolygon2D>("CollisionPolygon2D");

		if (AsteroidScene == null)
		{
			AsteroidScene = GD.Load<PackedScene>("res://Scripts/Asteroid.cs");
		}
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
			SmoothShape();
		}
		else
		{
			HandleMultipleFragments(result);
		}

	}
	private void HandleMultipleFragments(Godot.Collections.Array<Vector2[]> result)
	{
		UpdateShape(result[0]);
		for(int i=1; i<result.Count; i++)
		{
			Vector2[] fragmentPoints = (Vector2[]) result[i];
			CreateFragment(fragmentPoints);
		}
	}
	private void CreateFragment(Vector2[] points)
	{
		Asteroid fragment = AsteroidScene.Instantiate<Asteroid>();
		fragment.Position = this.Position;
    	fragment.Rotation = this.Rotation;

		fragment.UpdateShape(points, true);

		GetParent().AddChild(fragment);
		//TODO zrobić to poprawnie
		//dostane autyzmu
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
	private void SmoothShape()
	{
		if(currentShape.Length < 6) return;

		List<Vector2> smoothed = new();
		smoothed.Add(currentShape[0]);

		for(int i=1; i<currentShape.Length - 1; i++)
		{
			Vector2 prev = currentShape[i-1];
			Vector2 current = currentShape[i];
			Vector2 next = currentShape[i+1];

			if(current.DistanceTo(prev) < minDistance)
				continue;

			smoothed.Add(current);
		}
		smoothed.Add(currentShape[^1]); // ^ zwraca od konca tablicy

		UpdateShape(smoothed.ToArray());
	}
}
