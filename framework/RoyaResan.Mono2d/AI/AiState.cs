namespace RoyaResan.Mono2d.AI;

public abstract class AiState
{
    protected AiContext Context;

    public AiState(AiContext context)
    {
        Context = context;
    }

    public virtual void Enter()
    { }

    public virtual void Update(float dt)
    { }

    public virtual void Exit()
    { }
}