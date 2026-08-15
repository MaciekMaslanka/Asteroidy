using Godot;
using System;
using System.Collections.Generic;


public partial class Asteroid : RigidBody2D
{
	[Export] private AsteroidSettings Settings;
	[Export] private AsteroidShapeSettings ShapeSettings;
	[Export] float BaseRadius = 200f;
	[Export] int PointsAmount = 60;
	[Export] float Amplitude = 0.4f;
	[Export] float NoiseScale = 0.7f;
	[Export] float Frequency = 1f;
	[Export] int Octaves = 4;
	[Export] float MassDensity = 0.05f;
	public PackedScene AsteroidScene {private set; get;}

	[ExportCategory("Smoothing")]
	[Export] float minDistance = 12f;

	[ExportCategory("Ores")]
	[Export] PackedScene OreScene;
	[Export] private int OresMinAmount;
	[Export] private int OresMaxAmount;
	[Export] private float MinDistanceBetweenOres;
	[Export] private float OresGenerationOffset;
	[Export] private Godot.Collections.Array<OreRarity> OreRarities;
	public List<OreScript> ores {private set; get;} = new();

	//shape
	public Polygon2D body {private set; get;}
	private Polygon2D background;
	private CollisionPolygon2D collider;
	public Vector2[] currentShape {private set; get;}
	private bool hasCustomShape = false;
	
	public override void _Ready()
	{
		body = GetNode<Polygon2D>("Polygon2D");
		background = GetNode<Polygon2D>("Polygon2DBackground");
		collider = GetNode<CollisionPolygon2D>("CollisionPolygon2D");
		AsteroidScene = GD.Load<PackedScene>("res://Scenes/Asteroid.tscn");

		if(Settings != null)
		{
			ApplySettings();
		}
		
		if(!hasCustomShape)
		{
			GenerateShape();
			GenerateOres(OresMinAmount, OresMaxAmount, MinDistanceBetweenOres, OresGenerationOffset, OreRarities);
		}
		else
		{
			UpdateShape(currentShape);
			background.Visible = false;
		}
		UpdateMass();
	}
	public void SetSettings(AsteroidSettings settings, AsteroidShapeSettings shapeSettings)
	{
		Settings = settings;
		ShapeSettings = shapeSettings;
	}
	private void ApplySettings()
	{
		//visuals
		if(Settings.Texture != null)
			body.Texture = Settings.Texture;
			background.Texture = Settings.Texture;
			
		//asteroida
		BaseRadius = ShapeSettings.BaseRadius;
		PointsAmount = ShapeSettings.PointsAmount;
		Amplitude = ShapeSettings.Amplitude;
		NoiseScale = ShapeSettings.NoiseScale;
		Frequency = ShapeSettings.Frequency;
		Octaves = ShapeSettings.Octaves;
		MassDensity = ShapeSettings.MassDensity;

		//ore
		OresMinAmount = ShapeSettings.MinOreAmount;
		OresMaxAmount = ShapeSettings.MaxOreAmount;
		MinDistanceBetweenOres = Settings.MinDistanceBetweenOres;
		OreRarities = Settings.OreRarities;
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
		float MaxRadius = 0f;
		for(int i=0; i<PointsAmount; i++)
		{
			float angle = Mathf.Tau * i / PointsAmount;

			float noiseValue = noise.GetNoise2D(
				Mathf.Cos(angle) * NoiseScale,
				Mathf.Sin(angle) * NoiseScale
			);

			float radius = BaseRadius * (1f + noiseValue * Amplitude);
			if(radius > MaxRadius) MaxRadius = radius;

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
		smoothed.Add(currentShape[^1]); // ^ zwraca od konca tablicy (nie wiedziałem że takie coś istnieje)

		UpdateShape(smoothed.ToArray());
	}

	//rudy
	private void GenerateOres(int minAmount = 2, int maxAmount = 7, float minDistanceBetweenOres = 1, float oresGenerationOffset = 1, Godot.Collections.Array<OreRarity> rarity = null)
	{
		OreGenerator generator = new(this, OreScene, minAmount, maxAmount, oresGenerationOffset, minDistanceBetweenOres, rarity);
		generator.GenerateOres();
	}
	public void OnOreDestroyed(OreScript ore)
	{
		DiggingHandler digHandler = new DiggingHandler(this, ore);
		digHandler.OnOreDestroyed();

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
