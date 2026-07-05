using RoyaResan.Mono2d.Scripting;

namespace RoyaResan.Mono2d.AI;

/// <summary>
/// Attach via PhysicsBody.AddScript(new EnemyFsmScript { Fsm = myFsm }).
/// Wires Fsm.Body to this script's Owner and ticks the state machine
/// every frame, same as any other Script.
/// </summary>
public class EnemyFsmScript : Script
{
    public EnemyFsm Fsm;

    public override void Start()
    {
        if (Fsm != null)
            Fsm.Body = Owner;
    }

    public override void Update(GameTime gameTime)
    {
        Fsm?.Update(gameTime);
    }
}