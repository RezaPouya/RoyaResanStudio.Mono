using RoyaResan.Mono2d.AI;
using RoyaResan.Mono2d.Combat;
using RoyaResan.Mono2d.Scripting;

namespace RoyaResan.Mono2d.Gameplay;

/// <summary>
/// Directional block for the Shield/Spear enemy: while the player is in
/// front of the enemy's FacingDirection, Hurtbox.BlockedTags blocks
/// "Sword" (frontal melee bounces off) but never "Kunai" (a thrown kunai
/// always lands and is expected to trigger Stagger via the enemy's own
/// Health.OnDamaged wiring - same as any other enemy). Attack from
/// behind, or while the enemy is in its Stagger state, and everything
/// lands normally.
///
/// This only works correctly with a single attacker (the player) - it
/// reads Player.GlobalPosition directly rather than reacting to whichever
/// hitbox actually connects, which is the honest, simple version of
/// directional blocking this prototype needs rather than a full per-
/// attacker system.
/// </summary>
public class ShieldBlockScript : Script
{
    public EnemyFsm Fsm;
    public Hurtbox Hurtbox;
    public PhysicsBody Player;

    private static readonly HashSet<string> BlockSword = new() { "Sword" };

    public override void Update(GameTime gameTime)
    {
        if (Player == null || Hurtbox == null || Fsm == null)
            return;

        if (Fsm.CurrentStateName == "Stagger" || Fsm.CurrentStateName == "Dead")
        {
            Hurtbox.BlockedTags = null;
            return;
        }

        float toPlayer = Player.GlobalPosition.X - Owner.GlobalPosition.X;
        bool playerInFront = Math.Sign(toPlayer) == Math.Sign(Fsm.FacingDirection.X);

        Hurtbox.BlockedTags = playerInFront ? BlockSword : null;
    }
}
