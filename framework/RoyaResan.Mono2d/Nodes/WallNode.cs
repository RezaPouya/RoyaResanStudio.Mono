using RoyaResan.Mono2d.Physics;

namespace RoyaResan.Mono2d.Nodes;

public class WallNode : PhysicsBody
{
    public WallNode()
    {
        IsStatic = true;

        Collider = new Collider
        {
            Owner = this,
            Size = new Vector2(64, 64),
            IsStatic = true
        };
    }
}