
using Godot;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class RunningBase(string stateName, Humanoid player, float kP = 5500f)
    : Moving(stateName, player, 741.6f, kP, 50f)
{
    private const float KAltitudeP = 30000f;
    private const float KAltitudeD = 1100f;

    private float _prevSpeedY;

    public override void PhysicsProcess(double delta)
    {
        base.PhysicsProcess(delta);
        
        float floorDistance = Player.GetFloorDistance();
        
        // Counteract gravity and adjust our legs to the floor.
        float desiredYAccel = KAltitudeP * (Humanoid.HipHeight - floorDistance) - KAltitudeD * Player.LinearVelocity.Y;

        if (desiredYAccel > 0f)
        {
            float currentAccelY = _prevSpeedY - Player.LinearVelocity.Y;
            _prevSpeedY = Player.LinearVelocity.Y;

            float accelDelta = desiredYAccel - currentAccelY;

            float deltaForce = accelDelta * Player.Mass;
            
            Player.ApplyCentralForce(Vector3.Up * deltaForce * 0.1f);
        }

        if (!Player.RotationLocked)
        {
            Vector3 playerHeading = Player.GetPlayerHeading();
            float angle = playerHeading.SignedAngleTo(Player.GetMoveDirection(), Vector3.Up);
            float desiredRotationalVelocity = 8.0f * angle;

            float desiredTorque = 100f * Player.GetInertia().Y * (desiredRotationalVelocity - Player.AngularVelocity.Y);
            
            const float torqueMax = 1e5f;
            desiredTorque = Mathf.Clamp(desiredTorque, -torqueMax, torqueMax);
		
            Player.ApplyTorque(Vector3.Up * desiredTorque);
        }
    }
}