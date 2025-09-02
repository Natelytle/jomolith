using Godot;
using static Jomolith.Scripts.Humanoid.HumanoidStateMachine;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class Falling(Humanoid player)
    : Moving("Falling", player, 143f, kP: 5000f)
{
    public override void OnEnter()
    {
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

        float floorDistance = Player.GetFloorDistance();
        
        // We are touching the ground if floor distance is the same as our leg length with a bit of margin for error.
        // To prevent jumping from snapping to floors, we also make sure we have basically no upwards linear velocity.
        bool touchingGround = floorDistance < Humanoid.HipHeight + 0.05 && Player.LinearVelocity.Y < 5;

        if (touchingGround)
        {
            float impactForce = -Player.LinearVelocity.Y * Player.Mass;
            Player.ApplyCentralImpulse(Humanoid.WorldYVector * impactForce);
        }

        float backwardsVelocity = Player.GetLinearVelocity().Project(-Player.PlayerZVector).Length();

        if (ComputeEvent(EventType.FacingLadder) && backwardsVelocity < 0.5)
        {
            InvokeFinished(this, ComputeEvent(EventType.OnFloor) ? StateType.StandClimbing : StateType.Climbing);
        }
        else if (ComputeEvent(EventType.OnFloor))
        {
            InvokeFinished(this, StateType.Running);
        }
    }
}