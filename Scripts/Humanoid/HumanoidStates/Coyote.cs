using static Jomolith.Scripts.Humanoid.Humanoid;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class Coyote(Humanoid player, StateType priorState)
    : FallingBase("Coyote", player, priorState)
{
    private const double CoyoteTime = 0.125d; 

    public override void OnEnter()
    {
        base.OnEnter();

        Timer = CoyoteTime;
        
        Player.AnimationPlayer.SetCurrentAnimation("Fall");
        Player.AnimationPlayer.SetSpeedScale(1);
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

        if (ComputeEvent(EventType.FacingLadder))
        {
            InvokeFinished(this, StateType.Climbing);
        }
        else if (ComputeEvent(EventType.OnFloor))
        {
            InvokeFinished(this, StateType.Running);
        }
        else if (ComputeEvent(EventType.TimerUp))
        {
            InvokeFinished(this, StateType.Falling);
        }
        else if (ComputeEvent(EventType.JumpCommand))
        {
            InvokeFinished(this, StateType.Jumping);
        }
    }
}