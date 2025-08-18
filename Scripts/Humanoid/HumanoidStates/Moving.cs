using System;
using Godot;
using Jomolith.Scripts.Utils;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class Moving(string stateName, Humanoid player, float accel = 800f, float kP = 2250f, float kD = 50f)
    : Balancing(stateName, player, kP, kD)
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

        Vector3 targetMovementVector = Player.GetMoveDirection();
        Vector3 target = targetMovementVector * Player.GetWalkSpeed();
        Vector3 correctionVector = target - new Vector3(Player.LinearVelocity.X, 0, Player.LinearVelocity.Z);

        float length = Math.Min(accel, 100 * correctionVector.Length());

        correctionVector = correctionVector.Normalized() * length;
        
        Player.ApplyCentralForce(correctionVector);

        Vector3 rotationTarget = target;

        // We don't actually want to rotate when 
        if (Player.RotationLocked)
        {
            rotationTarget = Player.GetPlayerHeading();
        }
        
        Vector3 otherRotation = Player.AngularVelocity.RemoveAngularComponent(Humanoid.WorldBasis.Y);

        if (target.Length() == 0)
        {
            Player.SetAngularVelocity(otherRotation);
            return;
        }

        Vector3 playerHeading = Player.GetPlayerHeading();
        float angle = playerHeading.AngleTo(rotationTarget);
        Vector3 cross = playerHeading.Cross(rotationTarget).Normalized();
		
        Player.SetAngularVelocity(otherRotation + cross * angle * 10.0f);
    }
}