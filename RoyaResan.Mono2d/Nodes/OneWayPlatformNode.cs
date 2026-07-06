namespace RoyaResan.Mono2d.Nodes;

/// <summary>
/// Thin compatibility wrapper - equivalent to `new PlatformNode(oneWay: true)`.
/// A platform you can jump up through and land on top of, but never
/// collide with from below or the sides. See PlatformNode for the
/// adjustable version (set .OneWay per-instance instead of picking a
/// class).
/// </summary>
public class OneWayPlatformNode : PlatformNode
{
    public OneWayPlatformNode() : base(new Vector2(64, 16), oneWay: true)
    {
    }
}
