using RoyaResan.Mono2d.Animation;
using RoyaResan.Mono2d.Scripting;

namespace RoyaResan.Mono2d.Gameplay;

/// <summary>Size/color/offset for one Animator state's placeholder rectangle.</summary>
public struct PlaceholderPose
{
    public Vector2 Size;
    public Vector2 Offset;
    public Color Color;

    public PlaceholderPose(Vector2 size, Vector2 offset, Color color)
    {
        Size = size;
        Offset = offset;
        Color = color;
    }
}

/// <summary>
/// Drives a PlaceholderRectNode's Size/Position/Color from the owner's
/// current Animator state - same pattern as HurtboxProfileScript, just
/// applied to the placeholder visual instead of the hurtbox. This is
/// what "Use Animator for animation state changes" means for a prototype
/// with no spritesheets: the Animator is still the single source of
/// truth for what state the character is in, it just drives a colored
/// rectangle's shape instead of picking a spritesheet frame.
///
/// Usage:
///   var pose = new PlaceholderPoseScript { Animator = animator, Visual = rectNode };
///   pose.States["Idle"]   = new PlaceholderPose(new Vector2(24, 48), Vector2.Zero, Color.White);
///   pose.States["Crouch"] = new PlaceholderPose(new Vector2(24, 28), new Vector2(0, 10), Color.White);
///   pose.States["Attack"] = new PlaceholderPose(new Vector2(24, 48), Vector2.Zero, Color.Yellow);
///   body.AddScript(pose);
///
/// Offset is added to Visual.Position each frame (in local space) - use
/// it for poses that should sit lower/higher than the idle rect (crouch
/// shrinking from the top down, since feet should stay planted).
/// </summary>
public class PlaceholderPoseScript : Script
{
    public Animator Animator;
    public PlaceholderRectNode Visual;

    public PlaceholderPose Default = new PlaceholderPose(new Vector2(24, 48), Vector2.Zero, Color.White);
    public readonly Dictionary<string, PlaceholderPose> States = new();

    public override void Update(GameTime gameTime)
    {
        var pose = States.TryGetValue(Animator.CurrentStateName, out var p) ? p : Default;

        Visual.Size = pose.Size;
        Visual.Position = pose.Offset;
        Visual.Color = pose.Color;
    }
}