namespace RoyaResan.Mono2d.Nodes;

public class Node
{
    public string Name;
    public Node Parent;
    public List<Node> Children = new();

    public virtual void Update(GameTime gameTime)
    {
        for (int i = 0; i < Children.Count; i++)
            Children[i].Update(gameTime);
    }

    public virtual void Draw(Renderer renderer)
    {
        for (int i = 0; i < Children.Count; i++)
            Children[i].Draw(renderer);
    }

    public void AddChild(Node node)
    {
        node.Parent = this;
        Children.Add(node);
    }
}
