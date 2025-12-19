using System.Numerics;
using static Jomolith.Src.Humanoid.Humanoid;

namespace Jomolith.Src.Humanoid.HumanoidStates;

public class Idle(Humanoid player, StateType priorState) 
    : RunningBase("Idle", player, priorState)
{
    public override void OnEnter()
    {
        Player.AnimationPlayer.SetCurrentAnimation("Idle");
        Player.AnimationPlayer.SetSpeedScale(1);
    }

    public override void OnExit()
    {
    }
    
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
        else if (!ComputeEvent(EventType.IsIdle))
        {
            InvokeFinished(this, StateType.Running);
        }
    }
}