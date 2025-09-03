using static Jomolith.Scripts.Humanoid.HumanoidStateMachine;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class Landed(Humanoid player) 
    : RunningBase("Landed", player, kP: 7500f)
{
    public override void OnEnter()
    {
        Player.GetPhysicsMaterialOverride().Friction = 0.3f;
        Timer = 0.05;
    }

    public override void OnExit()
    {
    }

    public override void Process(double delta)
    {
    }

    public override void PhysicsProcess(double delta)
    {
        base.PhysicsProcess(delta);
        Timer -= delta;
        
        // Transition to other states
        if (ComputeEvent(EventType.TimerUp))
        {
            InvokeFinished(this, StateType.Running);
        }
        // else if (ComputeEvent(EventType.OffFloor))
        // {
        //     InvokeFinished(this, StateType.Coyote);
        // }
    }
}