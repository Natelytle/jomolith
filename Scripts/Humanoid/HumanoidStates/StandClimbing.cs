using System;
using Godot;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public partial class StandClimbing(Humanoid player) : HumanoidState("StandClimbing", player)
{
    // Acceleration when dismounting this ladder
    private const float Acceleration = 800f;

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
            EmitSignalFinished(this, "Standing");
        }
        else if (!touchingGround)
        {
            EmitSignalFinished(this, "Climbing");
        }
    }
    
    private void Climb(Vector3 target)
    {
        Vector3 velocityVector = new Vector3(Player.LinearVelocity.X, 0, Player.LinearVelocity.Z);

        if (target.Length() > 0)
        {
            float angle = target.AngleTo(Player.PlayerZVector);

            bool isClimbDownAngle = angle > float.DegreesToRadians(100f);

            if (isClimbDownAngle)
            {
                Player.Walk(target, Acceleration);

                if (!Player.RotationLocked)
                {
                    Player.RotateTo(target);
                }

                return;
            }

            velocityVector += Humanoid.WorldYVector * 16;
        }

        Player.SetLinearVelocity(velocityVector);
    }
}