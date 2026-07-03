using RoyaResan.Mono2d.Scripting;

namespace RoyaResan.Mono2d.Physics
{
    public class PhysicsBody : TransformNode
    {
        public Vector2 Velocity;

        public Collider Collider;

        public bool IsStatic = false;

        /// <summary>Off by default - existing top-down movement (e.g. PlayerMovementScript) is unaffected unless opted in.</summary>
        public bool UseGravity = false;

        /// <summary>Optional - null means fully visible (multiplier 1). See StealthModifier.</summary>
        public StealthModifier Stealth;

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

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!IsStatic)
            {
                if (UseGravity)
                    Velocity.Y += PhysicsSettings.Gravity * dt;

                Position += Velocity * dt;
            }

            base.Update(gameTime);
        }
    }
}