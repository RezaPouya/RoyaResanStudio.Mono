namespace RoyaResan.Mono2d.AI;

public class Ai
{
    private AiStateMachine _stateMachine;

    public Ai(AiStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }

    public void Update(float dt)
    {
        _stateMachine.Update(dt);
    }

    public void ChangeState(AiState newState)
    {
        _stateMachine.ChangeState(newState);
    }
}