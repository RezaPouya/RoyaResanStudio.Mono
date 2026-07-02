using RoyaResan.Mono2d.AI;

namespace RoyaResan.Mono2d.Components;

public class AIComponent : Component
{
    private Ai _ai;

    public AIComponent(Ai ai)
    {
        _ai = ai;
    }

    public override void Update(float dt)
    {
        _ai.Update(dt);
    }
}