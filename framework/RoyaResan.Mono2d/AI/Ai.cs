using Microsoft.Xna.Framework;
using RoyaResan.Mono2d.Nodes;

namespace RoyaResan.Mono2d.AI;

public class Ai
{
    private readonly AiStateMachine _stateMachine;
    private readonly TransformNode _owner;

    public Ai(TransformNode owner, AiStateMachine stateMachine)
    {
        _owner = owner;
        _stateMachine = stateMachine;
    }

    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _stateMachine.Update(dt);
    }
}