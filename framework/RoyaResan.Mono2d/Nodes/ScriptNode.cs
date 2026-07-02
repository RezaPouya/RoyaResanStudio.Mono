using Microsoft.Xna.Framework;

namespace RoyaResan.Mono2d.Nodes;

public abstract class ScriptNode : Node
{
    private bool _started;

    public virtual void Start() { }

    public sealed override void Update(GameTime gameTime)
    {
        if (!_started)
        {
            Start();
            _started = true;
        }

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        OnUpdate(dt);
        base.Update(gameTime);
    }

    protected abstract void OnUpdate(float dt);
}