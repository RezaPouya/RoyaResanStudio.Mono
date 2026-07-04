using RoyaResan.Mono2d.Physics;

namespace RoyaResan.Mono2d.Nodes;

/// <summary>
/// A platform you can jump up through and land on top of, but never
/// collide with from below or the sides - the standard 2D platformer
/// one-way platform.
/// </summary>
public class OneWayPlatformNode : PhysicsBody
{
    public OneWayPlatformNode()
    {
        IsStatic = true;

        Collider = new Collider
        {
            Owner = this,
            Size = new Vector2(64, 16),
            IsStatic = true,
            IsOneWay = true
        };
    }
}
