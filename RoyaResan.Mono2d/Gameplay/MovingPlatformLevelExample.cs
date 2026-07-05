using RoyaResan.Mono2d.AI;
using RoyaResan.Mono2d.Combat;
using RoyaResan.Mono2d.Core;

namespace RoyaResan.Mono2d.Gameplay;

/// <summary>
/// USAGE EXAMPLE - not wired into World.cs automatically. Builds a small
/// platformer test level:
///
///   - Ground + two side walls (WallNode).
///   - A horizontal moving platform and a vertical moving platform
///     (MovingPlatformNode) - riders get carried, and either platform
///     will crush a character it moves into from the side against
///     something solid (see PhysicsWorld.PushSideways).
///   - A player with PlatformerMovementScript, gravity on, and
///     FallDeathScript so falling into the pit between the walls kills it.
///   - Two patrol enemies (EnemyWiringExample) that now avoid walking off
///     ledges (PatrolState.AvoidEdges) and also die if they fall, since
///     they share the same FallDeathScript wiring as the player.
///
/// Ground layout assumed (world units, Y grows downward):
///
///   walls at X=0 and X=900, floor at Y=500, pit is the ~300px gap of
///   floor deliberately left out in the middle - that's what the
///   horizontal platform bridges, and what FallDeathScript punishes if
///   you miss the jump. DeathY = 900 sits comfortably below the floor.
/// </summary>
public static class MovingPlatformLevelExample
{
    public static PhysicsBody BuildLevel(Scene scene, Texture2D enemySheet, Action onPlayerDied = null)
    {
        BuildGroundAndWalls(scene);

        var horizontalPlatform = new MovingPlatformNode
        {
            Position = new Vector2(300, 420),
            PointA = new Vector2(300, 420),
            PointB = new Vector2(600, 420),
            Speed = 80f
        };
        scene.AddBody(horizontalPlatform);

        var verticalPlatform = new MovingPlatformNode
        {
            Position = new Vector2(750, 250),
            PointA = new Vector2(750, 150),
            PointB = new Vector2(750, 460),
            Speed = 60f
        };
        scene.AddBody(verticalPlatform);

        // --- Player ---
        var player = new PhysicsBody { Position = new Vector2(60, 400) };
        player.Collider = new Collider { Owner = player, Size = new Vector2(32, 32) };
        player.AddScript(new PlatformerMovementScript());

        var playerFallDeath = new FallDeathScript { DeathY = 900f };
        Vector2 playerSpawn = player.Position;
        playerFallDeath.OnFallDeath = () =>
        {
            // Simplest possible "death": snap back to spawn. Swap this for
            // a real death animation/game-over flow as needed - the point
            // is OnFallDeath fires exactly once per fall, so this is a
            // safe place to do it.
            player.Position = playerSpawn;
            player.Velocity = Vector2.Zero;
            playerFallDeath.Reset();
            onPlayerDied?.Invoke();
        };
        player.AddScript(playerFallDeath);

        scene.AddBody(player);
        scene.Camera.FollowTarget = player;

        // --- Enemies ---
        // Left one patrols the ground next to the pit - AvoidEdges (on by
        // default now) stops it from marching straight off into the gap.
        // Right one patrols on top of the vertical platform's travel path,
        // so you can see the carry + crush behavior play out against it.
        var group = new CombatGroup { MaxSimultaneousAttackers = 1, Target = player };

        var enemyA = EnemyWiringExample.BuildEnemy(scene, group, enemySheet, spawnX: 150, leftBound: 40, rightBound: 380);
        var enemyB = EnemyWiringExample.BuildEnemy(scene, group, enemySheet, spawnX: 800, leftBound: 700, rightBound: 880);

        // Enemies fall to their death too, same as the player - just
        // despawn them instead of respawning.
        AddFallDeath(enemyA, scene, deathY: 900f);
        AddFallDeath(enemyB, scene, deathY: 900f);

        return player;
    }

    private static void AddFallDeath(PhysicsBody enemy, Scene scene, float deathY)
    {
        var fallDeath = new FallDeathScript { DeathY = deathY };
        fallDeath.OnFallDeath = () => scene.RemoveBody(enemy);
        enemy.AddScript(fallDeath);
    }

    private static void BuildGroundAndWalls(Scene scene)
    {
        // Two floor segments with a gap in between (the pit the moving
        // platform bridges), each made of adjacent WallNode tiles just
        // like LevelLoader would generate from a Tiled map.
        for (int x = 0; x < 250; x += 64)
            scene.AddBody(new WallNode { Position = new Vector2(x, 532) });

        for (int x = 650; x < 900; x += 64)
            scene.AddBody(new WallNode { Position = new Vector2(x, 532) });

        // Side walls so patrol enemies (and the player) can't wander off
        // the edges of the level itself.
        scene.AddBody(new WallNode { Position = new Vector2(-32, 300) });
        scene.AddBody(new WallNode { Position = new Vector2(932, 300) });
    }
}
