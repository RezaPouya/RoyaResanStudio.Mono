using RoyaResan.Mono2d.Combat;
using RoyaResan.Mono2d.Scripting;

namespace RoyaResan.Mono2d.Gameplay;

public class EnemyHealthBarScript : Script
{
    public Health Health;
    public PlaceholderRectNode Fill;
    public PlaceholderRectNode Bg;

    public override void Update(GameTime gameTime)
    {
        if (Health == null || Fill == null || Bg == null) return;

        float healthPct = Health.Max > 0 ? (float)Health.Current / Health.Max : 0f;
        Fill.Size = new Vector2(30 * healthPct, 2);
        Fill.Position = new Vector2(-15 + 15 * healthPct, Bg.Position.Y);  // left-aligned fill

        // Hide when dead
        bool visible = Health.Current > 0;
        Fill.Visible = visible;
        Bg.Visible = visible;
    }
}