namespace RoyaResan.Mono2d.Physics
{
    public class PhysicsWorld
    {
        public List<PhysicsBody> Bodies = new();
        public List<Rope> Ropes = new();

        public void Step(float dt)
        {
            // 1. Update ropes
            foreach (var rope in Ropes)
                rope.Step();

            // 2. Advance kinematic platforms (moving platforms) – they move first
            foreach (var body in Bodies)
            {
                if (!body.IsStatic || !body.IsMovingPlatform)
                    continue;

                Vector2 oldPos = body.Position;
                body.AdvanceKinematic(dt);
                Vector2 delta = body.Position - oldPos;

                if (delta.X != 0f)
                    PushSideways(body, delta);

                if (delta.Y != 0f)
                    PushVertically(body, delta);
            }

            // 3. Dynamic bodies: carry, gravity, swept movement
            foreach (var body in Bodies)
            {
                if (body.IsStatic || body.Collider is null)
                    continue;

                // ---- Determine which platform (if any) should carry this body ----
                PhysicsBody carryingPlatform = null;

                // If we were standing on a moving platform last frame, keep riding it.
                if (body.StandingOn != null && body.StandingOn.IsMovingPlatform)
                {
                    carryingPlatform = body.StandingOn;
                }
                else
                {
                    // Not on a moving platform – check for new support (static or one‑way).
                    carryingPlatform = FindSupportingPlatform(body);
                }

                // Clear ground state – will be reset by the sweep below if we land.
                body.IsGrounded = false;
                body.StandingOn = null;

                // Apply the platform's movement delta (if any)
                if (carryingPlatform != null)
                {
                    Vector2 platformDelta = carryingPlatform.Position - carryingPlatform.PreviousPosition;
                    body.Position += platformDelta;

                    // ---- FIX: Clamp Y to platform top if it's a vertical moving platform ----
                    if (Math.Abs(platformDelta.Y) > 0.001f &&
                        Math.Abs(body.Position.X - carryingPlatform.Position.X) <
                            (body.Collider.Size.X / 2 + carryingPlatform.Collider.Size.X / 2))
                    {
                        float halfHeight = body.Collider.Size.Y / 2f;
                        float targetY = carryingPlatform.Collider.Bounds.Top - halfHeight;
                        // Only clamp if we're near the platform (prevents snapping from far away)
                        if (Math.Abs(body.Position.Y - targetY) < 10f)
                        {
                            body.Position.Y = targetY;
                        }
                    }
                }

                // Apply gravity
                if (body.UseGravity)
                    body.Velocity.Y += PhysicsSettings.Gravity * dt;

                // Swept move along X and Y
                Vector2 delta = body.Velocity * dt;

                MoveAxisSwept(body, delta.X, horizontal: true);
                MoveAxisSwept(body, delta.Y, horizontal: false);
            }

            // 4. Dynamic‑vs‑dynamic soft separation (enemies, player push, etc.)
            for (int i = 0; i < Bodies.Count; i++)
                for (int j = i + 1; j < Bodies.Count; j++)
                    ResolveDynamicPair(Bodies[i], Bodies[j]);

            // 5. Cache positions for next step (used by moving platforms and carry)
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
            if (body.Collider is null)
                return null;

            float halfWidth = body.Collider.Size.X / 2f;
            float halfHeight = body.Collider.Size.Y / 2f;
            float feetY = body.Position.Y + halfHeight;

            const float tolerance = 20f;  // Increased significantly for vertical platforms + jumps

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
        /// Vertical counterpart to PushSideways - resolves a moving
        /// platform's own Y motion against any non-rider body it moved
        /// into this step, BEFORE that body's own gravity/sweep runs.
        ///
        /// Without this, a vertical (elevator-style) platform that moves
        /// fast enough to already overlap a body by the time the normal
        /// per-body swept pass runs will silently pass through it: the
        /// swept check in MoveAxisSwept only reacts to the BODY's own
        /// motion this step, not to the platform having moved into the
        /// body first. This is what causes an elevator to "phase through"
        /// a player standing above or below it instead of carrying/
        /// blocking them - the missing vertical equivalent of the
        /// horizontal crush-push.
        /// </summary>
        private void PushVertically(PhysicsBody platform, Vector2 delta)
        {
            Rectangle platformBounds = platform.Collider.Bounds; // already at post-move position

            foreach (var body in Bodies)
            {
                if (body == platform || body.IsStatic || body.Collider == null)
                    continue;

                if (body.StandingOn == platform)
                    continue; // rider - already carried in full by the Step loop above

                Vector2 halfSize = body.Collider.Size / 2f;

                bool xOverlaps = body.Position.X + halfSize.X > platformBounds.Left &&
                                  body.Position.X - halfSize.X < platformBounds.Right;
                if (!xOverlaps)
                    continue;

                Rectangle bodyBounds = body.Collider.Bounds;
                if (!platformBounds.Intersects(bodyBounds))
                    continue;

                if (delta.Y < 0f)
                {
                    // Platform rising into the underside of a body above it:
                    // push the body up so it ends up resting on top, exactly
                    // as if it had landed there normally.
                    float push = platformBounds.Top - bodyBounds.Bottom;
                    if (push < 0f)
                    {
                        body.Position.Y += push;
                        if (body.Velocity.Y > 0f)
                            body.Velocity.Y = 0f;

                        body.IsGrounded = true;
                        body.StandingOn = platform;
                    }
                }
                else
                {
                    // Platform descending into a body below it (e.g. one
                    // standing under a descending elevator): push the body
                    // down out of the way, same idea as hitting a ceiling.
                    float push = platformBounds.Bottom - bodyBounds.Top;
                    if (push > 0f)
                    {
                        body.Position.Y += push;
                        if (body.Velocity.Y < 0f)
                            body.Velocity.Y = 0f;
                    }
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

            const float epsilon = 0.5f; // Tolerance for floating‑point rounding

            foreach (var other in Bodies)
            {
                if (other == body || other.Collider == null || !other.IsStatic)
                    continue; // only static geometry clips movement

                Rectangle otherBounds = other.Collider.Bounds;

                if (other.Collider.IsOneWay)
                {
                    if (horizontal)
                        continue; // one‑way platforms never block horizontal movement

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
                else // vertical movement
                {
                    bool xOverlaps = startPos.X + halfSize.X > otherBounds.Left &&
                                      startPos.X - halfSize.X < otherBounds.Right;
                    if (!xOverlaps)
                        continue;

                    if (delta > 0f)
                    {
                        float startBottom = startPos.Y + halfSize.Y;

                        // FIX: add epsilon tolerance so tiny floating‑point overlaps still snap to ground
                        if (startBottom <= otherBounds.Top + epsilon && startBottom + delta > otherBounds.Top)
                        {
                            float clipped = otherBounds.Top - startBottom;
                            if (clipped < allowedDelta)
                            {
                                allowedDelta = clipped;
                                hitSolid = true;
                                hitBody = other;
                            }
                        }
                    }
                    else // delta < 0 (moving upward)
                    {
                        float startTop = startPos.Y - halfSize.Y;

                        if (startTop >= otherBounds.Bottom - epsilon && startTop + delta < otherBounds.Bottom)
                        {
                            float clipped = otherBounds.Bottom - startTop;
                            if (clipped > allowedDelta)
                            {
                                allowedDelta = clipped;
                                hitSolid = true;
                                hitBody = other;
                            }
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
                        body.IsGrounded = true;
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

            // Projectiles (kunai, enemy shots, rocks) only interact via
            // their Hitbox - never via physical push-apart. Without this,
            // a projectile spawned next to its owner gets shoved and its
            // velocity zeroed here, which ProjectileScript then reads as
            // "stopped by a wall" and despawns on the spot.
            if (a.IsProjectile || b.IsProjectile)
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
