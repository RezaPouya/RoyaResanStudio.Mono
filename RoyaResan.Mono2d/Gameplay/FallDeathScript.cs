using RoyaResan.Mono2d.Scripting;

namespace RoyaResan.Mono2d.Gameplay;

/// <summary>
/// Attach to any PhysicsBody (player or enemy) to kill it once it falls
/// past a Y threshold - the standard platformer "fell into a pit" death.
/// Deliberately decoupled from Health: the player doesn't have a Health
/// component in these examples, and a fall death isn't a combat hit, so
/// this works uniformly for both. Wire OnFallDeath to whatever "death"
/// means for this particular body - respawn the player, scene.RemoveBody
/// an enemy, Health.Damage(int.MaxValue) if you want it to also count as
/// a combat kill, etc.
/// </summary>
public class FallDeathScript : Script
{
    /// <summary>World-space Y below which the owner is considered fallen.</summary>
    public float DeathY = 1000f;

    /// <summary>Fires once, the instant the owner crosses DeathY.</summary>
    public Action OnFallDeath;

    private bool _dead;

    public override void Update(GameTime gameTime)
    {
        if (_dead)
            return;

        if (Owner.GlobalPosition.Y > DeathY)
        {
            _dead = true;
            OnFallDeath?.Invoke();
        }
    }

    /// <summary>Call after a respawn so this same script instance can trigger again.</summary>
    public void Reset()
    {
        _dead = false;
    }
}
