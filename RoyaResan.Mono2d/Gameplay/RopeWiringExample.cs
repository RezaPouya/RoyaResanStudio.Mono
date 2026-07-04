using RoyaResan.Mono2d.Core;
using RoyaResan.Mono2d.Nodes;
using RoyaResan.Mono2d.Physics;

namespace RoyaResan.Mono2d.Gameplay;

/// <summary>
/// USAGE EXAMPLES - not wired into World.cs automatically. Both enable
/// gravity on the player (off by default elsewhere in the project).
/// </summary>
public static class RopeWiringExample
{
    /// <summary>Shadow Dancer style - Space grapples the nearest pre-placed anchor point.</summary>
    public static void SetupNearestAnchor(Scene scene, PhysicsBody player)
    {
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

    /// <summary>
    /// Bionic Commando style - Space raycasts toward the mouse and
    /// attaches wherever it hits static geometry (walls/ceilings),
    /// at the real hit distance - no pre-placed anchors needed.
    /// </summary>
    public static void SetupAimAndFire(Scene scene, PhysicsBody player, Camera2D camera)
    {
        player.UseGravity = true;

        var rope = new Rope { Body = player };
        scene.AddRope(rope);

        player.AddScript(new RopeController
        {
            Rope = rope,
            Camera = camera,
            World = scene.Physics,
            Range = 300f,
            ReelSpeed = 80f
        });
    }
}
