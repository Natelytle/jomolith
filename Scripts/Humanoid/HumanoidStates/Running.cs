using Godot;
using static Jomolith.Scripts.Humanoid.HumanoidStateMachine;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class Running(Humanoid player) 
    : RunningBase("Running", player)
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
            Player.Jump();
            InvokeFinished(this, StateType.Falling);
        }
        else if (ComputeEvent(EventType.OffFloor))
        {
            InvokeFinished(this, StateType.Coyote);
        }
    }
}