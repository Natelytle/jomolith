using System;
using Godot;
using Jomolith.Scripts.Utils;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class Moving(string stateName, Humanoid player, float maxAccel = 741.6f, float kP = 2250f, float kD = 50f)
    : Balancing(stateName, player, kP, kD)
{
    private const float Gain = 150f;

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

        float length = Math.Min(maxAccel, Gain * correctionVector.Length());

        correctionVector = correctionVector.Normalized() * length;
        
        Player.ApplyCentralForce(correctionVector);

        if (!Player.RotationLocked)
        {
            Vector3 playerHeading = Player.GetPlayerHeading();
            float angle = playerHeading.SignedAngleTo(target, Vector3.Up);
            float desiredRotationalVelocity = 10.0f * angle;

            float desiredTorque = 175f * Humanoid.TempInertia.Y * (desiredRotationalVelocity - Player.AngularVelocity.Dot(Vector3.Up));
		
            Player.ApplyTorque(Vector3.Up * desiredTorque);
        }
    }
}