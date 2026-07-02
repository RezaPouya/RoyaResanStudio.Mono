namespace RoyaResan.Mono2d.Components;

using RoyaResan.Mono2d.Node;

public abstract class Component
{
    public Node Owner { get; private set; }

    internal void Attach(Node owner)
    {
        Owner = owner;
        OnAttach();
    }

    internal void Detach()
    {
        OnDetach();
        Owner = null;
    }

    protected virtual void OnAttach() { }
    protected virtual void OnDetach() { }

    public virtual void Update(float dt) { }
}