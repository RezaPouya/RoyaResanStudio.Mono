namespace RoyaResan.Mono2d.Physics
{
    public class PhysicsWorld
    {
        public List<PhysicsBody> Bodies = new();
        public List<Rope> Ropes = new();

        public void Step()
        {
            foreach (var rope in Ropes)
                rope.Step();

            // Naive O(n²) collision check
            for (int i = 0; i < Bodies.Count; i++)
            {
                for (int j = i + 1; j < Bodies.Count; j++)
                {
                    Resolve(Bodies[i], Bodies[j]);
                }
            }
        }

        private void Resolve(PhysicsBody a, PhysicsBody b)
        {
            if (a.Collider == null || b.Collider == null)
                return;

            if (a.IsStatic && b.IsStatic)
                return;

            Rectangle A = a.Collider.Bounds;
            Rectangle B = b.Collider.Bounds;

            if (!A.Intersects(B))
                return;

            Rectangle intersection = Rectangle.Intersect(A, B);

            // Static bodies never move; if both are dynamic, split the push.
            float aShare = a.IsStatic ? 0f : (b.IsStatic ? 1f : 0.5f);
            float bShare = b.IsStatic ? 0f : (a.IsStatic ? 1f : 0.5f);

            if (intersection.Width < intersection.Height)
            {
                float push = intersection.Width;
                bool aLeft = A.Center.X < B.Center.X;

                if (!a.IsStatic)
                {
                    a.Position.X += aLeft ? -push * aShare : push * aShare;
                    if (aLeft && a.Velocity.X > 0) a.Velocity.X = 0;
                    if (!aLeft && a.Velocity.X < 0) a.Velocity.X = 0;
                }
                if (!b.IsStatic)
                {
                    b.Position.X += aLeft ? push * bShare : -push * bShare;
                    if (aLeft && b.Velocity.X < 0) b.Velocity.X = 0;
                    if (!aLeft && b.Velocity.X > 0) b.Velocity.X = 0;
                }
            }
            else
            {
                float push = intersection.Height;
                bool aAbove = A.Center.Y < B.Center.Y;

                if (!a.IsStatic)
                {
                    a.Position.Y += aAbove ? -push * aShare : push * aShare;
                    if (aAbove && a.Velocity.Y > 0) a.Velocity.Y = 0;
                    if (!aAbove && a.Velocity.Y < 0) a.Velocity.Y = 0;
                }
                if (!b.IsStatic)
                {
                    b.Position.Y += aAbove ? push * bShare : -push * bShare;
                    if (aAbove && b.Velocity.Y < 0) b.Velocity.Y = 0;
                    if (!aAbove && b.Velocity.Y > 0) b.Velocity.Y = 0;
                }
            }
        }
    }
}