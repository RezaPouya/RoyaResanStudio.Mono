using Microsoft.Xna.Framework;
using RoyaResan.Mono2d.Nodes;

namespace RoyaResan.Mono2d.Physics;

public class Collider
{
    public TransformNode Owner;

    public Rectangle Bounds;

    public bool IsSolid = true;
    public bool CanMove = true;

    public Action<Collider> OnCollision;

    private readonly HashSet<Collider> _ignored = new();

    public bool IgnoreCollisionWith(Collider other)
    {
        return _ignored.Contains(other);
    }

    public void Ignore(Collider other)
    {
        _ignored.Add(other);
    }

    public void Update(Vector2 position, int width, int height)
    {
        Bounds = new Rectangle(
            (int)position.X,
            (int)position.Y,
            width,
            height
        );
    }
}