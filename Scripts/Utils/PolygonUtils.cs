using Godot;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
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
        if(polygon == null || polygon.Length < 3)
            return Vector2.Zero;

        float signedArea = 0f;
        float centerX = 0f;
        float centerY = 0f;

        for(int i=0; i<polygon.Length; i++)
        {
            Vector2 current = polygon[i];
            Vector2 next = polygon[(i+1) % polygon.Length];

            float cross = current.X * next.Y - next.X * current.Y;

            signedArea += cross;
            centerX += (current.X + next.X) * cross;
            centerY += (current.Y + next.Y) * cross;
        }
        signedArea *= 0.5f;

        if(Mathf.Abs(signedArea) < 0.0001f)
        {
            //dla zerowego polygonu
            Vector2 average = Vector2.Zero;

            foreach (Vector2 point in polygon)
                average += point;

            return average / polygon.Length;
        }

        centerX /= 6f * signedArea;
        centerY /= 6f * signedArea;

        return new Vector2(centerX, centerY);
    }
}
