namespace RoyaResan.Mono2d.AI.States;

/// <summary>
/// Not reached through normal transitions - force this state directly
/// from a Health.OnDamaged handler (see EnemyWiringExample) so getting
/// hit actually interrupts patrol/chase/attack instead of the enemy
/// ignoring damage entirely.
/// </summary>
public class StaggerState : EnemyState
{
    public string AnimationState = "Stagger";
    public float Duration = 0.3f;
    public string RecoverToState = "Chase";

    private float _timer;

    public override void Enter()
    {
        _timer = 0f;
        Machine.Animator?.Play(AnimationState, 0.05f);
        Machine.Body.Velocity = Vector2.Zero;
    }

    public override void Update(GameTime gameTime)
    {
        _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_timer >= Duration)
            Machine.ChangeState(RecoverToState);
    }
}
