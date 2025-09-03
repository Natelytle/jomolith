using Godot;
using static Jomolith.Scripts.Humanoid.HumanoidStateMachine;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class Coyote(Humanoid player)
    : FallingBase("Coyote", player)
{
    private const double CoyoteTime = 0.125d; 

    public override void OnEnter()
    {
        Timer = CoyoteTime;
        Player.GetPhysicsMaterialOverride().Friction = 0f;
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

        if (ComputeEvent(EventType.FacingLadder))
        {
            InvokeFinished(this, StateType.Climbing);
        }
        else if (ComputeEvent(EventType.OnFloor))
        {
            InvokeFinished(this, StateType.Landed);
        }
        else if (ComputeEvent(EventType.TimerUp))
        {
            InvokeFinished(this, StateType.Falling);
        }
        else if (ComputeEvent(EventType.JumpCommand))
        {
            Player.Jump();
            InvokeFinished(this, StateType.Falling);
        }
    }
}