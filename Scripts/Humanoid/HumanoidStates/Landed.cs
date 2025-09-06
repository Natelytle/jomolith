using static Jomolith.Scripts.Humanoid.HumanoidStateMachine;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class Landed(KineticHumanoid player, StateType priorState) 
    : RunningBase("Landed", player, priorState, kP: 15000f)
{
    public override void OnEnter()
    {
        base.OnEnter();

        Timer = 0.05;
    }

    public override void PhysicsProcess(double delta)
    {
        base.PhysicsProcess(delta);
        
        // Transition to other states
        if (ComputeEvent(EventType.TimerUp))
        {
            InvokeFinished(this, StateType.Running);
        }
        // else if (ComputeEvent(EventType.OffFloor))
        // {
        //     InvokeFinished(this, StateType.Falling);
        // }
    }
}