using System;
using System.Collections.Generic;
using Godot;

public enum OreType
{
    Coal,
    Copper,
    Diamond,
    Gold,
    Iron,
    Silver,
    Uranium
}

public partial class OreScript : StaticBody2D
{
	[Export] private float MaxHealth = 50f;
    [Export] private Godot.Collections.Array<OreData> OreInfo;
    private Dictionary<OreType, OreData> oreLookup;
    [Export] public InvItem item {private set; get;}
    [Export] private PackedScene itemDropScene;
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

        oreLookup = new();
        foreach (var ore in OreInfo)
        {
            oreLookup.Add(ore.Type, ore);
        }
    }

    public void TakeDamage(float amount)
    {
        CurrentHealth -= amount;

        if (CurrentHealth <= 0)
        {
            var itemDrop = itemDropScene.Instantiate<ItemDrop>();
            itemDrop.GlobalPosition = this.GlobalPosition;
            itemDrop.SetItem(item, 1);
            GetTree().CurrentScene.GetNode("ItemDrops").AddChild(itemDrop);
            
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
        shape.Texture = oreLookup[type].Texture;
        item = oreLookup[type].Item;
    }
}