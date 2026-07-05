using RoyaResan.Mono2d.Scripting;

namespace RoyaResan.Mono2d.Physics
{
    public class PhysicsBody : TransformNode
    {
        public Vector2 Velocity;

        public Collider Collider;

        public bool IsStatic = false;

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
        /// frame. Not meant to be set by gameplay code.
        /// </summary>
        public Vector2 PreviousPosition;

        public List<Script> Scripts = new();

        public void AddScript(Script script)
        {
            script.Owner = this;
            Scripts.Add(script);
        }

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