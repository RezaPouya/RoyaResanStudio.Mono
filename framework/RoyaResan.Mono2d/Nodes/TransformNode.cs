using Microsoft.Xna.Framework;

namespace RoyaResan.Mono2d.Nodes;

public class TransformNode : Node
{
    public Vector2 Position;
    public float Rotation;
    public Vector2 Scale = Vector2.One;

    public Vector2 GlobalPosition
    {
        get
        {
            if (Parent is TransformNode parentTransform)
                return parentTransform.GlobalPosition + Position;

            return Position;
        }
    }
}