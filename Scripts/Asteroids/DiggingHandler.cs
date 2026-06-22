using System;
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
	private List<OreScript> parentOres;
	public DiggingHandler(Vector2 point, float radius, int segments, Asteroid parent)
	{
		this.point = point;
		this.radius = radius;
		this.segments = segments;
		this.parent = parent;
		this.parentOres = parent.ores;
		NormalDigging();
	}
	public DiggingHandler(Asteroid parent, OreScript ore)
	{
		this.parent = parent;
		this.ore = ore;
		OnOreDestroyed();
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
		var resultCopy = result;
		foreach (var el in resultCopy)
		{
			if(Geometry2D.IsPolygonClockwise(el))
			{
				result.Remove(el);
			}
		}

		if(result.Count == 0) return;
		else if(result.Count == 1)
		{
			if(PolygonUtils.CalculatePolygonArea(result[0]) < killThreshold)
			{
				parent.QueueFree();
			}
			else
			{
				parent.UpdateShape(result[0]);
			}
		}
		else
		{
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

			for(int i=0; i<result.Count; i++)
			{
				if(i == biggestIndex) continue;
				if(PolygonUtils.CalculatePolygonArea(result[i]) < killThreshold) continue;
				CreateNewFragment(result[i]);
			}
		}
		parent.UpdateMass();
	}
	private void CreateNewFragment(Vector2[] points)
	{
		if(parent.AsteroidScene == null)
		{
			GD.PrintErr("debil");
			return;
		}
		Asteroid fragment = parent.AsteroidScene.Instantiate<Asteroid>();
		fragment.Position = parent.Position;
		fragment.Rotation = parent.Rotation;
		fragment.SetCustomShape(points);
		parent.GetParent().AddChild(fragment);
		AssignOres(fragment);
		fragment.GetNode<Polygon2D>("Polygon2D").TextureRotation = parent.body.TextureRotation;
	}
	private void AssignOres(Asteroid newFragment)
	{
		if (parentOres == null || parentOres.Count == 0)
			return;

		var oresToCheck = new List<OreScript>(parentOres);

		foreach (var ore in oresToCheck)
		{
			Vector2 oreGlobalPos = ore.GlobalPosition;
			Vector2 oreLocalToFragment = newFragment.ToLocal(oreGlobalPos);

			if (Geometry2D.IsPointInPolygon(oreLocalToFragment, newFragment.currentShape))
			{
				if (ore.GetParent() != null)
					ore.GetParent().RemoveChild(ore);

				newFragment.AddChild(ore);
				newFragment.ores.Add(ore);

				parentOres.Remove(ore);
			}
		}
	}
}
