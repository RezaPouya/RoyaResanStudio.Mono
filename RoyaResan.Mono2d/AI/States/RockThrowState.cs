using RoyaResan.Mono2d.Combat;
using RoyaResan.Mono2d.Gameplay;

namespace RoyaResan.Mono2d.AI.States;

public class RockThrowState : EnemyState
{
    public string AnimationState = "Attack";
    public float FireInterval = 3f;  // 5 seconds cooldown as requested
    public float HorizontalSpeed = 220f;
    public int Damage = 1;
    public float ReengageHeightThreshold = 40f;

    private float _timer;

    public override void Enter()
    {
        _timer = 0.5f; // Quick first throw
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

    private void Throw(PhysicsBody target)
    {
        if (Machine.Scene == null) return;

        var body = Machine.Body;
        Vector2 from = body.Position + new Vector2(0, -20f);
        Vector2 to = target.GlobalPosition;

        float dx = to.X - from.X;
        float dy = to.Y - from.Y;
        float dir = dx < 0 ? -1f : 1f;
        float distX = Math.Abs(dx);

        float time = Math.Max(distX / HorizontalSpeed, 0.5f);
        float vy = (dy - 0.5f * PhysicsSettings.Gravity * time * time) / time;
        float vx = dir * distX / time;

        var rock = new PhysicsBody { UseGravity = true, Team = "Enemy" };
        rock.Position = from;
        rock.Collider = new Collider { Owner = rock, Size = new Vector2(14f, 14f) };

        var visual = new Nodes.PlaceholderRectNode { Size = new Vector2(14f, 14f), Color = Color.SaddleBrown };
        rock.AddChild(visual);

        var hitbox = new Hitbox
        {
            Owner = rock,
            Damage = Damage,
            Size = new Vector2(14f, 14f),
            Tag = "EnemyRock",
            Knockback = new Vector2(60f, -40f)
        };

        rock.UserData = body; // Prevent self-damage

        Machine.Scene.AddBody(rock);
        Machine.Scene.AddHitbox(hitbox);

        rock.Velocity = new Vector2(vx, vy);
        rock.AddScript(new ProjectileScript { Scene = Machine.Scene, Hitbox = hitbox, StoppedSpeedThreshold = 5f, Lifetime = 4f });
    }
}