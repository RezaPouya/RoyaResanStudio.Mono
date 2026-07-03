using RoyaResan.Mono2d.Physics;

namespace RoyaResan.Mono2d.Scripting
{
    public abstract class Script
    {
        public PhysicsBody Owner;

        public bool Started { get; private set; }

        public void InternalStart()
        {
            if (Started) return;
            Started = true;
            Start();
        }

        public virtual void Start() { }

        public virtual void Update(GameTime gameTime) { }
    }
}