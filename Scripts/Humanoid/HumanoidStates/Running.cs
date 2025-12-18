using System.Numerics;
using static Jomolith.Scripts.Humanoid.Humanoid;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class Running(Humanoid player, StateType priorState) 
    : RunningBase("Running", player, priorState)
{
    public override void OnEnter()
    {
        Player.AnimationPlayer.SetCurrentAnimation("Walk");
    }

    public override void OnExit()
    {
    }
    
    public override void PhysicsProcess(double delta)
    {
        base.PhysicsProcess(delta);

        Vector3 horizontalSpeed = new Vector3(Player.LinearVelocity.X, 0, Player.LinearVelocity.Z);
        Player.AnimationPlayer.SetSpeedScale(horizontalSpeed.Length() / 14.5f);
        
        // Transition to other states
        if (ComputeEvent(EventType.FacingLadder) && !ComputeEvent(EventType.InFloor))
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
        else if (ComputeEvent(EventType.IsIdle))
        {
            InvokeFinished(this, StateType.Idle);
        }
    }
}