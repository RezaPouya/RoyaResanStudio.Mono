using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RoyaResan.Mono2d.AI;
using RoyaResan.Mono2d.AI.States;
using RoyaResan.Mono2d.Animation;
using RoyaResan.Mono2d.Combat;
using RoyaResan.Mono2d.Core;
using RoyaResan.Mono2d.Gameplay;
using RoyaResan.Mono2d.Graphics;
using RoyaResan.Mono2d.Inputs;
using RoyaResan.Mono2d.Nodes;
using RoyaResan.Mono2d.Physics;
using RoyaResan.Mono2d.UI;
using System;

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

    /// <summary>World-space Y below which anything (player or enemy) is considered fallen into a pit and dies - see FallDeathScript.</summary>
    private const float FallDeathY = 900f;

    private UiElement _hud;
    private UiLabel _healthLabel;
    private UiLabel _kunaiLabel;

    public World()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;

    }

    protected override void LoadContent()
    {
        var spriteBatch = new SpriteBatch(GraphicsDevice);
        _renderer = new Renderer(spriteBatch);

        VirtualControls.Initialize(GraphicsDevice);
        VirtualControls.ShowVirtualControls = false; // true for mobile / testing

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
        BuildHud();

        _scene.Camera.FollowTarget = _player;
        _scene.Camera.FollowSmoothing = 8f;

        InputManager.Initialize(); // Important!
        InputManager.RemapKeyboard("Left", Keys.Left);   // Your change
        InputManager.RemapKeyboard("Right", Keys.Right);
        InputManager.RemapKeyboard("Attack", Keys.A);
        InputManager.RemapKeyboard("Throw", Keys.Q);

        // 50ms freeze on a landed sword hit - classic hit-stop, makes
        // impact register even with placeholder rectangles. Filtered by
        // Tag so kunai/enemy hits don't also trigger it.
        _scene.Combat.OnHit += (hitbox, hurtbox) =>
        {
            if (hitbox.Tag == "Sword")
                _scene.TriggerHitStop(0.09f); // was 0.05f - 3 frames @60fps read as nothing; ~5-6 frames is the usual floor for a felt hit-stop
        };
    }

    // -----------------------------------------------------------------
    // TEMP - a bare floor + side walls so movement/jump/gravity can be
    // playtested before a real Tiled level exists. Delete this method
    // (and its call above) once Level 1 loads from Tiled instead.
    // -----------------------------------------------------------------
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

        // PIT - deliberately no floor tiles from here (x=1280) to the start
        // of the extended floor below (x=1472). The horizontal moving
        // platform bridges it; missing the jump means falling to
        // FallDeathY. Existing enemy patrol bounds all sit well before
        // this, so nothing patrols over open air.
        for (int i = 0; i < 4; i++)
        {
            var floorTile = new WallNode();
            floorTile.Position = new Vector2(1472 + i * 64, 500);
            _scene.AddBody(floorTile);
            var visual = new PlaceholderRectNode { Size = new Vector2(64, 64), Color = Color.DimGray };
            floorTile.AddChild(visual);
        }

        // NOTE: the old left/right WallNode stacks that used to cap this
        // level are gone. They were sealing the player in (and were
        // already inconsistent with enemy #3's patrol bounds reaching
        // past the old right wall at x=1248). Now that FallDeathY exists,
        // walking off either end is handled the same way as the pit -
        // just a fall - so there's no need for a hard wall to prevent it.

        // Step platform (unchanged)
        var stepPlatform = new OneWayPlatformNode();
        stepPlatform.OneWay = false;
        stepPlatform.Position = new Vector2(400, 400);
        _scene.AddBody(stepPlatform);
        stepPlatform.AddChild(new PlaceholderRectNode { Size = new Vector2(64, 16), Color = Color.SaddleBrown });

        BuildMovingPlatforms();
    }

    // -----------------------------------------------------------------
    // MOVING PLATFORMS - one horizontal (bridges the pit above), one
    // vertical (a free-standing elevator near spawn). Riders standing on
    // either get carried automatically, and either will crush a
    // character it moves into sideways against something solid - see
    // PhysicsWorld.PushSideways.
    // -----------------------------------------------------------------
    private void BuildMovingPlatforms()
    {
        var horizontalPlatform = new MovingPlatformNode
        {
            Position = new Vector2(1328, 470),
            PointA = new Vector2(1328, 470),
            PointB = new Vector2(1424, 470),
            Speed = 70f
        };
        horizontalPlatform.AddChild(new PlaceholderRectNode { Size = new Vector2(96, 16), Color = Color.Goldenrod });
        _scene.AddBody(horizontalPlatform);

        var verticalPlatform = new MovingPlatformNode
        {
            // Old PointA (150,460) sat flush with the main floor's top
            // (468) - basically at ground level, so there was nothing to
            // actually jump up onto. The floor top is 468; JumpVelocity
            // 420 against Gravity 900 gives a max jump height of roughly
            // 420*420/(2*900) ≈ 98px, so a low point at 400 (68px up) is
            // an easy direct jump, while the high point at 150 (318px up)
            // is only reachable by riding the platform up.
            Position = new Vector2(980, 400),
            PointA = new Vector2(980, 400),
            PointB = new Vector2(980, 150),
            Speed = 60f
        };
        verticalPlatform.AddChild(new PlaceholderRectNode { Size = new Vector2(96, 16), Color = Color.CadetBlue });
        _scene.AddBody(verticalPlatform);
    }

    // -----------------------------------------------------------------
    // PLAYER
    // -----------------------------------------------------------------
    private void BuildPlayer()
    {
        _player = new PhysicsBody { Team = "Player" };
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
        var swordHitbox = new Hitbox
        {
            Owner = _player,
            Damage = 2,
            Tag = "Sword",
            Knockback = new Vector2(70f, 0f), // was 180f - enough to sell the hit without launching the enemy out of attack range
            SelfKnockback = new Vector2(20f, 0f) // small recoil - mostly for feel, not balance
        };
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

        // Falling into a pit is lethal, same as running out of Health -
        // routes through the same OnDeath -> animatorDriver.IsDead flow
        // rather than inventing a second death path.
        var fallDeath = new FallDeathScript { DeathY = FallDeathY };
        fallDeath.OnFallDeath = () => _playerHealth.Damage(_playerHealth.Max);
        _player.AddScript(fallDeath);

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
    // HUD - kept as a direct reference and Update/Draw'd manually
    // alongside Scene.Ui, NOT pushed through Scene.Ui.Push(). Per
    // UiManager's own doc-comment: only the top of that stack gets
    // input/draw, so a HUD that needs to stay visible under a pause
    // menu can't live there - it has to be driven separately.
    // -----------------------------------------------------------------
    private void BuildHud()
    {
        var font = Content.Load<SpriteFont>("DefaultFont");

        _hud = new UiElement { Position = new Vector2(16, 16) };

        _healthLabel = new UiLabel { Font = font, Color = Color.Red };
        _hud.AddChild(_healthLabel);

        _kunaiLabel = new UiLabel { Font = font, Position = new Vector2(0, 26), Color = Color.White };
        _hud.AddChild(_kunaiLabel);

        RefreshHud();
    }

    /// <summary>Recomputes HUD text from current game state - called every frame rather than wired to events, since it's cheap and this way it can never drift out of sync.</summary>
    private void RefreshHud()
    {
        _healthLabel.Text = new string('\u2665', Math.Max(0, _playerHealth.Current));
        _kunaiLabel.Text = $"Kunai x{_playerKunai.Ammo}";
    }

    // -----------------------------------------------------------------
    // ENEMIES
    // -----------------------------------------------------------------
    private void BuildEnemies()
    {
        _enemyGroup = new CombatGroup { Target = _player, MaxSimultaneousAttackers = 1 };
        //BuildMeleeEnemy(new Vector2(500, 444), leftBound: 450, rightBound: 650);
        //BuildMeleeEnemy(new Vector2(900, 444), leftBound: 850, rightBound: 1050);
        //BuildMeleeEnemy(new Vector2(1300, 444), leftBound: 1250, rightBound: 1450);

        //BuildRangedEnemy(new Vector2(1100, 444));

        BuildShieldEnemy(new Vector2(700, 444), leftBound: 650, rightBound: 780);
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
        var body = new PhysicsBody { Position = position, UseGravity = true, Team = "Enemy" };
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

        var fsm = new EnemyFsm { Animator = animator, Group = _enemyGroup, World = _scene.Physics, Scene = _scene, HomePosition = position };
        _enemyGroup.Join(fsm);

        fsm.AddState("Dead", new DeadState { Hurtbox = hurtbox });
        health.OnDeath += () => fsm.ChangeState("Dead", force: true);

        body.AddScript(new EnemyFsmScript { Fsm = fsm });

        // Falling into a pit is lethal, same as losing all Health - routes
        // through the same OnDeath -> "Dead" state -> despawn flow already
        // wired above, rather than a separate fall-specific death path.
        var fallDeath = new FallDeathScript { DeathY = FallDeathY };
        fallDeath.OnFallDeath = () => health.Damage(health.Max);
        body.AddScript(fallDeath);

        // World-space health bar above enemy (now after health is declared)
        var healthBarBg = new PlaceholderRectNode { Size = new Vector2(32, 4), Color = Color.Black, Position = new Vector2(0, -size.Y / 2 - 10) };
        var healthBarFill = new PlaceholderRectNode { Size = new Vector2(30, 2), Color = Color.Lime, Position = new Vector2(0, -size.Y / 2 - 10) };
        body.AddChild(healthBarBg);
        body.AddChild(healthBarFill);

        // Script to update health bar
        body.AddScript(new EnemyHealthBarScript { Health = health, Fill = healthBarFill, Bg = healthBarBg });

        _scene.AddBody(body);

        return (body, health, hurtbox, fsm, animator);
    }

    private void BuildMeleeEnemy(Vector2 position, float leftBound, float rightBound)
    {

        var (body, health, hurtbox, fsm, _) = BuildEnemyBase(position, new Vector2(28, 48), maxHealth: 10, Color.IndianRed);

        var meleeHitbox = new Hitbox
        {
            Owner = body,
            Damage = 1,
            Size = new Vector2(24, 20),
            Tag = "EnemyMelee",
            Knockback = new Vector2(120f, 0f)
        };
        _scene.AddHitbox(meleeHitbox);

        var vision = new VisionCone { Range = 220f, HalfAngleDegrees = 60f };

        fsm.AddState("Idle", new IdleState { Vision = vision, NextState = "Patrol" });
        fsm.AddState("Patrol", new PatrolState { LeftBound = leftBound, RightBound = rightBound, Speed = 60f, Vision = vision });
        fsm.AddState("Chase", new ChaseState { Speed = 140f, AttackRange = 30f, CanThrowAtRange = true });
        fsm.AddState("Attack", new AttackState { Hitbox = meleeHitbox, AttackRange = 30f, AttackDuration = 0.4f, ActiveWindowStart = 0.12f, ActiveWindowEnd = 0.25f });
        fsm.AddState("Stagger", new StaggerState { Duration = 0.3f, RecoverToState = "Chase" });
        fsm.AddState("Return", new ReturnState());
        fsm.AddState("RockThrow", new RockThrowState());

        health.OnDamaged += (amount, source) =>
        {
            if (!health.IsDead)
            {
                if (source != null && source.Team == "Player")
                    _enemyGroup.RaiseAlert(source);
                fsm.ChangeState("Stagger");
            }
        };

        fsm.ChangeState("Idle", force: true);
    }

    private void BuildRangedEnemy(Vector2 position)
    {
        var (body, health, hurtbox, fsm, _) = BuildEnemyBase(position, new Vector2(24, 40), maxHealth: 10, Color.MediumPurple);

        var vision = new VisionCone { Range = 320f, HalfAngleDegrees = 180f }; // stationary - never patrols to flip facing, so it needs to notice the player from either side

        fsm.AddState("Idle", new IdleState { Vision = vision, NextState = "RangedAttack" });
        fsm.AddState("RangedAttack", new RangedAttackState { Vision = vision, FireInterval = 2f, ProjectileSpeed = 350f, Damage = 1 });
        fsm.AddState("Stagger", new StaggerState { Duration = 0.3f, RecoverToState = "Idle" });

        health.OnDamaged += (amount, source) =>
        {
            if (!health.IsDead)
            {
                if (source != null && source.Team == "Player")
                    _enemyGroup.RaiseAlert(source);
                fsm.ChangeState("Stagger");
            }
        };

        fsm.ChangeState("Idle", force: true);
    }

    private void BuildShieldEnemy(Vector2 position, float leftBound, float rightBound)
    {
        // Build base enemy (body, main hurtbox, FSM, etc.)
        var (body, health, hurtbox, fsm, _) = BuildEnemyBase(position, new Vector2(36, 48), maxHealth: 100, Color.SlateGray);

        // ---- Weapon hitbox (spear) ----
        var spearHitbox = new Hitbox
        {
            Owner = body,
            Damage = 1,
            Size = new Vector2(46, 16),
            Tag = "EnemyMelee",
            Knockback = new Vector2(160f, 0f)
        };
        _scene.AddHitbox(spearHitbox);

        // ---- Shield: purely cosmetic facing indicator now, no physics/collider/hurtbox of its own ----
        // Just a visual child node so you can see which way the enemy is facing.
        // Kunai blocking is handled entirely by ShieldBlockScript toggling the
        // MAIN body's Hurtbox.BlockedTags below - no separate physical shield.
        var shieldVisual = new PlaceholderRectNode
        {
            Size = new Vector2(12, 28),
            Color = Color.Orange,
            Position = new Vector2(12, -2) // local offset relative to enemy centre
        };
        body.AddChild(shieldVisual);

        // ---- Enemy AI states ----
        var vision = new VisionCone { Range = 500f, HalfAngleDegrees = 60f };

        fsm.AddState("Idle", new IdleState { Vision = vision, NextState = "Patrol" });
        fsm.AddState("Patrol", new PatrolState { LeftBound = leftBound, RightBound = rightBound, Speed = 30f, Vision = vision });
        fsm.AddState("Chase", new ChaseState { Speed = 50f, AttackRange = 50f, CanThrowAtRange = true });
        fsm.AddState("Attack", new AttackState { Hitbox = spearHitbox, AttackRange = 50f, AttackDuration = 0.6f, ActiveWindowStart = 0.2f, ActiveWindowEnd = 0.4f });
        fsm.AddState("Stagger", new StaggerState { Duration = 0.4f, RecoverToState = "Chase" });
        fsm.AddState("Return", new ReturnState());
        fsm.AddState("RockThrow", new RockThrowState());

        // ---- Damage reaction ----
        health.OnDamaged += (amount, source) =>
        {
            if (!health.IsDead)
            {
                if (source != null && source.Team == "Player")
                    _enemyGroup.RaiseAlert(source);
                fsm.ChangeState("Stagger");
            }
        };

        // ---- ShieldBlockScript – attaches to the enemy's MAIN body, toggles the MAIN hurtbox's BlockedTags ----
        body.AddScript(new ShieldBlockScript
        {
            Fsm = fsm,
            Player = _player,
            Hurtbox = hurtbox,
            BlockRange = 600f, // effectively "as far as a kunai can reach" - facing is what actually gates the block
            ShieldVisual = shieldVisual,
            ShieldVisualOffsetX = 12f
        });

        fsm.ChangeState("Idle", force: true);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        InputManager.Update();
        VirtualControls.Update();

        _scene.Update(gameTime);

        RefreshHud();
        _hud.Update(gameTime, Vector2.Zero);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _renderer.Begin();

        _scene.Draw(_renderer);

        VirtualControls.Draw(_renderer);

        _hud.Draw(_renderer);

        _renderer.End();

        base.Draw(gameTime);
    }
}