using Microsoft.Xna.Framework;
using RoyaResan.Mono2d.Nodes;
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
    public void Update(GameTime gameTime)
    {
        if (NodeRoot == null)
            return;

        NodeRoot.Update(gameTime);
    }

    // -----------------------------
    // RENDER COLLECTION
    // -----------------------------
    public void CollectRenderables()
    {
        _renderQueue.Clear();
        if (NodeRoot != null)
            CollectSprites(NodeRoot);
    }

    private void CollectSprites(Node node)
    {
        if (node is SpriteNode sprite)
            _renderQueue.Add(sprite);

        foreach (var child in node.Children)
            CollectSprites(child);
    }

    // -----------------------------
    // RENDER (DEPTH SORTED)
    // -----------------------------
    public void Draw(Render renderer)
    {
        if (NodeRoot == null)
            return;

        CollectRenderables();

        var sorted = _renderQueue
            .OrderBy(s => s.GlobalPosition.Y)
            .ToList();

        renderer.Begin();

        for (int i = 0; i < sorted.Count; i++)
        {
            renderer.DrawSprite(sorted[i]);
        }

        renderer.End();
    }
}