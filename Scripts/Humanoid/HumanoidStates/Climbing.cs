using System;
using Godot;
using static Jomolith.Scripts.Humanoid.HumanoidStateMachine;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class Climbing(RigidHumanoid player, StateType priorState)
    : Balancing("Climbing", player, priorState, 2250f, 50f)
{
    public override void OnEnter()
    {
        Player.SetClimbingPosition();
    }

    public override void OnExit()
    {
        Player.SetDefaultPosition();
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
        float adjustmentForce = Math.Min(correctionVector.Length() * 50f, 14000f) * Player.Mass;
        Player.ApplyCentralForce(correctionVector.Normalized() * adjustmentForce);
        Player.AngularVelocity = new Vector3(Player.AngularVelocity.X, 0, Player.AngularVelocity.Z);

        float floorDistance = Player.GetFloorDistance();

        // We are touching the ground if floor distance is the same as our leg length with a bit of margin for error.
        bool touchingGround = floorDistance < RigidHumanoid.HipHeight + 0.05;
        
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
            InvokeFinished(this, StateType.Jumping);
        }
    }

    private void Climb(Vector3 target)
    {
        Vector3 velocityVector = new(Player.LinearVelocity.X, 0, Player.LinearVelocity.Z);

        if (target.Length() > 0)
        {
            float angle = target.AngleTo(Player.PlayerZVector);

            bool isClimbDownAngle = angle > float.DegreesToRadians(100f);

            velocityVector += isClimbDownAngle ? RigidHumanoid.WorldYVector * -Player.GetWalkSpeed() * 0.7f : RigidHumanoid.WorldYVector * Player.GetWalkSpeed() * 0.7f;
        }
		
        Player.SetLinearVelocity(velocityVector);
    }
}