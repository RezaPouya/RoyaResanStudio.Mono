using RoyaResan.Mono2d.AI;
using RoyaResan.Mono2d.Combat;
using RoyaResan.Mono2d.Scripting;

namespace RoyaResan.Mono2d.Gameplay;

/// <summary>
/// Directional block for the Shield/Spear enemy:
/// - Blocks "Kunai" only when the player is in front AND within BlockRange.
/// - Sword (melee) always goes through.
/// - Attack from behind or during Stagger → no blocking.
/// </summary>
public class ShieldBlockScript : Script
{
    public EnemyFsm Fsm;
    public Hurtbox Hurtbox;
    public PhysicsBody Player;

    /// <summary>Maximum distance at which the shield will block kunai.</summary>
    public float BlockRange = 1f;

    /// <summary>
    /// Optional purely-cosmetic visual (e.g. the placeholder rect showing
    /// which way the shield/spear enemy is facing). If set, its local X
    /// offset is flipped to match Fsm.FacingDirection every frame - pass
    /// the offset it should sit at when facing right (positive X); it
    /// will be mirrored automatically when facing left.
    /// </summary>
    public TransformNode ShieldVisual;
    public float ShieldVisualOffsetX;

    private static readonly HashSet<string> BlockKunai = new() { "Kunai" };

    public override void Update(GameTime gameTime)
    {
        if (Player == null || Hurtbox == null || Fsm == null)
            return;

        if (ShieldVisual != null)
        {
            float facingSign = Fsm.FacingDirection.X >= 0f ? 1f : -1f;
            ShieldVisual.Position = new Vector2(facingSign * Math.Abs(ShieldVisualOffsetX), ShieldVisual.Position.Y);
        }

        // No blocking while staggered or dead
        if (Fsm.CurrentStateName == "Stagger" || Fsm.CurrentStateName == "Dead")
        {
            Hurtbox.BlockedTags = null;
            return;
        }

        float toPlayer = Player.GlobalPosition.X - Owner.GlobalPosition.X;
        float distance = Math.Abs(toPlayer);

        // Only block if player is in front AND within range
        bool playerInFront = Math.Sign(toPlayer) == Math.Sign(Fsm.FacingDirection.X);
        bool inRange = distance <= BlockRange;

        Hurtbox.BlockedTags = (playerInFront && inRange) ? BlockKunai : null;
    }
}