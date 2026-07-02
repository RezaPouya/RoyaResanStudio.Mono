namespace RoyaResan.Mono2d.AI;

public class AiStateMachine
{
    public AiState CurrentState { get; private set; }

    public void Update(float dt)
    {
        CurrentState?.Update(dt);
    }

    public void ChangeState(AiState newState)
    {
        if (newState == null)
            return;

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}