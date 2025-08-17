using System;
using Godot;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public partial class Climbing(Humanoid player) : HumanoidState("Climbing", player)
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
        Player.ApplyUprightForce();

        // Counteract gravity and adjust horizontal speed to 0.
        Player.ApplyCentralForce(-Player.GetGravity() * Player.Mass);
        Vector3 correctionVector = new Vector3(-Player.LinearVelocity.X, 0, -Player.LinearVelocity.Z);
        float adjustmentForce = Math.Min(correctionVector.Length() * 50f, 14000f);
        Player.ApplyCentralForce(correctionVector.Normalized() * adjustmentForce);

        float floorDistance = Player.GetFloorDistance();

        // We are touching the ground if floor distance is the same as our leg length with a bit of margin for error.
        bool touchingGround = floorDistance < Humanoid.HipHeight + 0.05;
        
        Vector3 targetMovementVector = Player.GetMoveDirection();

        Climb(targetMovementVector);
        
        if (Player.RotationLocked)
            Player.SnapToCamera();
        
        // Transition to other states
        if (!Player.IsClimbing())
        {
            EmitSignalFinished(this, "Falling");
        }
        else if (touchingGround)
        {
            EmitSignalFinished(this, "StandClimbing");
        }
    }

    private void Climb(Vector3 target)
    {
        Vector3 velocityVector = new Vector3(Player.LinearVelocity.X, 0, Player.LinearVelocity.Z);

        if (target.Length() > 0)
        {
            float angle = target.AngleTo(Player.PlayerZVector);

            bool isClimbDownAngle = angle > float.DegreesToRadians(100f);

            velocityVector += isClimbDownAngle ? Humanoid.WorldYVector * -16 : Humanoid.WorldYVector * 16;
        }
		
        Player.SetLinearVelocity(velocityVector);
    }
}