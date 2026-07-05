using RoyaResan.Mono2d.Combat;

namespace RoyaResan.Mono2d.AI.States;

/// <summary>
/// Reached by forcing Machine.ChangeState("Dead", force: true) from a
/// Health.OnDeath handler (same convention as StaggerState being forced
/// from OnDamaged). Freezes the body, makes the corpse untouchable, waits
/// DespawnDelay so the "Dead" placeholder pose is actually visible, then
/// removes the body/hurtbox/hitbox from the scene via Scene.RemoveBody
/// etc. Needs Machine.Scene set.
/// </summary>
public class DeadState : EnemyState
{
    public string AnimationState = "Dead";
    public float DespawnDelay = 0.6f;

    /// <summary>This enemy's own hurtbox - set Invulnerable so nothing can double-kill it mid-despawn, and removed from CombatWorld along with the body.</summary>
    public Hurtbox Hurtbox;

    /// <summary>Any hitboxes this enemy owned (melee swing, ranged shot) - removed alongside the body so CombatWorld stops carrying them forever.</summary>
    public Hitbox[] Hitboxes = System.Array.Empty<Hitbox>();

    private float _timer;
    private bool _despawned;

    public override void Enter()
    {
        _timer = 0f;
        _despawned = false;

        Machine.Animator?.Play(AnimationState, 0.05f);
        Machine.Body.Velocity = Vector2.Zero;

        if (Hurtbox != null)
            Hurtbox.Invulnerable = true;
    }

    public override void Update(GameTime gameTime)
    {
        if (_despawned)
            return;

        _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_timer < DespawnDelay)
            return;

        _despawned = true;

        if (Hurtbox != null)
            Machine.Scene?.RemoveHurtbox(Hurtbox);

        foreach (var hitbox in Hitboxes)
            Machine.Scene?.RemoveHitbox(hitbox);

        Machine.Scene?.RemoveBody(Machine.Body);
    }
}