using RoyaResan.Mono2d.AI;
using RoyaResan.Mono2d.Combat;
using RoyaResan.Mono2d.Scripting;

namespace RoyaResan.Mono2d.Gameplay;

/// <summary>
/// Directional block for the Shield/Spear enemy:
/// - While player is in front → blocks "Kunai"
/// - Sword (melee) always goes through
/// - Attack from behind or during Stagger → no blocking
/// </summary>
public class ShieldBlockScript : Script
{
    public EnemyFsm Fsm;
    public Hurtbox Hurtbox;
    public PhysicsBody Player;

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
        bool playerInFront = Math.Sign(toPlayer) == Math.Sign(Fsm.FacingDirection.X);

        // Block Kunai when player is in front, allow Sword
        Hurtbox.BlockedTags = playerInFront ? BlockKunai : null;
    }
}