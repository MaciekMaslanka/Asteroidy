using Godot;
using System;

public partial class ContextMap
{
    private readonly int _resolution;
    private readonly float[] _interest;
    private readonly float[] _danger;
    private readonly Vector2[] _directions;

    public ContextMap(int resolution = 16)
    {
        _resolution = resolution;
        _interest = new float[resolution];
        _danger = new float[resolution];
        _directions = new Vector2[resolution];

        float angleStep = Mathf.Tau / resolution;
        for(int i=0; i< resolution; i++)
        {
            _directions[i] = Vector2.Right.Rotated(i * angleStep);
        }
    }

    public void Update(Vector2 globalPosition, Vector2 targetDirection, PhysicsDirectSpaceState2D spaceState, float maxRayDistance = 600f)
    {
        //reset
        for(int i=0; i<_resolution; i++)
        {
            _interest[i] = 0;
            _danger[i] = 0;
        }

        //intrest
        Vector2 targetDirNorm = targetDirection.Normalized();
        for(int i=0; i<_resolution; i++)
        {
            float dot = _directions[i].Dot(targetDirNorm);
            _interest[i] = Mathf.Max(0f, dot);
        }

        //danger
        for(int i=0; i<_resolution; i++)
        {
            Vector2 dir = _directions[i];
            Vector2 to = globalPosition + dir * maxRayDistance;

            var query = new PhysicsRayQueryParameters2D
            {
                From = globalPosition,
                To = to,
                CollideWithBodies = true
            };

            var result = spaceState.IntersectRay(query);

            if(result.Count > 0)
            {
                Vector2 hitPoint = (Vector2) result["position"];
                float distance = globalPosition.DistanceTo(hitPoint);
                float dangerValue = 1f - (distance / maxRayDistance);
                _danger[i] = Mathf.Clamp(dangerValue * dangerValue, 0f, 1f);
            }
        }
    }
    public Vector2 GetBestDirection(float interestWeight = 1f, float dangerWeight = 1.5f)
    {
        float bestScore = float.MinValue;
        int bestIndex = 0;

        for(int i=0; i<_resolution; i++)
        {
            float score = (_interest[i] * interestWeight) - (_danger[i] * dangerWeight);
            if(score > bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }
        }

        return _directions[bestIndex];
    }
}