using Microsoft.Xna.Framework;

namespace RoyaResan.Mono2d.Nodes;

public abstract class Node
{
    public string Name { get; set; } = string.Empty;

    public Node Parent { get; private set; }

    private readonly List<Node> _children = new();
    public IReadOnlyList<Node> Children => _children;

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
            node.Parent = null;
    }

    public virtual void Update(GameTime gameTime)
    {
        for (int i = 0; i < _children.Count; i++)
            _children[i].Update(gameTime);
    }

    public virtual void Draw()
    {
        for (int i = 0; i < _children.Count; i++)
            _children[i].Draw();
    }
}