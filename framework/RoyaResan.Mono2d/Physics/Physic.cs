using Microsoft.Xna.Framework;

namespace RoyaResan.Mono2d.Physics;

public class Physic
{
    private readonly List<Collider> _colliders = new();

    public void Register(Collider collider)
    {
        if (collider == null || _colliders.Contains(collider))
            return;

        _colliders.Add(collider);
    }

    public void Unregister(Collider collider)
    {
        _colliders.Remove(collider);
    }

    public void Clear()
    {
        _colliders.Clear();
    }

    public void Update()
    {
        for (int i = 0; i < _colliders.Count; i++)
        {
            for (int j = i + 1; j < _colliders.Count; j++)
            {
                var a = _colliders[i];
                var b = _colliders[j];

                if (a == null || b == null)
                    continue;

                if (a.IgnoreCollisionWith(b))
                    continue;

                if (IsColliding(a.Bounds, b.Bounds))
                {
                    a.OnCollision?.Invoke(b);
                    b.OnCollision?.Invoke(a);

                    ResolveSimple(a, b);
                }
            }
        }
    }

    private bool IsColliding(Rectangle a, Rectangle b)
    {
        return a.Intersects(b);
    }

    private void ResolveSimple(Collider a, Collider b)
    {
        if (!a.IsSolid || !b.IsSolid)
            return;

        Rectangle overlap = GetOverlap(a.Bounds, b.Bounds);

        if (overlap.Width < overlap.Height)
        {
            int push = overlap.Width / 2;

            if (a.CanMove)
                a.Owner.Position.X -= push;

            if (b.CanMove)
                b.Owner.Position.X += push;
        }
        else
        {
            int push = overlap.Height / 2;

            if (a.CanMove)
                a.Owner.Position.Y -= push;

            if (b.CanMove)
                b.Owner.Position.Y += push;
        }
    }

    private Rectangle GetOverlap(Rectangle a, Rectangle b)
    {
        int x = System.Math.Max(a.Left, b.Left);
        int y = System.Math.Max(a.Top, b.Top);
        int w = System.Math.Min(a.Right, b.Right) - x;
        int h = System.Math.Min(a.Bottom, b.Bottom) - y;

        return new Rectangle(x, y, w, h);
    }

    // -----------------------------
    // QUERY SYSTEM (IMPORTANT FOR AI + ROPE + COMBAT)
    // -----------------------------

    public List<Collider> QueryArea(Rectangle area)
    {
        List<Collider> result = new();

        foreach (var c in _colliders)
        {
            if (c.Bounds.Intersects(area))
                result.Add(c);
        }

        return result;
    }

    public Collider Raycast(Vector2 origin, Vector2 direction, float distance)
    {
        Rectangle rayArea = new Rectangle(
            (int)origin.X,
            (int)origin.Y,
            (int)(direction.X * distance),
            (int)(direction.Y * distance)
        );

        foreach (var c in _colliders)
        {
            if (c.Bounds.Intersects(rayArea))
                return c;
        }

        return null;
    }
}