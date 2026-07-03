namespace RoyaResan.Mono2d.Physics
{
    public class PhysicsWorld
    {
        public List<PhysicsBody> Bodies = new();

        public void Step()
        {
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

            Rectangle A = a.Collider.Bounds;
            Rectangle B = b.Collider.Bounds;

            if (!A.Intersects(B))
                return;

            Rectangle intersection = Rectangle.Intersect(A, B);

            // simple separation axis (minimal resolution)
            if (intersection.Width < intersection.Height)
            {
                float push = intersection.Width;

                if (A.Center.X < B.Center.X)
                    a.Position.X -= push;
                else
                    a.Position.X += push;
            }
            else
            {
                float push = intersection.Height;

                if (A.Center.Y < B.Center.Y)
                    a.Position.Y -= push;
                else
                    a.Position.Y += push;
            }
        }
    }
}