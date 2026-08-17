using Godot;

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

    public void Update(
        Vector2 globalPosition, 
        Vector2 targetDirection,
        PhysicsDirectSpaceState2D spaceState, 
        float rayLength,
        CircleShape2D avoidanceShape,
        Rid selfRid)
    {
        //reset
        for(int i=0; i<_directions.Length; i++)
        {
            _danger[i] = 0f;
            _interest[i] = 0f;
        }

        for(int i=0; i < _resolution; i++)
        {
            Vector2 dir = _directions[i];

            //interest
            float dot = dir.Dot(targetDirection);
            _interest[i] = Mathf.Max(0f, dot);

            //przeszkody
            var query = new PhysicsShapeQueryParameters2D
            {
                Shape = avoidanceShape,
                Transform = new Transform2D(0f, globalPosition),
                Motion = dir * rayLength,
                Exclude = new Godot.Collections.Array<Rid> {selfRid},
                CollideWithBodies = true
            };

            var result = spaceState.CastMotion(query);

            if(result[0] < 1f)
            {
                _danger[i] = 1f - result[0];
            }
            else
            {
                _danger[i] = 0f;
            }
        }
    }
    public Vector2 GetSteeringDirection(float interestWeight = 1f, float dangerWeight = 1.5f)
    {
        Vector2 bestDir = Vector2.Zero;

        for (int i=0; i<_resolution; i++)
        {
            float dangerPenalty = Mathf.Clamp(
                1f - _danger[i] * dangerWeight,
                0f,
                1f
            );

            GD.Print(dangerPenalty);

            float weight = _interest[i] * interestWeight * dangerPenalty;

            if((_directions[i] * weight).Length() > bestDir.Length())
            {
                bestDir = _directions[i];
            }
        }

        return bestDir;
    }
}