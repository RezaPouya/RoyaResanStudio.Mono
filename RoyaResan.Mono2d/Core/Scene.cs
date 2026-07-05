using RoyaResan.Mono2d.Combat;
using RoyaResan.Mono2d.UI;

namespace RoyaResan.Mono2d.Core;

public class Scene
{
    public SceneNode Root = new SceneNode();
    public Camera2D Camera = new Camera2D();
    public PhysicsWorld Physics = new PhysicsWorld();
    public CombatWorld Combat = new CombatWorld();

    /// <summary>Screen-space UI overlay (pause menu, HUD, dialogs). Always updated/drawn, paused or not - see IsPaused.</summary>
    public UiManager Ui = new UiManager();

    /// <summary>
    /// Adds a body to the node tree AND registers it with the physics
    /// world. Use this instead of Root.AddChild for any PhysicsBody,
    /// or it will never collide with anything.
    /// </summary>
    public void AddBody(PhysicsBody body, Node parent = null)
    {
        (parent ?? (Node)Root).AddChild(body);
        Physics.Bodies.Add(body);
    }

    public void AddHitbox(Hitbox hitbox) => Combat.Hitboxes.Add(hitbox);
    public void AddHurtbox(Hurtbox hurtbox) => Combat.Hurtboxes.Add(hurtbox);
    public void AddRope(Rope rope) => Physics.Ropes.Add(rope);

    /// <summary>
    /// Despawns a body: removes it from the tree (Parent.RemoveChild)
    /// and from PhysicsWorld.Bodies in one call, so it stops colliding
    /// AND stops being drawn/updated. Use this for anything that needs
    /// to disappear at runtime - kunai hitting a wall, a dead enemy, a
    /// collected pickup. Safe to call from inside a Script's Update()
    /// (removal is immediate, not deferred - don't call this on a body
    /// while iterating something that also touches it this same frame
    /// without expecting it to vanish right then).
    /// </summary>
    public void RemoveBody(PhysicsBody body)
    {
        body.Parent?.RemoveChild(body);
        Physics.Bodies.Remove(body);
    }

    public void RemoveHitbox(Hitbox hitbox) => Combat.Hitboxes.Remove(hitbox);
    public void RemoveHurtbox(Hurtbox hurtbox) => Combat.Hurtboxes.Remove(hurtbox);
    public void RemoveRope(Rope rope) => Physics.Ropes.Remove(rope);

    /// <summary>
    /// Hard-pauses gameplay: while true, Update() no-ops entirely - no node
    /// updates (so no scripts, no AI, no animation), no physics step, no
    /// combat step, no camera update (so screen shake also freezes rather
    /// than continuing under a paused game). Draw() is unaffected and
    /// always runs, so the last simulated frame stays visible underneath
    /// a pause menu.
    ///
    /// This is a full stop, not a time-scale slowdown - there's no partial
    /// "everything moves at half speed" here. If you later want hit-stop
    /// or bullet-time, that's a different, smaller feature (a float
    /// TimeScale multiplied into each dt) - don't reach for IsPaused for it.
    ///
    /// Toggling and drawing whatever pause UI you show is entirely up to
    /// game code - Scene only owns the freeze. See PauseExample.cs.
    /// </summary>
    public bool IsPaused;

    //public void Update(GameTime gameTime)
    //{
    //    if (!IsPaused)
    //    {
    //        Root.Update(gameTime);
    //        Physics.Step();
    //        Combat.Step();
    //        Camera.Update(gameTime);
    //    }

    //    // UI always updates - a pause menu needs to keep receiving clicks
    //    // while gameplay itself is frozen.
    //    Ui.Update(gameTime);
    //}

    public void Update(GameTime gameTime)
    {
        if (!IsPaused)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Root.Update(gameTime);  // Scripts only now (no double-move)
            Physics.Step(dt);       // Now owns integration + resolve
            Combat.Step();
            Camera.Update(gameTime);
        }

        Ui.Update(gameTime);
    }
    public void Draw(Renderer renderer)
    {
        renderer.Camera = Camera;
        Root.Draw(renderer);

        // UI draw methods ignore Camera by design (see Renderer) - drawn
        // last so it renders on top of the world.
        Ui.Draw(renderer);
    }
}
