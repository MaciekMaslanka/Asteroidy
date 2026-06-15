using System;
using Godot;
using System;

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
		if(result.Count == 0) return;
		else if(result.Count == 1)
		{
			if(CalculatePolygonArea(result[0]) < killThreshold)
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
				float area = CalculatePolygonArea(result[i]);
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
				if(CalculatePolygonArea(result[i]) < killThreshold) continue;
				CreateNewFragment(result[i]);
			}
		}
	}
	private float CalculatePolygonArea(Vector2[] polygon)
	{
		float area = 0f;
		for (int i=0; i<polygon.Length; i++)
		{
			Vector2 a = polygon[i];
			Vector2 b = polygon[(i+1) % polygon.Length];
			area += a.X * b.Y - b.X * a.Y;
		}
		return Mathf.Abs(area) * 0.5f;
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
		fragment.GetNode<Polygon2D>("Polygon2D").TextureRotation = parent.body.TextureRotation;
	}
}
