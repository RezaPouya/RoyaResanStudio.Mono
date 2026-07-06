namespace RoyaResan.Mono2d.Nodes;

/// <summary>
/// A static platform whose solidity is a per-instance toggle instead of a
/// choice between two separate classes. Defaults to fully solid (blocks
/// from every side, like the old WallNode) - set OneWay = true to get the
/// old OneWayPlatformNode behavior (jump up through it from below, land
/// on top, walk through the sides) instead.
///
/// WallNode and OneWayPlatformNode still exist and behave exactly as
/// before (they're now thin wrappers around this), so nothing existing
/// breaks - but for new level geometry, prefer this one so "is this
/// platform solid or one-way" is a value you set per-instance rather than
/// a class you have to remember to pick correctly. This also applies to
/// anything that collides with static geometry generically (enemies,
/// thrown rocks, projectiles), so toggling OneWay here is enough to fix
/// solidity everywhere at once - no separate enemy-side flag needed.
/// </summary>
public class PlatformNode : PhysicsBody
{
    private bool _oneWay;

    public bool OneWay
    {
        get => _oneWay;
        set
        {
            _oneWay = value;
            if (Collider != null)
                Collider.IsOneWay = value;
        }
    }

    public PlatformNode(Vector2? size = null, bool oneWay = false)
    {
        IsStatic = true;

        Collider = new Collider
        {
            Owner = this,
            Size = size ?? new Vector2(64, 16),
            IsStatic = true,
            IsOneWay = oneWay
        };

        _oneWay = oneWay;
    }
}
