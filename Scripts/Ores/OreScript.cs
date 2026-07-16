using Godot;
using System;
public enum OreType
{
    Gold,
    Silver,
    Coal
}

public partial class OreScript : StaticBody2D
{
	[Export] private float MaxHealth = 50f;
    [Export] private Texture2D[] oreTextures; 
    [Export] public InvItem item {private set; get;}
    public float CurrentHealth { get; private set; }

    public Polygon2D shape {get; private set;}
	private CollisionPolygon2D collider;
    public OreType type {get; private set;}

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
            GetParent<Asteroid>().OnOreDestroyed(this);
        }
    }
    private void GenerateShape(float baseRadius = 35f, float amplitude = 0.3f)
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
    public void SetOreType(OreType newType)
    {
        type = newType;
        switch(type)
        {
            case OreType.Gold:
                shape.Texture = oreTextures[(int)OreType.Gold];
                item = GD.Load<InvItem>("res://Resources/Items/GoldItem.tres");
                break;
            case OreType.Silver:
                shape.Texture = oreTextures[(int)OreType.Silver];
                item = GD.Load<InvItem>("res://Resources/Items/SilverItem.tres");
                break;
            case OreType.Coal:
                shape.Texture = oreTextures[(int)OreType.Coal];
                item = GD.Load<InvItem>("res://Resources/Items/CoalItem.tres");
                break;
            default:
                shape.Texture = oreTextures[(int)OreType.Gold];
                item = GD.Load<InvItem>("res://Resources/Items/GoldItem.tres");
                GD.PrintErr("Nie dopisałeś case w tekturach od ore");
                break;
        }
    }
}