using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class OreGenerator
{
	private Asteroid parent;
	private int minAmount;
	private int maxAmount;
	private PackedScene oreScene;
	private Vector2[] parentShape;
	private float offset;
	private float minimalDistance;
	private List<OreScript> ores = new();
	private Array<OreRarity> rarities;

	public OreGenerator(Asteroid parent, PackedScene oreScene, int minAmount, int maxAmount, float offset, float minimalDistance, Array<OreRarity> rarities = null)
	{
		this.parent = parent;
		this.oreScene = oreScene;
		this.minAmount = minAmount;
		this.maxAmount = maxAmount;
		this.offset = offset;
		this.minimalDistance = minimalDistance;
		this.rarities = rarities;
		parentShape = parent.currentShape;
	}
	public void GenerateOres()
	{
		if(oreScene == null) return;

		int amount = GD.RandRange(minAmount, maxAmount);

		for(int i=0; i<amount; i++)
		{
			OreScript ore = oreScene.Instantiate<OreScript>();

			Vector2 pos = GetRandomPointInAsteroid();
			ore.Position = pos;

			ore.AddCollisionExceptionWith(parent);

			ores.Add(ore);
			parent.ores.Add(ore);
			parent.AddChild(ore);

			ore.SetOreType(GetRandomType());
			ore.shape.TextureRotation = parent.body.TextureRotation;
			ore.shape.TextureOffset = new Vector2(
				GD.RandRange(-128, 128),
				GD.RandRange(-128, 128)
			);
		}
	}
	private OreType GetRandomType()
	{
		if(rarities == null || rarities.Count == 0) return OreType.Gold;

		float totalWeight = 0;
		foreach(var r in rarities)
		{
			totalWeight += r.Weight;
		}

		if(totalWeight <= 0)
		{
			return OreType.Gold;
		}

		float random = (float) GD.Randf() * totalWeight;
		float current = 0;

		foreach(var r in rarities)
		{
			current += r.Weight;
			if(random <= current)
			{
				return r.Type;
			}
		}
		
		return rarities[0].Type;
	}
	private Vector2 GetRandomPointInAsteroid()
	{
		float minX = parentShape.Min(p => p.X) + offset;
		float maxX = parentShape.Max(p => p.X) - offset;
		float minY = parentShape.Min(p => p.Y) + offset;
		float maxY= parentShape.Max(p => p.Y) - offset;

		Vector2 point;
		int tries = 0;

		do
		{
			float x = (float) GD.RandRange(minX, maxX);
			float y = (float) GD.RandRange(minY, maxY);
			point = new Vector2(x, y);
			tries++;
		}
		while((!Geometry2D.IsPointInPolygon(point, parentShape) || !IsPointFarEnough(point)) && tries <= 50);

		return point;
	}
	private bool IsPointFarEnough(Vector2 point)
	{
		if(ores.Count == 0) return true;
		else
		{
			foreach(var ore in ores)
			{
				if(point.DistanceTo(ore.Position) < minimalDistance)
				{
					return false;
				}
			}
			return true;
		}
	}
}
