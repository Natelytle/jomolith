using Godot;
using static Jomolith.Scripts.Humanoid.HumanoidStateMachine;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class Running(RigidHumanoid player, StateType priorState) 
    : RunningBase("Running", player, priorState)
{
    public override void PhysicsProcess(double delta)
    {
        base.PhysicsProcess(delta);
        
        // Transition to other states
        if (ComputeEvent(EventType.FacingLadder))
        {
            InvokeFinished(this, StateType.StandClimbing);
        }
        else if (ComputeEvent(EventType.JumpCommand))
        {
            InvokeFinished(this, StateType.Jumping);
        }
        else if (ComputeEvent(EventType.OffFloor))
        {
            InvokeFinished(this, StateType.Coyote);
        }
    }
}