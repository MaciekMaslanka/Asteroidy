using Godot;
using System;

public partial class OreScript : RigidBody2D
{
	[Export] private float MaxHealth = 50f;
    public float CurrentHealth { get; private set; }

    private Polygon2D shape;
	private CollisionPolygon2D collider;

    public override void _Ready()
    {
        shape = GetNode<Polygon2D>("Polygon2D");
        collider = GetNode<CollisionPolygon2D>("CollisionPolygon2D");

        CurrentHealth = MaxHealth;
		GenerateShape();
    }

    public void TakeDamage(float amount)
    {
        CurrentHealth -= amount;

        if (CurrentHealth <= 0)
        {
            QueueFree();
        }
    }
    public void GenerateShape(float baseRadius = 35f, float amplitude = 0.4f)
    {
        var noise = new FastNoiseLite();
        noise.Seed = GD.RandRange(0, 99999);
        noise.Frequency = 0.8f;

        int pointCount = 24;
        var points = new Vector2[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            float angle = Mathf.Tau * i / pointCount;
            float noiseValue = noise.GetNoise2D(
                Mathf.Cos(angle) * 2f,
                Mathf.Sin(angle) * 2f
            );

            float radius = baseRadius * (1f + noiseValue * amplitude);
            points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        shape.Polygon = points;
		collider.Polygon = points;
    }
}