using RoyaResan.Mono2d.Animation;
using RoyaResan.Mono2d.Combat;
using RoyaResan.Mono2d.Core;
using RoyaResan.Mono2d.Scripting;

namespace RoyaResan.Mono2d.Gameplay;

public class KunaiThrowScript : Script
{
    public Scene Scene;
    public PlatformerMovementScript Movement;
    public Animator Animator;

    public int Damage = 1;
    public int Ammo = 100;
    public int MaxAmmo = 200;

    public float ThrowSpeed = 500f;
    public float Cooldown = 0.35f; // Slightly faster throw rate

    public Vector2 Knockback = new Vector2(60f, 0f);

    public event Action<int> OnAmmoChanged;

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
        kunaiBody.Position = Owner.Position + new Vector2(facingRight ? 45f : -45f, -15f); // Bigger offset + upward bias

        kunaiBody.Collider = new Collider { Owner = kunaiBody, Size = new Vector2(12f, 8f) };

        var visual = new PlaceholderRectNode { Size = new Vector2(12f, 8f), Color = Color.DarkSlateGray };
        kunaiBody.AddChild(visual);

        var hitbox = new Hitbox
        {
            Owner = kunaiBody,
            Damage = 3,
            Size = new Vector2(12f, 8f),
            Tag = "Kunai",
            Knockback = Knockback
        };

        kunaiBody.UserData = Owner;

        Scene.AddBody(kunaiBody);
        Scene.AddHitbox(hitbox);

        kunaiBody.Velocity = new Vector2(facingRight ? ThrowSpeed : -ThrowSpeed, 0f);

        kunaiBody.AddScript(new ProjectileScript
        {
            Scene = Scene,
            Hitbox = hitbox,
            Lifetime = 3.5f,
            StoppedSpeedThreshold = 1f
        });

        Ammo--;
        OnAmmoChanged?.Invoke(Ammo);
        Animator?.Play("Throw", 0.05f);
    }
}