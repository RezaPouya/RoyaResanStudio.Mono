
namespace RoyaResan.Mono2d.Rendering;

public class Renderer
{
    private SpriteBatch _batch;

    public Renderer(SpriteBatch batch)
        => _batch = batch;

    public void Begin() => _batch.Begin();
    public void End() => _batch.End();

    public void Draw(Texture2D tex, Vector2 pos)
        => _batch.Draw(tex, pos, Color.White);
}