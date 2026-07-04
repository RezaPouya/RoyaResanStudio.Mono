using RoyaResan.Mono2d.Graphics;
using RoyaResan.Mono2d.Inputs;
using RoyaResan.Mono2d.Nodes;
using RoyaResan.Mono2d.Physics;
using RoyaResan.Mono2d.Scripting;

namespace RoyaResan.Mono2d.Gameplay;

/// <summary>
/// Player-facing grapple control. Two modes, chosen automatically:
///
/// - Aim-and-fire (Bionic Commando style): if Camera and World are both
///   set, Space raycasts from the player toward the mouse position and
///   attaches wherever it hits static geometry, at the real hit distance.
/// - Nearest-anchor (Shadow Dancer style): if Camera/World aren't set,
///   falls back to grabbing the nearest pre-placed RopeAnchorNode - still
///   useful for designer-authored swing points.
///
/// Up/Down reel the rope in/out either way.
/// </summary>
public class RopeController : Script
{
    public Rope Rope;
    public float Range = 200f;
    public float ReelSpeed = 80f;

    // Aim-and-fire mode
    public Camera2D Camera;
    public PhysicsWorld World;

    // Nearest-anchor fallback mode
    public List<RopeAnchorNode> AvailableAnchors = new();

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (Input.IsKeyPressed(Keys.Space))
        {
            if (Rope.Attached)
                Rope.Detach();
            else if (Camera != null && World != null)
                TryFireAtMouse();
            else
                TryAttachNearest();
        }

        if (Rope.Attached)
        {
            if (Input.IsKeyDown(Keys.W))
                Rope.Reel(ReelSpeed * dt);
            if (Input.IsKeyDown(Keys.S))
                Rope.Reel(-ReelSpeed * dt);
        }
    }

    /// <summary>Raycasts toward the mouse in world space, hitting only static geometry, and attaches at the real hit point/distance.</summary>
    public bool TryFireAtMouse()
    {
        Vector2 worldTarget = Camera.ScreenToWorld(Input.MousePosition);
        Vector2 direction = worldTarget - Owner.GlobalPosition;

        if (Raycast.Cast(World, Owner.GlobalPosition, direction, Range, out var hit, b => b.IsStatic, Owner))
        {
            Rope.Attach(hit.Point, hit.Distance);
            return true;
        }

        return false;
    }

    private void TryAttachNearest()
    {
        RopeAnchorNode nearest = null;
        float nearestDist = Range;

        foreach (var anchor in AvailableAnchors)
        {
            float dist = Vector2.Distance(Owner.GlobalPosition, anchor.GlobalPosition);
            if (dist <= nearestDist)
            {
                nearest = anchor;
                nearestDist = dist;
            }
        }

        if (nearest != null)
            Rope.Attach(nearest.GlobalPosition);
    }
}
