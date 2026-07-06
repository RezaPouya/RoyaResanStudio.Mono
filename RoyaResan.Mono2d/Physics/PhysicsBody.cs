using RoyaResan.Mono2d.Scripting;

namespace RoyaResan.Mono2d.Physics
{
    public class PhysicsBody : TransformNode
    {
        public Vector2 Velocity;

        public Collider Collider;

        public bool IsStatic = false;

        /// <summary>
        /// True for a static body that moves itself each physics step (a
        /// moving platform). Still never affected by other bodies'
        /// collisions - same as any other IsStatic body - but
        /// PhysicsWorld.Step advances its position, carries any rider
        /// standing on top, and shoves aside any character it moves into
        /// from the side.
        /// </summary>
        public bool IsMovingPlatform = false;

        /// <summary>
        /// True for thrown/shot projectiles (kunai, enemy shots, rocks -
        /// anything driven by ProjectileScript). Set automatically by
        /// ProjectileScript.Start(). Excludes the body from
        /// PhysicsWorld.ResolveDynamicPair, since a projectile is only
        /// meant to interact via its Hitbox (damage), never via physical
        /// push-apart collision. Without this, a projectile spawned right
        /// next to its owner (or flying past another character) gets
        /// shoved and has its velocity zeroed by the dynamic-vs-dynamic
        /// pass, which ProjectileScript then reads as "stopped by a wall"
        /// and despawns immediately - the classic "kunai disappears right
        /// in front of the player" bug.
        /// </summary>
        public bool IsProjectile = false;

        /// <summary>
        /// The body (usually a moving platform) this body landed on as of
        /// the last grounding check - null if not resting on anything. Set
        /// by PhysicsWorld alongside IsGrounded; used to carry riders when
        /// StandingOn.IsMovingPlatform is true. Not meant to be set by
        /// gameplay code.
        /// </summary>
        public PhysicsBody StandingOn;

        /// <summary>
        /// Optional user-defined data. Useful for projectiles to remember their real owner (the player).
        /// </summary>
        public object UserData;

        /// <summary>
        /// True for one physics step if this body is resting on top of
        /// something solid (a normal collider it fell onto, or an active
        /// catch on a one-way platform). Set by PhysicsWorld each Step,
        /// reset to false at the start of the next Step before collisions
        /// are resolved - so it always reflects "was I grounded as of the
        /// last physics step," one frame behind, same as the rest of this
        /// engine's resolve-then-consume-next-frame pattern (see
        /// PreviousPosition). Meant to be read by movement scripts
        /// (coyote time, jump gating) - not meant to be set by gameplay code.
        /// </summary>
        public bool IsGrounded;

        /// <summary>Off by default - existing top-down movement (e.g. PlayerMovementScript) is unaffected unless opted in.</summary>
        public bool UseGravity = false;

        /// <summary>Optional - null means fully visible (multiplier 1). See StealthModifier.</summary>
        public StealthModifier Stealth;

        /// <summary>Used to prevent friendly fire between same-team entities (e.g. enemies don't hit each other).</summary>
        public string Team = "None";

        /// <summary>
        /// Position at the end of the previous physics step - cached by
        /// PhysicsWorld, used by one-way platform resolution to tell
        /// whether a body was already above/below the platform last
        /// frame, and by moving-platform carry to compute this step's
        /// platform delta. Not meant to be set by gameplay code.
        /// </summary>
        public Vector2 PreviousPosition;

        public List<Script> Scripts = new();

        public void AddScript(Script script)
        {
            script.Owner = this;
            Scripts.Add(script);
        }

        /// <summary>
        /// Advances this body's own position for one physics step. Empty by
        /// default - only overridden by kinematic bodies like moving
        /// platforms (see MovingPlatformNode). Called by PhysicsWorld.Step
        /// before gravity/sweep/carry for dynamic bodies, so riders and
        /// pushed characters see this step's motion immediately.
        /// </summary>
        public virtual void AdvanceKinematic(float dt) { }

        public override void Update(GameTime gameTime)
        {
            foreach (var script in Scripts)
            {
                script.InternalStart();
                script.Update(gameTime);
            }

            // NO movement/gravity here anymore!
            base.Update(gameTime);  // Still update children (visuals, etc.)
        }
    }
}
