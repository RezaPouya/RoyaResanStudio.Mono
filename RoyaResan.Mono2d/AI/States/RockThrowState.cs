using RoyaResan.Mono2d.Combat;
using RoyaResan.Mono2d.Gameplay;

namespace RoyaResan.Mono2d.AI.States;

/// <summary>
/// Entered by ChaseState when the target is too far above/below to reach
/// by walking (CanThrowAtRange + RockThrowHeightThreshold). Throws a
/// gravity-affected rock on a real ballistic arc rather than a straight
/// shot - see Throw() for the trajectory math - and hands back to Chase
/// the moment the height gap closes again.
/// </summary>
public class RockThrowState : EnemyState
{
    public string AnimationState = "Attack";
    public float FireInterval = 1.6f;

    /// <summary>
    /// Not the rock's actual launch speed - used to derive how long the
    /// throw should take to cover the horizontal distance, which the
    /// vertical launch velocity is then solved against. Lower = lobbier,
    /// higher = flatter/faster arc.
    /// </summary>
    public float HorizontalSpeed = 220f;

    public int Damage = 1;

    /// <summary>Once the height gap closes to less than this, hand back to Chase instead of continuing to throw.</summary>
    public float ReengageHeightThreshold = 40f;

    private float _timer;

    public override void Enter()
    {
        _timer = FireInterval; // throw almost immediately on entering, not after a full wait
        Machine.Animator?.Play(AnimationState, 0.1f);
    }

    public override void Update(GameTime gameTime)
    {
        var body = Machine.Body;
        var target = Machine.Group?.Target;

        if (target == null)
        {
            Machine.ChangeState("Idle");
            return;
        }

        float dir = target.GlobalPosition.X >= body.GlobalPosition.X ? 1f : -1f;
        Machine.FacingDirection = new Vector2(dir, 0);

        _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_timer >= FireInterval)
        {
            _timer = 0f;
            Throw(target);
        }

        float heightDiff = Math.Abs(target.GlobalPosition.Y - body.GlobalPosition.Y);
        if (heightDiff < ReengageHeightThreshold)
            Machine.ChangeState("Chase");
    }

    /// <summary>
    /// Real ballistic arc, not a straight/homing shot: pick a fixed
    /// horizontal speed, derive the time it takes to cover the horizontal
    /// gap at that speed, then solve the vertical launch velocity that
    /// lands exactly on the target's CURRENT position at that time given
    /// gravity (y = v0*t + 0.5*g*t^2, solved for v0). If the target keeps
    /// moving after the throw it'll miss slightly - that's expected/fine,
    /// not something to compensate for (a little inaccuracy reads as more
    /// natural than a homing rock).
    /// </summary>
    private void Throw(PhysicsBody target)
    {
        if (Machine.Scene == null)
            return;

        var body = Machine.Body;

        Vector2 from = body.Position + new Vector2(0, -20f);
        Vector2 to = target.GlobalPosition;

        float dx = to.X - from.X;
        float dy = to.Y - from.Y;
        float dir = dx < 0 ? -1f : 1f;
        float distX = Math.Abs(dx);

        float time = Math.Max(distX / HorizontalSpeed, 0.35f); // floor avoids a near-vertical, absurdly fast toss when the target is almost directly above/below
        float vy = (dy - 0.5f * PhysicsSettings.Gravity * time * time) / time;
        float vx = dir * distX / time;

        var rock = new PhysicsBody { UseGravity = true };
        rock.Position = from;
        rock.Collider = new Collider { Owner = rock, Size = new Vector2(12f, 12f) };

        var visual = new Nodes.PlaceholderRectNode { Size = new Vector2(12f, 12f), Color = Color.SaddleBrown };
        rock.AddChild(visual);

        var hitbox = new Hitbox
        {
            Owner = rock,
            Damage = Damage,
            Size = new Vector2(12f, 12f),
            Tag = "EnemyRock",
            Knockback = new Vector2(60f, -40f)
        };

        Machine.Scene.AddBody(rock);
        Machine.Scene.AddHitbox(hitbox);

        rock.Velocity = new Vector2(vx, vy);
        rock.AddScript(new ProjectileScript { Scene = Machine.Scene, Hitbox = hitbox, StoppedSpeedThreshold = 5f });
    }
}
