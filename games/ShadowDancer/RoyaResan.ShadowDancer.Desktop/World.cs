using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RoyaResan.Mono2d.Animation;
using RoyaResan.Mono2d.AI;
using RoyaResan.Mono2d.AI.States;
using RoyaResan.Mono2d.Combat;
using RoyaResan.Mono2d.Core;
using RoyaResan.Mono2d.Gameplay;
using RoyaResan.Mono2d.Graphics;
using RoyaResan.Mono2d.Inputs;
using RoyaResan.Mono2d.Nodes;
using RoyaResan.Mono2d.Physics;

namespace RoyaResan.ShadowDancer.Desktop;

public class World : Game
{
    private GraphicsDeviceManager _graphics;
    private Renderer _renderer;

    private Scene _scene;

    // Kept around so other systems (HUD, later phases) can read them
    // without hunting through the node tree.
    private PhysicsBody _player;
    private Health _playerHealth;
    private KunaiThrowScript _playerKunai;
    private SwordAttackScript _playerSword;

    private CombatGroup _enemyGroup;

    public World()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void LoadContent()
    {
        var spriteBatch = new SpriteBatch(GraphicsDevice);
        _renderer = new Renderer(spriteBatch);

        _scene = new Scene();

        // Renderer needs to know about the camera to convert world
        // positions to screen positions - this was never wired before,
        // meaning camera follow/shake had no visible effect on drawing.
        _renderer.Camera = _scene.Camera;

        // Camera.Position represents the world point shown at
        // screen-center, not screen-origin - must be set once the
        // graphics device (and therefore Viewport) exists.
        _scene.Camera.ScreenCenter = new Vector2(
            GraphicsDevice.Viewport.Width / 2f,
            GraphicsDevice.Viewport.Height / 2f);

        BuildTemporaryTestFloor();
        BuildPlayer();
        BuildEnemies();

        _scene.Camera.FollowTarget = _player;
        _scene.Camera.FollowSmoothing = 8f;
    }

    // -----------------------------------------------------------------
    // TEMP - a bare floor + side walls so movement/jump/gravity can be
    // playtested before a real Tiled level exists. Delete this method
    // (and its call above) once Level 1 loads from Tiled instead.
    // -----------------------------------------------------------------
    // In BuildTemporaryTestFloor() - expanded for walls/floor solidity
    private void BuildTemporaryTestFloor()
    {
        // Floor
        for (int i = 0; i < 20; i++)  // Wider floor
        {
            var floorTile = new WallNode();
            floorTile.Position = new Vector2(32 + i * 64, 500);
            _scene.AddBody(floorTile);
            var visual = new PlaceholderRectNode { Size = new Vector2(64, 64), Color = Color.DimGray };
            floorTile.AddChild(visual);
        }

        // Side walls (simple tall static walls)
        for (int i = 0; i < 10; i++)
        {
            var leftWall = new WallNode();
            leftWall.Position = new Vector2(32, 500 - (i * 64) - 32);  // Stack upward from floor
            _scene.AddBody(leftWall);
            leftWall.AddChild(new PlaceholderRectNode { Size = new Vector2(64, 64), Color = Color.DarkGray });

            var rightWall = new WallNode();
            rightWall.Position = new Vector2(32 + 19 * 64, 500 - (i * 64) - 32);
            _scene.AddBody(rightWall);
            rightWall.AddChild(new PlaceholderRectNode { Size = new Vector2(64, 64), Color = Color.DarkGray });
        }

        // Step platform (unchanged)
        var stepPlatform = new OneWayPlatformNode();
        stepPlatform.Position = new Vector2(400, 400);
        _scene.AddBody(stepPlatform);
        stepPlatform.AddChild(new PlaceholderRectNode { Size = new Vector2(64, 16), Color = Color.SaddleBrown });
    }

    // -----------------------------------------------------------------
    // PLAYER
    // -----------------------------------------------------------------
    private void BuildPlayer()
    {
        _player = new PhysicsBody();
        _player.Position = new Vector2(250, 250);

        _player.Collider = new Collider
        {
            Owner = _player,
            Size = new Vector2(24, 48)
        };

        var movement = new PlatformerMovementScript();
        _player.AddScript(movement);

        // --- placeholder visual, driven by the Animator's state name ---
        var visual = new PlaceholderRectNode { Color = Color.Red };
        _player.AddChild(visual);

        var animator = BuildPlayerAnimator();

        var pose = new PlaceholderPoseScript { Animator = animator, Visual = visual };
        pose.States["Idle"] = new PlaceholderPose(new Vector2(24, 48), Vector2.Zero, Color.White);
        pose.States["Run"] = new PlaceholderPose(new Vector2(26, 46), new Vector2(0, 1), Color.White);
        pose.States["Jump"] = new PlaceholderPose(new Vector2(22, 48), Vector2.Zero, Color.LightSkyBlue);
        pose.States["Fall"] = new PlaceholderPose(new Vector2(22, 48), Vector2.Zero, Color.SteelBlue);
        pose.States["Crouch"] = new PlaceholderPose(new Vector2(24, 28), new Vector2(0, 10), Color.White);
        pose.States["Attack"] = new PlaceholderPose(new Vector2(24, 48), Vector2.Zero, Color.Yellow);
        pose.States["Throw"] = new PlaceholderPose(new Vector2(24, 48), Vector2.Zero, Color.Orange);
        pose.States["Dead"] = new PlaceholderPose(new Vector2(30, 20), new Vector2(0, 14), Color.DarkRed);
        pose.Default = pose.States["Idle"];
        _player.AddScript(pose);

        // --- health + hurtbox ---
        _playerHealth = new Health(5);

        var hurtbox = new Hurtbox
        {
            Owner = _player,
            Size = new Vector2(24, 48),
            Health = _playerHealth
        };
        _scene.AddHurtbox(hurtbox);

        // --- sword ---
        var swordHitbox = new Hitbox { Owner = _player, Damage = 1, Tag = "Sword" };
        _scene.AddHitbox(swordHitbox);

        _playerSword = new SwordAttackScript
        {
            Hitbox = swordHitbox,
            Movement = movement,
            Animator = animator
        };
        _player.AddScript(_playerSword);

        // --- kunai ---
        _playerKunai = new KunaiThrowScript
        {
            Scene = _scene,
            Movement = movement,
            Animator = animator
        };
        _player.AddScript(_playerKunai);

        // --- animator state driver (movement -> state name) ---
        // Runs last so it reads this frame's grounded/velocity/attack state.
        var animatorDriver = new PlayerAnimatorDriverScript
        {
            Animator = animator,
            Movement = movement,
            Sword = _playerSword,
            Kunai = _playerKunai
        };
        _player.AddScript(animatorDriver);

        _playerHealth.OnDeath += () => animatorDriver.IsDead = true;

        _scene.AddBody(_player);
    }

    private Animator BuildPlayerAnimator()
    {
        var animator = new Animator();

        foreach (var name in new[] { "Idle", "Run", "Jump", "Fall", "Crouch", "Attack", "Throw", "Dead" })
            animator.AddState(new AnimationState { Name = name, Clip = new AnimationClip { Name = name, FrameTime = 0.1f, Loop = true } });

        animator.Play("Idle");
        return animator;
    }

    // -----------------------------------------------------------------
    // ENEMIES
    // -----------------------------------------------------------------
    private void BuildEnemies()
    {
        _enemyGroup = new CombatGroup { Target = _player, MaxSimultaneousAttackers = 1 };

        BuildMeleeEnemy(new Vector2(500, 460), leftBound: 450, rightBound: 650);
        BuildMeleeEnemy(new Vector2(900, 460), leftBound: 850, rightBound: 1050);
        BuildMeleeEnemy(new Vector2(1300, 460), leftBound: 1250, rightBound: 1450);

        BuildRangedEnemy(new Vector2(1100, 460));

        BuildShieldEnemy(new Vector2(700, 460), leftBound: 650, rightBound: 780);
    }

    /// <summary>
    /// Shared setup every enemy archetype needs: body, collider, placeholder
    /// visual, Health, Hurtbox, the EnemyFsm/EnemyFsmScript pair, and a Dead
    /// state wired to Health.OnDeath. Archetype-specific states (Patrol,
    /// Attack, RangedAttack, ShieldBlockScript, ...) are added by each
    /// Build*Enemy method on top of what this returns.
    /// </summary>
    private (PhysicsBody body, Health health, Hurtbox hurtbox, EnemyFsm fsm, Animator animator) BuildEnemyBase(
        Vector2 position, Vector2 size, int maxHealth, Color color)
    {
        var body = new PhysicsBody { Position = position, UseGravity = true };
        body.Collider = new Collider { Owner = body, Size = size };

        var visual = new PlaceholderRectNode { Size = size, Color = color };
        body.AddChild(visual);

        var animator = new Animator();
        foreach (var name in new[] { "Idle", "Run", "Attack", "Stagger", "Dead" })
            animator.AddState(new AnimationState { Name = name, Clip = new AnimationClip { Name = name, FrameTime = 0.1f, Loop = true } });
        animator.Play("Idle");

        var health = new Health(maxHealth);
        var hurtbox = new Hurtbox { Owner = body, Size = size, Health = health };
        _scene.AddHurtbox(hurtbox);

        var fsm = new EnemyFsm { Animator = animator, Group = _enemyGroup, World = _scene.Physics, Scene = _scene };
        _enemyGroup.Join(fsm);

        fsm.AddState("Dead", new DeadState { Hurtbox = hurtbox });
        health.OnDeath += () => fsm.ChangeState("Dead", force: true);

        body.AddScript(new EnemyFsmScript { Fsm = fsm });

        _scene.AddBody(body);

        return (body, health, hurtbox, fsm, animator);
    }

    private void BuildMeleeEnemy(Vector2 position, float leftBound, float rightBound)
    {
        var (body, health, hurtbox, fsm, _) = BuildEnemyBase(position, new Vector2(28, 48), maxHealth: 3, Color.IndianRed);

        var meleeHitbox = new Hitbox { Owner = body, Damage = 1, Size = new Vector2(24, 20), Tag = "EnemyMelee" };
        _scene.AddHitbox(meleeHitbox);

        var vision = new VisionCone { Range = 220f, HalfAngleDegrees = 60f };

        fsm.AddState("Idle", new IdleState { Vision = vision, NextState = "Patrol" });
        fsm.AddState("Patrol", new PatrolState { LeftBound = leftBound, RightBound = rightBound, Speed = 60f, Vision = vision });
        fsm.AddState("Chase", new ChaseState { Speed = 140f, AttackRange = 30f });
        fsm.AddState("Attack", new AttackState { Hitbox = meleeHitbox, AttackRange = 30f, AttackDuration = 0.4f, ActiveWindowStart = 0.12f, ActiveWindowEnd = 0.25f });
        fsm.AddState("Stagger", new StaggerState { Duration = 0.3f, RecoverToState = "Chase" });

        health.OnDamaged += (_, _) => { if (!health.IsDead) fsm.ChangeState("Stagger"); };

        fsm.ChangeState("Idle", force: true);
    }

    private void BuildRangedEnemy(Vector2 position)
    {
        var (body, health, hurtbox, fsm, _) = BuildEnemyBase(position, new Vector2(24, 40), maxHealth: 2, Color.MediumPurple);

        var vision = new VisionCone { Range = 320f, HalfAngleDegrees = 50f };

        fsm.AddState("Idle", new IdleState { Vision = vision, NextState = "RangedAttack" });
        fsm.AddState("RangedAttack", new RangedAttackState { Vision = vision, FireInterval = 2f, ProjectileSpeed = 350f, Damage = 1 });
        fsm.AddState("Stagger", new StaggerState { Duration = 0.3f, RecoverToState = "Idle" });

        health.OnDamaged += (_, _) => { if (!health.IsDead) fsm.ChangeState("Stagger"); };

        fsm.ChangeState("Idle", force: true);
    }

    private void BuildShieldEnemy(Vector2 position, float leftBound, float rightBound)
    {
        var (body, health, hurtbox, fsm, _) = BuildEnemyBase(position, new Vector2(36, 48), maxHealth: 5, Color.SlateGray);

        var spearHitbox = new Hitbox { Owner = body, Damage = 1, Size = new Vector2(46, 16), Tag = "EnemyMelee" };
        _scene.AddHitbox(spearHitbox);

        var vision = new VisionCone { Range = 200f, HalfAngleDegrees = 60f };

        fsm.AddState("Idle", new IdleState { Vision = vision, NextState = "Patrol" });
        fsm.AddState("Patrol", new PatrolState { LeftBound = leftBound, RightBound = rightBound, Speed = 30f, Vision = vision });
        fsm.AddState("Chase", new ChaseState { Speed = 50f, AttackRange = 48f });
        fsm.AddState("Attack", new AttackState { Hitbox = spearHitbox, AttackRange = 48f, AttackDuration = 0.6f, ActiveWindowStart = 0.2f, ActiveWindowEnd = 0.4f });
        fsm.AddState("Stagger", new StaggerState { Duration = 0.4f, RecoverToState = "Chase" });

        health.OnDamaged += (_, _) => { if (!health.IsDead) fsm.ChangeState("Stagger"); };

        body.AddScript(new ShieldBlockScript { Fsm = fsm, Hurtbox = hurtbox, Player = _player });

        fsm.ChangeState("Idle", force: true);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        Input.Update(); // MUST be first

        _scene.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _renderer.Begin();

        _scene.Draw(_renderer);

        _renderer.End();

        base.Draw(gameTime);
    }
}
