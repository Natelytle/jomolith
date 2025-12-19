using static Jomolith.Src.Humanoid.Humanoid;

namespace Jomolith.Src.Humanoid.HumanoidStates;

public class Falling(Humanoid player, StateType priorState)
    : FallingBase("Falling", player, priorState)
{
    public override void OnEnter()
    {
        Player.AnimationPlayer.SetCurrentAnimation("Fall");
        Player.AnimationPlayer.SetSpeedScale(1);
    }

    public override void PhysicsProcess(double delta)
    {
        base.PhysicsProcess(delta);

        if (ComputeEvent(EventType.FacingLadder) && !ComputeEvent(EventType.InFloor))
        {
            InvokeFinished(this, ComputeEvent(EventType.OnFloor) ? StateType.StandClimbing : StateType.Climbing);
        }
        else if (ComputeEvent(EventType.OnFloor))
        {
            InvokeFinished(this, StateType.Landed);
        }
    }
}