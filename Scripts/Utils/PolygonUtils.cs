using Godot;
using System;
using System.Linq;
public static partial class PolygonUtils
{
    public static float CalculatePolygonArea(Vector2[] polygon)
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
    public static Vector2 GetPolygonCenter(Vector2[] polygon)
    {
        Vector2 center = Vector2.Zero;
        foreach(var p in polygon)
        {
            center += p;
        }
        center /= polygon.Count();
        return center;
    }
}
