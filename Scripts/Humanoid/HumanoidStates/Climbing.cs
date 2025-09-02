using System;
using Godot;
using static Jomolith.Scripts.Humanoid.HumanoidStates.HumanoidStateMachine;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class Climbing(Humanoid player)
    : Balancing("Climbing", player)
{
    public override void OnEnter()
    {
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

        // Counteract gravity and adjust horizontal speed to 0.
        Player.ApplyCentralForce(-Player.GetGravity() * Player.Mass);
        Vector3 correctionVector = new(-Player.LinearVelocity.X, 0, -Player.LinearVelocity.Z);
        float adjustmentForce = Math.Min(correctionVector.Length() * 50f, 14000f);
        Player.ApplyCentralForce(correctionVector.Normalized() * adjustmentForce);

        float floorDistance = Player.GetFloorDistance();

        // We are touching the ground if floor distance is the same as our leg length with a bit of margin for error.
        bool touchingGround = floorDistance < Humanoid.HipHeight + 0.05;
        
        Vector3 targetMovementVector = Player.GetMoveDirection();

        Climb(targetMovementVector);
        
        // Transition to other states
        if (ComputeEvent(EventType.AwayLadder))
        {
            InvokeFinished(this, StateType.Falling);
        }
        else if (ComputeEvent(EventType.OnFloor))
        {
            InvokeFinished(this, StateType.StandClimbing);
        }
        else if (ComputeEvent(EventType.JumpCommand))
        {
            Player.LadderJump();
            InvokeFinished(this, StateType.Falling);
        }
    }

    private void Climb(Vector3 target)
    {
        Vector3 velocityVector = new(Player.LinearVelocity.X, 0, Player.LinearVelocity.Z);

        if (target.Length() > 0)
        {
            float angle = target.AngleTo(Player.PlayerZVector);

            bool isClimbDownAngle = angle > float.DegreesToRadians(100f);

            velocityVector += isClimbDownAngle ? Humanoid.WorldYVector * -Player.GetWalkSpeed() * 0.7f : Humanoid.WorldYVector * Player.GetWalkSpeed() * 0.7f;
        }
		
        Player.SetLinearVelocity(velocityVector);
    }
}