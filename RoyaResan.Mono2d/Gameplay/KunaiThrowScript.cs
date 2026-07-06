using RoyaResan.Mono2d.Animation;
using RoyaResan.Mono2d.Combat;
using RoyaResan.Mono2d.Core;
using RoyaResan.Mono2d.Scripting;

namespace RoyaResan.Mono2d.Gameplay;

/// <summary>
/// Player kunai: tracks ammo (design doc: start 10, pickups +5, cap 20),
/// and on ThrowKey spawns a straight-line projectile using ProjectileScript
/// - the same primitive a ranged enemy would use for its shot.
///
/// Wires its own placeholder visual (a small dark PlaceholderRectNode)
/// onto each spawned kunai, so no spritesheet is needed.
///
/// Usage:
///   var kunai = new KunaiThrowScript { Scene = scene, Movement = platformerScript, Animator = animator };
///   kunai.OnAmmoChanged += count => hudKunaiLabel.Text = $"Kunai x{count}";
///   player.AddScript(kunai);
/// </summary>
public class KunaiThrowScript : Script
{
    public Scene Scene;
    public PlatformerMovementScript Movement;
    public Animator Animator;


    public int Damage = 3;
    public int Ammo = 50;
    public int MaxAmmo = 100;

    public float ThrowSpeed = 700f;
    public float Cooldown = 0.75f;

    /// <summary>Knockback applied to whatever a thrown kunai hits - see Hitbox.Knockback. Light by default; a stronger throw could set this per-item if you add throwable variety later.</summary>
    public Vector2 Knockback = new Vector2(10f, 0f);

    /// <summary>Fires whenever Ammo changes - wire a HUD label to this instead of polling.</summary>
    public event Action<int> OnAmmoChanged;

    /// <summary>True for ThrowPoseDuration after a throw - lets an Animator transition rule yield to the Throw state briefly, same pattern as SwordAttackScript.IsAttacking.</summary>
    public bool IsThrowing { get; private set; }

    public float ThrowPoseDuration = 0.2f;

    private float _cooldownTimer;
    private float _throwPoseTimer;

    public void AddAmmo(int amount)
    {
        Ammo = Math.Min(MaxAmmo, Ammo + amount);
        OnAmmoChanged?.Invoke(Ammo);
    }

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_cooldownTimer > 0f)
            _cooldownTimer -= dt;

        if (_throwPoseTimer > 0f)
        {
            _throwPoseTimer -= dt;
            if (_throwPoseTimer <= 0f)
                IsThrowing = false;
        }

        if (_cooldownTimer <= 0f && Ammo > 0 && InputManager.IsActionPressed(InputManager.Throw))
        {
            Throw();
            _cooldownTimer = Cooldown;
            _throwPoseTimer = ThrowPoseDuration;
            IsThrowing = true;
        }
    }

    private void Throw()
    {
        bool facingRight = Movement?.FacingRight ?? true;

        var kunaiBody = new PhysicsBody { UseGravity = false };
        kunaiBody.Position = Owner.Position + new Vector2(facingRight ? 35f : -35f, -8f);

        kunaiBody.Collider = new Collider { Owner = kunaiBody, Size = new Vector2(10f, 6f) };

        var visual = new PlaceholderRectNode { Size = new Vector2(10f, 6f), Color = Color.DarkSlateGray };
        kunaiBody.AddChild(visual);

        var hitbox = new Hitbox
        {
            Owner = kunaiBody,
            Damage = 3,
            Size = new Vector2(10f, 6f),
            Tag = "Kunai",
            Knockback = Knockback
        };

        // Remember the real owner (player) so damage events know who attacked
        kunaiBody.UserData = Owner;

        Scene.AddBody(kunaiBody);
        Scene.AddHitbox(hitbox);

        kunaiBody.Velocity = new Vector2(facingRight ? ThrowSpeed : -ThrowSpeed, 0f);
        kunaiBody.AddScript(new ProjectileScript
        {
            Scene = Scene,
            Hitbox = hitbox,
            Lifetime = 2.5f,           // Longer lifetime
            StoppedSpeedThreshold = 5f
        });

        Ammo--;
        OnAmmoChanged?.Invoke(Ammo);
        Animator?.Play("Throw", 0.05f);
    }
}