using System;

namespace RoyaResan.Mono2d.Combat;

/// <summary>
/// Standalone health component - attach a reference to it from any
/// PhysicsBody (or elsewhere) rather than baking HP into the entity
/// classes themselves.
/// </summary>
public class Health
{
    public int Max;
    public int Current;
    public bool IsDead => Current <= 0;

    /// <summary>Fires on every successful hit: (amount, source that dealt it).</summary>
    public event Action<int, PhysicsBody> OnDamaged;
    public event Action OnDeath;

    public Health(int max = 100)
    {
        Max = max;
        Current = max;
    }

    public void Damage(int amount, PhysicsBody source = null)
    {
        if (IsDead || amount <= 0)
            return;

        Current = Math.Max(0, Current - amount);
        OnDamaged?.Invoke(amount, source);

        if (IsDead)
            OnDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0)
            return;

        Current = Math.Min(Max, Current + amount);
    }
}
