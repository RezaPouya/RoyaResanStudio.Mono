using RoyaResan.Mono2d.AI;
using RoyaResan.Mono2d.AI.States;
using RoyaResan.Mono2d.Core;
using RoyaResan.Mono2d.Inputs;
using RoyaResan.Mono2d.Physics;
using RoyaResan.Mono2d.Scripting;

namespace RoyaResan.Mono2d.Gameplay;

/// <summary>
/// Toggles the player's StealthModifier while Ctrl is held - crouching
/// halves detection range and slightly slows movement (adjust to taste).
/// </summary>
public class CrouchScript : Script
{
    public StealthModifier Stealth;
    public float CrouchVisibility = 0.5f;

    public override void Update(GameTime gameTime)
    {
        bool crouching = Input.IsKeyDown(Keys.LeftControl);
        Stealth.VisibilityMultiplier = crouching ? CrouchVisibility : 1f;
    }
}

/// <summary>
/// USAGE EXAMPLE - not wired into World.cs automatically. Builds one
/// patrolling enemy using a real VisionCone (not the plain-distance
/// fallback), gives the player a StealthModifier + crouch, and fires a
/// noise event that alerts the group even with no line of sight.
/// </summary>
public static class StealthWiringExample
{
    public static StealthModifier SetupPlayerStealth(PhysicsBody player)
    {
        var stealth = new StealthModifier();
        player.Stealth = stealth;
        player.AddScript(new CrouchScript { Stealth = stealth });
        return stealth;
    }

    public static void BuildPatrollingGuard(Scene scene, CombatGroup group, PhysicsBody player)
    {
        var guard = new PhysicsBody { Position = new Vector2(400, 100) };
        guard.Collider = new Collider { Owner = guard, Size = new Vector2(32, 32) };

        var vision = new VisionCone
        {
            Range = 180f,
            HalfAngleDegrees = 50f,
            FacingDirection = Vector2.UnitX
        };

        var fsm = new EnemyFsm { Group = group, World = scene.Physics };
        fsm.AddState("Patrol", new PatrolState
        {
            LeftBound = 350, RightBound = 550, Speed = 50f,
            Vision = vision
        });
        fsm.AddState("Idle", new IdleState { Vision = vision });
        fsm.AddState("Chase", new ChaseState { Speed = 90f, AttackRange = 36f });
        fsm.ChangeState("Patrol");

        guard.AddScript(new EnemyFsmScript { Fsm = fsm });

        group.Join(fsm);
        scene.AddBody(guard);
    }

    /// <summary>
    /// Call this wherever a loud action happens (e.g. a running footstep
    /// past a certain speed, or a thrown object landing) - alerts the
    /// group within radius even if no guard currently has line of sight.
    /// </summary>
    public static void EmitFootstepNoise(CombatGroup group, PhysicsBody source)
    {
        NoiseSystem.Emit(group, source.GlobalPosition, radius: 120f, target: source);
    }
}
