namespace RoyaResan.Mono2d.Nodes;

/// <summary>
/// A solid platform that ping-pongs between two points at constant speed.
/// Riders standing on top are carried automatically (PhysicsBody.StandingOn),
/// and any character the platform moves into from the side gets shoved
/// aside rather than the platform stopping - see PhysicsWorld.PushSideways.
/// </summary>
public class MovingPlatformNode : PhysicsBody
{
    public Vector2 PointA;
    public Vector2 PointB;
    public float Speed = 80f;

    private bool _movingToB = true;

    public MovingPlatformNode()
    {
        IsStatic = true;
        IsMovingPlatform = true;

        Collider = new Collider
        {
            Owner = this,
            Size = new Vector2(96, 16),
            IsStatic = true
        };
    }

    public override void AdvanceKinematic(float dt)
    {
        Vector2 target = _movingToB ? PointB : PointA;
        Vector2 toTarget = target - Position;
        float dist = toTarget.Length();
        float step = Speed * dt;

        if (dist <= step)
        {
            Position = target;
            _movingToB = !_movingToB;
        }
        else
        {
            Position += toTarget / dist * step;
        }
    }
}
