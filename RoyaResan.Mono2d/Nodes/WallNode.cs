namespace RoyaResan.Mono2d.Nodes;

/// <summary>
/// Thin compatibility wrapper - equivalent to `new PlatformNode(oneWay: false)`.
/// See PlatformNode for the adjustable version (set .OneWay per-instance
/// instead of picking a class).
/// </summary>
public class WallNode : PlatformNode
{
    public WallNode() : base(new Vector2(64, 64), oneWay: false)
    {
    }
}
