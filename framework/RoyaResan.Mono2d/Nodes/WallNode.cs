using RoyaResan.Mono2d.Physics;

public class WallNode : TransformNode
{
    public Collider Collider;

    public WallNode()
    {
        Collider = new Collider
        {
            Owner = this,
            Size = new Vector2(64, 64),
            IsStatic = true
        };
    }
}