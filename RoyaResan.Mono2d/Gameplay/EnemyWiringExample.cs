using RoyaResan.Mono2d.AI;
using RoyaResan.Mono2d.AI.States;
using RoyaResan.Mono2d.Animation;
using RoyaResan.Mono2d.Combat;
using RoyaResan.Mono2d.Core;

namespace RoyaResan.Mono2d.Gameplay;

/// <summary>
/// USAGE EXAMPLE - not wired into World.cs automatically. Builds two
/// enemies sharing one CombatGroup with MaxSimultaneousAttackers = 1, so
/// only one of them ever attacks at a time - the other waits its turn.
/// Also shows Health.OnDamaged forcing a Stagger, interrupting whatever
/// the enemy was doing.
/// </summary>
public static class EnemyWiringExample
{
    public static void BuildTwoEnemies(Scene scene, PhysicsBody player, Texture2D enemySheet)
    {
        var group = new CombatGroup { MaxSimultaneousAttackers = 1, Target = player };

        BuildEnemy(scene, group, enemySheet, spawnX: 300, leftBound: 250, rightBound: 400);
        BuildEnemy(scene, group, enemySheet, spawnX: 500, leftBound: 450, rightBound: 600);
    }

    private static void BuildEnemy(Scene scene, CombatGroup group, Texture2D sheet, float spawnX, float leftBound, float rightBound)
    {
        var enemy = new PhysicsBody
        {
            Position = new Vector2(spawnX, 100)
        };
        enemy.Collider = new Collider { Owner = enemy, Size = new Vector2(32, 32) };

        // --- Animation (adjust rows/columns to your actual sheet) ---
        var animator = new Animator();
        animator.AddState(new AnimationState { Name = "Idle", Clip = AnimationClip.FromGrid("idle", sheet, 32, 32, 4, 4, 0, 0, 0.15f, true) });
        animator.AddState(new AnimationState { Name = "Run", Clip = AnimationClip.FromGrid("run", sheet, 32, 32, 6, 6, 0, 32, 0.08f, true) });
        animator.AddState(new AnimationState { Name = "Attack", Clip = AnimationClip.FromGrid("attack", sheet, 32, 32, 5, 5, 0, 64, 0.06f, false) });
        animator.AddState(new AnimationState { Name = "Stagger", Clip = AnimationClip.FromGrid("stagger", sheet, 32, 32, 2, 2, 0, 96, 0.1f, false) });
        animator.Play("Idle");

        var spriteNode = new SpriteNode { Animator = animator };
        enemy.AddChild(spriteNode);

        // --- Combat ---
        var health = new Health(30);
        var hurtbox = new Hurtbox { Owner = enemy, Size = new Vector2(32, 32), Health = health };
        var hitbox = new Hitbox { Owner = enemy, Size = new Vector2(24, 16), Offset = new Vector2(20, 0), Damage = 8 };
        scene.AddHurtbox(hurtbox);
        scene.AddHitbox(hitbox);

        // --- AI ---
        var fsm = new EnemyFsm { Animator = animator, Group = group };
        fsm.AddState("Idle", new IdleState { VisionRange = 150f });
        fsm.AddState("Patrol", new PatrolState { LeftBound = leftBound, RightBound = rightBound, Speed = 50f, VisionRange = 150f });
        fsm.AddState("Chase", new ChaseState { Speed = 90f, AttackRange = 36f });
        fsm.AddState("Attack", new AttackState { Hitbox = hitbox, AttackRange = 36f });
        fsm.AddState("Stagger", new StaggerState { Duration = 0.3f, RecoverToState = "Chase" });
        fsm.ChangeState("Patrol");

        // Getting hit interrupts whatever the enemy was doing.
        health.OnDamaged += (amount, source) => fsm.ChangeState("Stagger", force: true);

        enemy.AddScript(new EnemyFsmScript { Fsm = fsm });

        group.Join(fsm);
        scene.AddBody(enemy);
    }
}