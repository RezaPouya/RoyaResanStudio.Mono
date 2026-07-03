namespace RoyaResan.Mono2d.Physics;

public class Collider
{
    public Vector2 Size = new Vector2(32, 32);

    public bool IsStatic = false;

    public TransformNode Owner;

    public Rectangle Bounds
    {
        get
        {
            Vector2 pos = Owner.GlobalPosition;

            return new Rectangle(
                (int)(pos.X - Size.X / 2),
                (int)(pos.Y - Size.Y / 2),
                (int)Size.X,
                (int)Size.Y
            );
        }
    }
}