using RoyaResan.Mono2d.Node;
using RoyaResan.Mono2d.Rendering;

namespace RoyaResan.Mono2d.Scene;

public class Scene
{
    public string Name { get; set; } = string.Empty;

    public Node NodeRoot { get; private set; } = new SceneNode();

    private readonly List<SpriteNode> _renderQueue = new();

    public bool IsActive { get; private set; } = true;

    // -----------------------------
    // LIFECYCLE
    // -----------------------------
    public virtual void Load() { }

    public virtual void Unload() { }

    // -----------------------------
    // UPDATE
    // -----------------------------
    public virtual void Update(float dt)
    {
        NodeRoot.Update(dt);
    }

    // -----------------------------
    // RENDER COLLECTION
    // -----------------------------
    public void CollectRenderables()
    {
        _renderQueue.Clear();

        CollectSprites(NodeRoot);
    }

    private void CollectSprites(Node node)
    {
        if (node is SpriteNode sprite)
        {
            _renderQueue.Add(sprite);
        }

        foreach (var child in node.Children)
        {
            CollectSprites(child);
        }
    }

    // -----------------------------
    // RENDER (DEPTH SORTED)
    // -----------------------------
    public void Draw(Render renderer)
    {
        CollectRenderables();

        var sorted = _renderQueue
            .OrderBy(s => s.GlobalPosition.Y) // 🔥 depth = Y-axis (classic 2.5D)
            .ToList();

        renderer.Begin();

        foreach (var sprite in sorted)
        {
            renderer.DrawSprite(sprite);
        }

        renderer.End();
    }
}