using Godot;
using static Jomolith.Scripts.Humanoid.HumanoidStates.HumanoidStateMachine;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class Coyote(Humanoid player)
    : Moving("Coyote", player, 143f, kP: 5000f)
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

        float floorDistance = Player.GetFloorDistance();

        // We are touching the ground if floor distance is the same as our leg length with a bit of margin for error.
        // To prevent jumping from snapping to floors, we also make sure we have basically no upwards linear velocity.
        bool touchingGround = floorDistance < Humanoid.HipHeight + 0.05 && Player.LinearVelocity.Y < 5;

        if (touchingGround)
        {
            float impactForce = -Player.LinearVelocity.Y * Player.Mass;
            Player.ApplyCentralImpulse(Humanoid.WorldYVector * impactForce);
        }

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
            Player.Jump();
            InvokeFinished(this, StateType.Falling);
        }
    }
}