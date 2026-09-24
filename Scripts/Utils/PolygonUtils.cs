using System.Collections.Generic;
using Godot;
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

    public static Vector2[] Simplify(Vector2[] points, float epsilon)
    {
        if(points == null || points.Length < 6)
            return points;
        
        int last = points.Length - 1;
        
        bool[] keep = new bool[points.Length];
        keep[0] = true;
        keep[last] = true;

        Rdp(points, 0, last, epsilon, keep);

        var result = new List<Vector2>(points.Length);
        for(int i=0; i<points.Length; i++)
        {
            if(keep[i])
                result.Add(points[i]);
        }

        if(result.Count < 3)
            return points;

        return result.ToArray();
    }

    private static void Rdp(Vector2[] points, int start, int end, float epsilon, bool[] keep)
    {
        if(end <= start + 1)
            return;

        float maxDist = 0f;
        int index = -1;

        Vector2 a = points[start];
        Vector2 b = points[end];

        for(int i=start+1; i<end; i++)
        {
            float dist = PerpDistance(points[i], a, b);
            if(dist > maxDist)
            {
                maxDist = dist;
                index = i;
            }
        }

        if(maxDist > epsilon && index != -1)
        {
            keep[index] = true;
            Rdp(points, start, index, epsilon, keep);
            Rdp(points, index, end, epsilon, keep);
        }
    }
    private static float PerpDistance(Vector2 p, Vector2 a, Vector2 b)
    {
        float dx = b.X - a.X;
        float dy = b.Y - a.Y;

        float lenSq = dx * dx + dy * dy;
        if(lenSq < 0.0001f)
            return p.DistanceTo(a);

        float t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lenSq;
        t = Mathf.Clamp(t, 0f, 1f);

        Vector2 proj = new Vector2(a.X + t * dx, a.Y + t * dy);
        return p.DistanceTo(proj);
    }
}
