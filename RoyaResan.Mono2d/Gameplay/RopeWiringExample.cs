using RoyaResan.Mono2d.Core;
using RoyaResan.Mono2d.Nodes;
using RoyaResan.Mono2d.Physics;

namespace RoyaResan.Mono2d.Gameplay;

/// <summary>
/// USAGE EXAMPLE - not wired into World.cs automatically. Enables gravity
/// on the player (off by default elsewhere in the project), places two
/// RopeAnchorNodes, and attaches a RopeController so Space grapples the
/// nearest one and W/S reel in/out.
/// </summary>
public static class RopeWiringExample
{
    public static void Setup(Scene scene, PhysicsBody player)
    {
        // Rope-swing gameplay needs gravity - it's off by default so it
        // doesn't silently change existing top-down movement.
        player.UseGravity = true;

        var anchor1 = new RopeAnchorNode { Position = new Vector2(400, 50) };
        var anchor2 = new RopeAnchorNode { Position = new Vector2(700, 80) };
        scene.Root.AddChild(anchor1);
        scene.Root.AddChild(anchor2);

        var rope = new Rope { Body = player };
        scene.AddRope(rope);

        player.AddScript(new RopeController
        {
            Rope = rope,
            AvailableAnchors = new List<RopeAnchorNode> { anchor1, anchor2 },
            Range = 220f,
            ReelSpeed = 80f
        });
    }
}
