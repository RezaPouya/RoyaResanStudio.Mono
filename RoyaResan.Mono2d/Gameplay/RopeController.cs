using RoyaResan.Mono2d.Inputs;
using RoyaResan.Mono2d.Nodes;
using RoyaResan.Mono2d.Physics;
using RoyaResan.Mono2d.Scripting;

namespace RoyaResan.Mono2d.Gameplay;

/// <summary>
/// Player-facing grapple control. Space attaches to the nearest
/// RopeAnchorNode within Range (or detaches if already attached), Up/Down
/// reel the rope in/out while attached.
///
/// NOTE: this finds the nearest pre-placed RopeAnchorNode, it does not
/// raycast toward wherever the player is aiming - the framework has no
/// raycasting yet (planned next). For designer-placed swing points
/// (the Shadow Dancer era didn't have physically simulated arbitrary
/// grapple targets either), this is enough to build real rope-swing
/// gameplay with today.
/// </summary>
public class RopeController : Script
{
    public Rope Rope;
    public List<RopeAnchorNode> AvailableAnchors = new();
    public float Range = 200f;
    public float ReelSpeed = 80f;

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (Input.IsKeyPressed(Keys.Space))
        {
            if (Rope.Attached)
                Rope.Detach();
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
