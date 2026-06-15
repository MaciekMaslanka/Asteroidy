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
			parent.UpdateShape(result[0]);
		}
		else
		{
			
		}
	}
}
