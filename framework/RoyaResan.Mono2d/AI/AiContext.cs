using RoyaResan.Mono2d.Animation;
using RoyaResan.Mono2d.Node;

namespace RoyaResan.Mono2d.AI;

public class AiContext
{
    public TransformNode Transform;
    public Animator Animator;

    public TransformNode Target;

    public float MoveSpeed = 2f;
    public float AttackRange = 1.5f;
}