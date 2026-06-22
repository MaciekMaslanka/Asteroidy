using Godot;
using System;
using System.Collections.Generic;
using System.Dynamic;


public partial class Asteroid : RigidBody2D
{
	[Export] float BaseRadius = 100f;
	[Export] int PointsAmount = 60;
	[Export] float Amplitude = 0.5f;
	[Export] float NoiseScale = 1f;
	[Export] float Frequency = 1f;
	[Export] int Octaves = 4;
	[Export] float MassDensity = 1f;
	public PackedScene AsteroidScene {private set; get;}
	[Export] PackedScene OreScene;

	[ExportCategory("Smoothing")]
	[Export] float minDistance = 12f;
	public Polygon2D body {private set; get;}
	Polygon2D background;
	CollisionPolygon2D collider;
	public Vector2[] currentShape {private set; get;}
	private bool hasCustomShape = false;
	public List<OreScript> ores {private set; get;} = new();

	public override void _Ready()
	{
		body = GetNode<Polygon2D>("Polygon2D");
		background = GetNode<Polygon2D>("Polygon2DBackground");
		collider = GetNode<CollisionPolygon2D>("CollisionPolygon2D");
		AsteroidScene = GD.Load<PackedScene>("res://Scenes/Asteroid.tscn");
		
		if(!hasCustomShape)
		{
			GenerateShape();
			GenerateOres();
		}
		else
		{
			UpdateShape(currentShape);
			background.Visible = false;
		}
		UpdateMass();
	}

	//kształt
	public void SetCustomShape(Vector2[] points)
	{
		currentShape = points;
		hasCustomShape = true;
	}
	public void UpdateShape(Vector2[] points, bool UpdateBackground = false)
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
	private void SmoothShape()
	{
		if(currentShape.Length < 6) return;

		List<Vector2> smoothed = new();
		smoothed.Add(currentShape[0]);

		for(int i=1; i<currentShape.Length - 1; i++)
		{
			Vector2 prev = currentShape[i-1];
			Vector2 current = currentShape[i];

			if(current.DistanceTo(prev) < minDistance)
				continue;

			smoothed.Add(current);
		}
		smoothed.Add(currentShape[^1]); // ^ zwraca od konca tablicy

		UpdateShape(smoothed.ToArray());
	}

	//rudy
	private void GenerateOres(int minAmount = 2, int maxAmount = 7)
	{
		OreGenerator generator = new(this, OreScene, minAmount, maxAmount, 30f, 100f);
		generator.GenerateOres();
	}
	public void OnOreDestroyed(OreScript ore)
	{
		DiggingHandler digHandler = new DiggingHandler(this, ore);
		digHandler.OnOreDestroyed();

		if(GetTree().GetFirstNodeInGroup("Player") is PlayerScript player)
		{
			player.CollectItem(ore.item);
		}

		ores.Remove(ore);
		ore.QueueFree();
		SmoothShape();
	}

	//kopanie
	public void DigAt(Vector2 point, float radius = 10f, int segments = 10)
	{
		DiggingHandler digHandler = new DiggingHandler(ToLocal(point), radius, segments, this);
		digHandler.NormalDigging();
		SmoothShape();
	}

	public void UpdateMass()
	{
		CenterOfMass = PolygonUtils.GetPolygonCenter(body.Polygon);
		Mass = PolygonUtils.CalculatePolygonArea(body.Polygon) * MassDensity;
	}
}
