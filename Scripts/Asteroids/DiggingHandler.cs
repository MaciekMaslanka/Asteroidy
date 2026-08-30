using System.Collections.Generic;
using Godot;

public partial class DiggingHandler : Node
{
	private Vector2 point;
	private float radius;
	private int segments;
	private OreScript ore;
	private Asteroid parent;
	const float killThreshold = 100f;
	public DiggingHandler(Vector2 point, float radius, int segments, Asteroid parent)
	{
		this.point = point;
		this.radius = radius;
		this.segments = segments;
		this.parent = parent;
	}
	public DiggingHandler(Asteroid parent, OreScript ore)
	{
		this.parent = parent;
		this.ore = ore;
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
	public void NormalDigging()
	{
		Vector2[] cutter = CreateCutter(point, radius, segments);
		var result = Geometry2D.ClipPolygons(parent.currentShape, cutter);
		HandleFragments(result);
	}
	public void OnOreDestroyed()
	{
		//przełożenie ore na lokalne koordy
		Vector2[] oreLocalShape = ore.GetNode<Polygon2D>("Polygon2D").Polygon;
		Vector2 oreLocalPos = ore.Position;
		Vector2[] cutterShape = new Vector2[oreLocalShape.Length];
		for(int i=0; i<cutterShape.Length; i++)
		{
			cutterShape[i] = oreLocalPos + oreLocalShape[i];
		}

		var result = Geometry2D.ClipPolygons(parent.currentShape, cutterShape);
		HandleFragments(result);
	}
	private void HandleFragments(Godot.Collections.Array<Vector2[]> result)
	{
		//sprawdzenie czy nie zrobił się problem z otoczeniem polB przez polA
		for(int i = result.Count-1; i>=0; i--)
		{
			if(Geometry2D.IsPolygonClockwise(result[i]))
			{
				result.RemoveAt(i);
			}
		}

		if(result.Count == 0) return;

		int biggestIndex = 0;
		float biggestArea = 0f;

		for(int i=0; i<result.Count; i++)
		{
			float area = PolygonUtils.CalculatePolygonArea(result[i]);
			if(area > biggestArea)
			{
				biggestArea = area;
				biggestIndex = i;
			}
		}

		parent.UpdateShape(result[biggestIndex]);

		if(biggestArea < killThreshold)
		{
			parent.QueueFree();
			return;
		}

		List<Asteroid> newFragments = new();

		for (int i=0; i<result.Count; i++)
		{
			if(i==biggestIndex) continue;
			if(PolygonUtils.CalculatePolygonArea(result[i]) < killThreshold) continue;

			Asteroid fragment = CreateNewFragment(result[i]);
			if(fragment != null)
				newFragments.Add(fragment);
		}
		DistributeOresToFragments(newFragments);

		parent.UpdateMass();
	}
	private Asteroid CreateNewFragment(Vector2[] points)
	{
		if(parent.AsteroidScene == null)
		{
			GD.PrintErr("asteroid scene to null");
			return null;
		}
		Asteroid fragment = parent.AsteroidScene.Instantiate<Asteroid>();
		fragment.Position = parent.Position;
		fragment.Rotation = parent.Rotation;
		fragment.SetCustomShape(points);

		parent.GetParent().AddChild(fragment);
		fragment.body.Texture = parent.body.Texture;
		fragment.body.TextureRotation = parent.body.TextureRotation;

		return fragment;
	}
	private void DistributeOresToFragments(List<Asteroid> fragments)
	{
		var parentOres = parent.ores;

		if(parentOres == null || parentOres.Count == 0 || fragments.Count == 0)
			return;
			
		var oresToCheck = new List<OreScript>(parentOres);

		foreach(var ore in oresToCheck)
		{
			if (!GodotObject.IsInstanceValid(ore)) continue;

			Vector2 oreGlobal = ore.GlobalPosition;
			foreach (var fragment in fragments)
			{
				if (!GodotObject.IsInstanceValid(fragment)) continue;
				
				Vector2 oreLocal = fragment.ToLocal(oreGlobal);

				if (Geometry2D.IsPointInPolygon(oreLocal, fragment.currentShape))
				{
					ore.Reparent(fragment);
					fragment.ores.Add(ore);
					parentOres.Remove(ore);
					break; // przypisujemy tylko do pierwszego pasującego fragmentu
				}
			}
		}
	}
}
