using RoyaResan.Mono2d.Components;

namespace RoyaResan.Mono2d.Node;


public abstract class Node
{
    public string Name { get; set; } = string.Empty;

    public Node Parent { get; private set; }

    private readonly List<Node> _children = new();

    public IReadOnlyList<Node> Children => _children;

    private readonly List<Component> _components = new();

    public virtual void AddChild(Node node)
    {
        if (node == null || node == this)
            return;

        node.Parent = this;
        _children.Add(node);
    }

    public virtual void RemoveChild(Node node)
    {
        if (_children.Remove(node))
        {
            node.Parent = null;
        }
    }

    public virtual void Draw()
    {
        for (int i = 0; i < _children.Count; i++)
        {
            _children[i].Draw();
        }
    }




    public T AddComponent<T>(T component) where T : Component
    {
        _components.Add(component);
        component.Attach(this);
        return component;
    }

    public T GetComponent<T>() where T : Component
    {
        return _components.OfType<T>().FirstOrDefault();
    }

    public virtual void Update(float dt)
    {
        for (int i = 0; i < _components.Count; i++)
            _components[i].Update(dt);

        for (int i = 0; i < _children.Count; i++)
            _children[i].Update(dt);
    }
}