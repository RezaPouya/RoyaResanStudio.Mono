namespace RoyaResan.Mono2d.Physics
{
    public class PhysicsWorld
    {
        public List<PhysicsBody> Bodies = new();
        public List<Rope> Ropes = new();

        public void Step(float dt)
        {
            foreach (var rope in Ropes)
                rope.Step();

            // KINEMATIC PLATFORMS MOVE FIRST
            //
            // Moving platforms advance their own position before anything
            // else happens this step, so their delta is known and can be
            // used both to carry riders standing on top and to shove aside
            // any character the platform moves into from the side (crushing
            // it against a wall if there's nowhere to go).
            //
            // PushSideways below reads body.StandingOn to skip genuine
            // riders (they're carried separately, in full, below) - that
            // has to be each body's StandingOn as of the END of LAST step,
            // since this step hasn't recomputed it yet. Do not clear
            // StandingOn/IsGrounded anywhere before this loop runs.
            foreach (var body in Bodies)
            {
                if (!body.IsStatic || !body.IsMovingPlatform)
                    continue;

                Vector2 oldPos = body.Position;
                body.AdvanceKinematic(dt);
                Vector2 delta = body.Position - oldPos;

                if (delta != Vector2.Zero)
                    PushSideways(body, delta);
            }

            // INTEGRATION + SWEPT MOVE (per dynamic body, against static geometry)
            //
            // Separate-axis swept move: resolve X first, then Y, each one
            // clipping the requested movement to the exact point of
            // contact rather than moving the full distance and correcting
            // overlap afterward. This is what actually prevents a fast
            // body (a hard fall, a dash, knockback) from tunneling
            // through a thin wall/floor/one-way platform between steps -
            // it holds regardless of how large a single step's movement
            // gets, not just "usually fine at today's speeds."
            foreach (var body in Bodies)
            {
                if (body.IsStatic || body.Collider == null)
                    continue;

                // Carry: if currently resting on a moving platform, ride
                // along with its delta this step (any direction) before
                // our own gravity/input movement is applied below.
                //
                // Deliberately re-checked geometrically every step (who's
                // directly under my feet right now) instead of trusting
                // last step's StandingOn flag - a flag surviving cleanly
                // across the platform-advance loop above and this loop
                // is an easy thing to break (it WAS broken - StandingOn
                // was being cleared before anything read it). A fresh
                // geometric check has no such cross-step dependency.
                var supportingPlatform = FindSupportingPlatform(body);
                if (supportingPlatform != null)
                {
                    Vector2 platformDelta = supportingPlatform.Position - supportingPlatform.PreviousPosition;
                    body.Position += platformDelta;
                }

                // Cleared here, re-set fresh by the sweep below - so
                // IsGrounded/StandingOn (read by scripts across frames,
                // e.g. coyote time) always reflect where the body actually
                // ends up this step, independent of the carry check above.
                body.IsGrounded = false;
                body.StandingOn = null;

                if (body.UseGravity)
                    body.Velocity.Y += PhysicsSettings.Gravity * dt;

                Vector2 delta = body.Velocity * dt;

                MoveAxisSwept(body, delta.X, horizontal: true);
                MoveAxisSwept(body, delta.Y, horizontal: false);
            }

            // Dynamic-vs-dynamic soft separation (e.g. two enemies
            // overlapping, an enemy pushing the player) - deliberately
            // still discrete overlap+push-apart. These are slow, mutual
            // pushes, not fast-moving projectiles or falls, so tunneling
            // isn't a realistic risk here, and a correct swept resolution
            // between two simultaneously-moving bodies (relative-velocity
            // sweep, deciding which one yields) is a lot more machinery
            // for no practical gain in a 2D game.
            for (int i = 0; i < Bodies.Count; i++)
                for (int j = i + 1; j < Bodies.Count; j++)
                    ResolveDynamicPair(Bodies[i], Bodies[j]);

            foreach (var body in Bodies)
                body.PreviousPosition = body.Position;
        }

        /// <summary>
        /// Direct, same-step geometric check for "is this body currently
        /// resting on top of a moving platform" - used to drive carry.
        /// Checked fresh every step rather than trusting a flag set by a
        /// previous step's sweep, which is what carry used to rely on and
        /// what made it fragile enough to silently stop working. Uses a
        /// small vertical tolerance since a body resting on a platform
        /// won't sit at an exact float-equal Y every frame.
        /// </summary>
        private PhysicsBody FindSupportingPlatform(PhysicsBody body)
        {
            if (body.Collider == null)
                return null;

            float halfWidth = body.Collider.Size.X / 2f;
            float halfHeight = body.Collider.Size.Y / 2f;
            float feetY = body.Position.Y + halfHeight;

            const float tolerance = 4f;

            foreach (var other in Bodies)
            {
                if (other == body || !other.IsStatic || !other.IsMovingPlatform || other.Collider == null)
                    continue;

                var bounds = other.Collider.Bounds;

                bool xOverlaps = body.Position.X + halfWidth > bounds.Left && body.Position.X - halfWidth < bounds.Right;
                if (!xOverlaps)
                    continue;

                if (Math.Abs(feetY - bounds.Top) <= tolerance)
                    return other;
            }

            return null;
        }

        /// <summary>
        /// Shoves any non-rider character the platform's motion this step
        /// overlaps, in the direction of that motion. Riders (StandingOn ==
        /// platform) are skipped here - they're already carried in full by
        /// the Step loop above, so pushing them again here would double
        /// their movement. This only resolves overlap against the
        /// platform's own new bounds; it does not additionally sweep the
        /// pushed body against walls in the same step, so a character
        /// pinned between a platform and a wall will visibly get squeezed
        /// deeper each step it stays trapped rather than being stopped
        /// exactly at the wall - the effect you want for a crush, but
        /// worth knowing if you later add a "crushed" death/damage check
        /// (compare the character's Collider width to the gap and kill/
        /// damage it once the gap goes to zero or negative).
        /// </summary>
        private void PushSideways(PhysicsBody platform, Vector2 delta)
        {
            if (delta.X == 0f)
                return; // vertical carry is handled by the rider-carry step; this is sideways-only

            Rectangle platformBounds = platform.Collider.Bounds; // already at post-move position

            foreach (var body in Bodies)
            {
                if (body == platform || body.IsStatic || body.Collider == null)
                    continue;

                if (body.StandingOn == platform)
                    continue; // rider - already carried, don't push twice

                Vector2 halfSize = body.Collider.Size / 2f;

                bool yOverlaps = body.Position.Y + halfSize.Y > platformBounds.Top &&
                                  body.Position.Y - halfSize.Y < platformBounds.Bottom;
                if (!yOverlaps)
                    continue;

                Rectangle bodyBounds = body.Collider.Bounds;
                if (!platformBounds.Intersects(bodyBounds))
                    continue;

                if (delta.X > 0f)
                {
                    float push = platformBounds.Right - bodyBounds.Left;
                    if (push > 0f)
                        body.Position.X += push;
                }
                else
                {
                    float push = platformBounds.Left - bodyBounds.Right;
                    if (push < 0f)
                        body.Position.X += push;
                }
            }
        }

        /// <summary>
        /// Moves `body` by `delta` along a single axis, clipping the
        /// movement to the first static collider it would hit along the
        /// way instead of moving the full distance and separating
        /// afterward. Resolving one axis at a time (X then Y, called
        /// separately by Step) is the standard "separate axis" technique
        /// for axis-aligned platformer collision - it naturally gives you
        /// sliding along a wall/floor for free, since the other axis is
        /// resolved independently.
        /// </summary>
        private void MoveAxisSwept(PhysicsBody body, float delta, bool horizontal)
        {
            if (delta == 0f)
                return;

            Vector2 halfSize = body.Collider.Size / 2f;
            Vector2 startPos = body.Position;

            float allowedDelta = delta;
            bool hitSolid = false;
            PhysicsBody hitBody = null;

            foreach (var other in Bodies)
            {
                if (other == body || other.Collider == null || !other.IsStatic)
                    continue; // only static geometry clips movement here - see ResolveDynamicPair for body-vs-body

                Rectangle otherBounds = other.Collider.Bounds;

                if (other.Collider.IsOneWay)
                {
                    if (horizontal)
                        continue; // one-way platforms never block horizontal movement

                    ResolveOneWaySwept(body, other, otherBounds, delta, ref allowedDelta);
                    continue;
                }

                if (horizontal)
                {
                    bool yOverlaps = startPos.Y + halfSize.Y > otherBounds.Top &&
                                      startPos.Y - halfSize.Y < otherBounds.Bottom;
                    if (!yOverlaps)
                        continue;

                    if (delta > 0f)
                    {
                        float startRight = startPos.X + halfSize.X;
                        if (startRight <= otherBounds.Left && startRight + delta > otherBounds.Left)
                        {
                            float clipped = otherBounds.Left - startRight;
                            if (clipped < allowedDelta) { allowedDelta = clipped; hitSolid = true; hitBody = other; }
                        }
                    }
                    else
                    {
                        float startLeft = startPos.X - halfSize.X;
                        if (startLeft >= otherBounds.Right && startLeft + delta < otherBounds.Right)
                        {
                            float clipped = otherBounds.Right - startLeft;
                            if (clipped > allowedDelta) { allowedDelta = clipped; hitSolid = true; hitBody = other; }
                        }
                    }
                }
                else
                {
                    bool xOverlaps = startPos.X + halfSize.X > otherBounds.Left &&
                                      startPos.X - halfSize.X < otherBounds.Right;
                    if (!xOverlaps)
                        continue;

                    if (delta > 0f)
                    {
                        float startBottom = startPos.Y + halfSize.Y;
                        if (startBottom <= otherBounds.Top && startBottom + delta > otherBounds.Top)
                        {
                            float clipped = otherBounds.Top - startBottom;
                            if (clipped < allowedDelta) { allowedDelta = clipped; hitSolid = true; hitBody = other; }
                        }
                    }
                    else
                    {
                        float startTop = startPos.Y - halfSize.Y;
                        if (startTop >= otherBounds.Bottom && startTop + delta < otherBounds.Bottom)
                        {
                            float clipped = otherBounds.Bottom - startTop;
                            if (clipped > allowedDelta) { allowedDelta = clipped; hitSolid = true; hitBody = other; }
                        }
                    }
                }
            }

            if (horizontal)
                body.Position.X += allowedDelta;
            else
                body.Position.Y += allowedDelta;

            if (hitSolid)
            {
                if (horizontal)
                {
                    body.Velocity.X = 0f;
                }
                else
                {
                    if (delta > 0f)
                    {
                        body.Velocity.Y = 0f;
                        body.IsGrounded = true; // was falling, landed on top of something solid
                        body.StandingOn = hitBody;
                    }
                    else
                    {
                        body.Velocity.Y = 0f; // hit a ceiling
                    }
                }
            }
        }

        /// <summary>
        /// One-way platform check for the Y-axis sweep: only catches a
        /// body that starts at/above the platform's top and is moving
        /// down through it this step - lets it be jumped up through from
        /// below, walked through from the side, or dropped through once
        /// already past it. Computed directly against this step's
        /// movement (not last frame's cached position), so it's exact
        /// regardless of how far the body falls in one step.
        /// </summary>
        private void ResolveOneWaySwept(PhysicsBody body, PhysicsBody platform, Rectangle platformBounds, float originalDelta, ref float allowedDelta)
        {
            if (originalDelta <= 0f || body.Velocity.Y < 0f)
                return; // not moving down - always pass through

            float halfHeight = body.Collider.Size.Y / 2f;
            float halfWidth = body.Collider.Size.X / 2f;

            bool xOverlaps = body.Position.X + halfWidth > platformBounds.Left &&
                              body.Position.X - halfWidth < platformBounds.Right;
            if (!xOverlaps)
                return;

            float startBottom = body.Position.Y + halfHeight;

            // Small tolerance so a body already resting exactly on the
            // platform top doesn't fall through due to float rounding.
            if (startBottom > platformBounds.Top + 1f)
                return;

            if (startBottom + originalDelta <= platformBounds.Top)
                return; // won't reach the platform this step

            float clipped = platformBounds.Top - startBottom;
            if (clipped < allowedDelta)
            {
                allowedDelta = clipped;
                body.Velocity.Y = 0f;
                body.IsGrounded = true;
                body.StandingOn = platform;
            }
        }

        /// <summary>
        /// Discrete overlap+push-apart between two dynamic bodies (static
        /// geometry is handled entirely by the swept per-axis pass in
        /// Step/MoveAxisSwept, so this only ever runs for pairs where
        /// neither side is static).
        /// </summary>
        private void ResolveDynamicPair(PhysicsBody a, PhysicsBody b)
        {
            if (a.Collider == null || b.Collider == null)
                return;

            if (a.IsStatic || b.IsStatic)
                return;

            // Enemies deliberately do NOT physically push each other. This
            // is what actually eliminates the "conga line shoves the front
            // enemy off a ledge" problem - not edge detection (an enemy
            // avoiding a ledge on its own can still be shoved into it by
            // whoever's behind it). Spacing between enemies is handled by
            // steering instead - see EnemyState.ComputeSeparation, used by
            // ChaseState - which is also the more common approach in
            // action platformers generally (Hollow Knight/Dead
            // Cells/Hades-style: same-faction actors don't collide, they
            // steer clear of each other and reserve attack slots).
            if (a.Team == "Enemy" && b.Team == "Enemy")
                return;

            Rectangle A = a.Collider.Bounds;
            Rectangle B = b.Collider.Bounds;

            if (!A.Intersects(B))
                return;

            Rectangle intersection = Rectangle.Intersect(A, B);
            const float share = 0.5f; // both dynamic - split the separation evenly

            if (intersection.Width < intersection.Height)
            {
                float push = intersection.Width;
                bool aLeft = A.Center.X < B.Center.X;

                a.Position.X += aLeft ? -push * share : push * share;
                b.Position.X += aLeft ? push * share : -push * share;

                if (aLeft)
                {
                    if (a.Velocity.X > 0) a.Velocity.X = 0;
                    if (b.Velocity.X < 0) b.Velocity.X = 0;
                }
                else
                {
                    if (a.Velocity.X < 0) a.Velocity.X = 0;
                    if (b.Velocity.X > 0) b.Velocity.X = 0;
                }
            }
            else
            {
                float push = intersection.Height;
                bool aAbove = A.Center.Y < B.Center.Y;

                if (aAbove)
                {
                    // Move only the upper body.
                    a.Position.Y -= push;

                    if (a.Velocity.Y > 0)
                    {
                        a.Velocity.Y = 0;
                        a.IsGrounded = true;
                    }
                }
                else
                {
                    // Move only the upper body.
                    b.Position.Y -= push;

                    if (b.Velocity.Y > 0)
                    {
                        b.Velocity.Y = 0;
                        b.IsGrounded = true;
                    }
                }
            }
        }
    }
}
