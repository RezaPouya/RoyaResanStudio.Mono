namespace RoyaResan.Mono2d.Node;

public abstract class ScriptNode : Node
{
    public virtual void Start() { }

    public override void Update(float dt)
    {
        OnUpdate(dt);

        base.Update(dt);
    }

    protected abstract void OnUpdate(float dt);
}
