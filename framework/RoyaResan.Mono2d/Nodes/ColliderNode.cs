using Microsoft.Xna.Framework;

namespace RoyaResan.Mono2d.Nodes;

public class ColliderNode : TransformNode
{
    public Vector2 Size;

    public Rectangle GetBounds()
    {
        var pos = GlobalPosition;

        return new Rectangle(
            (int)pos.X,
            (int)pos.Y,
            (int)Size.X,
            (int)Size.Y
        );
    }
}