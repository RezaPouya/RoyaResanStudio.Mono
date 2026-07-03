namespace RoyaResan.Mono2d.Nodes;

public class TransformNode : Node
{
    public Vector2 Position;
    public float Rotation = 0f;

    public Vector2 GlobalPosition
    {
        get
        {
            if (Parent is TransformNode parentTransform)
                return parentTransform.GlobalPosition + Position;

            return Position;
        }
    }

    public override void Draw(Renderer renderer)
    {
        base.Draw(renderer);
    }
}