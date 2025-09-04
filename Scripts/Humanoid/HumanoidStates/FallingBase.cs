
using Godot;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class FallingBase(string stateName, Humanoid player, float kP = 10000f)
    : Moving(stateName, player, 143.0f, kP, 100f)
{
    public override void OnEnter()
    {
        Player.GetPhysicsMaterialOverride().Friction = 0f;
        Player.GetPhysicsMaterialOverride().Bounce = 1f;
    }

    public override void PhysicsProcess(double delta)
    {
        base.PhysicsProcess(delta);
        
        if (!Player.RotationLocked)
        {
            Vector3 playerHeading = Player.GetPlayerHeading();
            float angle = playerHeading.SignedAngleTo(Player.GetMoveDirection(), Vector3.Up);
            float desiredRotationalVelocity = float.Abs(angle) > 1 ? 8.0f * float.Sign(angle) : 8.0f * angle;

            float desiredTorque = 100f * Player.GetInertia().Y * (desiredRotationalVelocity - Player.AngularVelocity.Y);
		
            Player.ApplyTorque(Vector3.Up * desiredTorque);
        }
    }
}