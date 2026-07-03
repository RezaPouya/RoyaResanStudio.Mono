using System;

namespace RoyaResan.Mono2d.AI.States;

public class ChaseState : EnemyState
{
    public string AnimationState = "Run";
    public float Speed = 100f;
    public float AttackRange = 32f;

    public override void Enter() => Machine.Animator?.Play(AnimationState, 0.1f);

    public override void Update(GameTime gameTime)
    {
        var body = Machine.Body;
        var target = Machine.Group?.Target;

        if (target == null)
        {
            Machine.ChangeState("Idle");
            return;
        }

        float dx = target.GlobalPosition.X - body.GlobalPosition.X;
        float dist = Math.Abs(dx);

        if (dist <= AttackRange)
        {
            Machine.ChangeState("Attack");
            return;
        }

        float dir = dx < 0 ? -1f : 1f;
        body.Velocity = new Vector2(dir * Speed, body.Velocity.Y);
    }

    public override void Exit()
    {
        Machine.Body.Velocity = new Vector2(0, Machine.Body.Velocity.Y);
    }
}
