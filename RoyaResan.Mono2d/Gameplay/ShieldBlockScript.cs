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

    private static readonly HashSet<string> BlockKunai = new() { "Kunai" };

    public override void Update(GameTime gameTime)
    {
        if (Player == null || Hurtbox == null || Fsm == null)
            return;

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