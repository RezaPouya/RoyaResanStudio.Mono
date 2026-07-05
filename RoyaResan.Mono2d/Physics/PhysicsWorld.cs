namespace RoyaResan.Mono2d.Physics
{
    public class PhysicsWorld
    {
        public List<PhysicsBody> Bodies = new();
        public List<Rope> Ropes = new();

        //public void Step()
        //{
        //    foreach (var rope in Ropes)
        //        rope.Step();

        //    // Cleared before resolving - Resolve()/ResolveOneWay() set it
        //    // back to true for whatever's actually resting on something
        //    // this step.
        //    foreach (var body in Bodies)
        //        if (!body.IsStatic)
        //            body.IsGrounded = false;

        //    // Naive O(n²) collision check
        //    for (int i = 0; i < Bodies.Count; i++)
        //    {
        //        for (int j = i + 1; j < Bodies.Count; j++)
        //        {
        //            Resolve(Bodies[i], Bodies[j]);
        //        }
        //    }

        //    // Cache resolved positions for next frame's one-way platform checks.
        //    foreach (var body in Bodies)
        //        body.PreviousPosition = body.Position;
        //}

        public void Step(float dt)  // Change signature
        {
            foreach (var rope in Ropes)
                rope.Step();

            // Clear grounded
            foreach (var body in Bodies)
                if (!body.IsStatic)
                    body.IsGrounded = false;

            // INTEGRATION (gravity + move)
            foreach (var body in Bodies)
            {
                if (!body.IsStatic)
                {
                    if (body.UseGravity)
                        body.Velocity.Y += PhysicsSettings.Gravity * dt;
                    body.Position += body.Velocity * dt;
                }
            }

            // Then resolve (as before, but now on post-move positions)
            for (int i = 0; i < Bodies.Count; i++)
                for (int j = i + 1; j < Bodies.Count; j++)
                    Resolve(Bodies[i], Bodies[j]);

            // PreviousPosition cache
            foreach (var body in Bodies)
                body.PreviousPosition = body.Position;
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

            // One-way platform: exactly one side is marked IsOneWay -
            // handle it separately, it never uses the normal push-apart.
            if (a.Collider.IsOneWay != b.Collider.IsOneWay)
            {
                var platform = a.Collider.IsOneWay ? a : b;
                var passer = a.Collider.IsOneWay ? b : a;
                ResolveOneWay(platform, passer);
                return;
            }

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
                    if (aAbove && a.Velocity.Y > 0)
                    {
                        a.Velocity.Y = 0;
                        a.IsGrounded = true; // a was falling, landed on top of b
                    }
                    if (!aAbove && a.Velocity.Y < 0) a.Velocity.Y = 0;
                }
                if (!b.IsStatic)
                {
                    b.Position.Y += aAbove ? push * bShare : -push * bShare;
                    if (aAbove && b.Velocity.Y < 0) b.Velocity.Y = 0;
                    if (!aAbove && b.Velocity.Y > 0)
                    {
                        b.Velocity.Y = 0;
                        b.IsGrounded = true; // b was falling, landed on top of a
                    }
                }
            }
        }

        /// <summary>
        /// Only catches a body that was fully above the platform last
        /// frame AND is moving downward - lets it be jumped up through
        /// from below, walked through from the side, or dropped through
        /// once already past it, exactly like a standard one-way platform.
        /// </summary>
        private void ResolveOneWay(PhysicsBody platform, PhysicsBody passer)
        {
            if (passer.IsStatic || passer.Velocity.Y < 0f)
                return;

            Rectangle platformBounds = platform.Collider.Bounds;
            float passerHalfHeight = passer.Collider.Size.Y / 2f;
            float prevBottom = passer.PreviousPosition.Y + passerHalfHeight;

            // Small tolerance avoids missing a catch due to float rounding
            // right at the boundary.
            if (prevBottom > platformBounds.Top + 1f)
                return;

            passer.Position.Y = platformBounds.Top - passerHalfHeight;
            passer.Velocity.Y = 0f;
            passer.IsGrounded = true;
        }
    }
}