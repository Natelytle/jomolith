using System;
using Godot;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class StandClimbing(Humanoid player) : Balancing("StandClimbing", player)
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
        float angle = targetMovementVector.AngleTo(Player.GetPlayerHeading());
        bool isClimbDownAngle = angle > float.DegreesToRadians(100f);

        if (!isClimbDownAngle)
            Climb(targetMovementVector);
        
        // Transition to other states
        if (!Player.IsClimbing() || isClimbDownAngle)
        {
            InvokeFinished(this, "Running");
        }
        else if (!touchingGround)
        {
            InvokeFinished(this, "Climbing");
        }
        else if (Input.IsActionPressed("jump"))
        {
            Player.LadderJump();
            InvokeFinished(this, "Falling");
        }
    }
    
    private void Climb(Vector3 target)
    {
        Vector3 velocityVector = new(Player.LinearVelocity.X, 0, Player.LinearVelocity.Z);

        if (target.Length() > 0)
        {
            velocityVector += Humanoid.WorldYVector * 16;
        }

        Player.SetLinearVelocity(velocityVector);
    }
}