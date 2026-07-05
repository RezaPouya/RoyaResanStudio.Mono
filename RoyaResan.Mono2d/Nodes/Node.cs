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

    /// <summary>
    /// Detaches a child from the tree (it stops receiving Update/Draw).
    /// Does NOT remove it from PhysicsWorld/CombatWorld - for a
    /// PhysicsBody, use Scene.RemoveBody instead so both sides stay in
    /// sync. This exists purely for the tree side of despawning
    /// (projectiles, dead enemies, collected pickups).
    /// </summary>
    public void RemoveChild(Node node)
    {
        if (Children.Remove(node))
            node.Parent = null;
    }
}