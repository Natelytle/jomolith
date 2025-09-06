
using Godot;
using static Jomolith.Scripts.Humanoid.HumanoidStateMachine;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class RunningBase(string stateName, RigidHumanoid player, StateType priorState, float kP = 5500f)
    : Moving(stateName, player, priorState, 741.6f, kP, 50f)
{
    public override void OnEnter()
    {
        Player.GetPhysicsMaterialOverride().Friction = 0.3f;
        Player.GetPhysicsMaterialOverride().Bounce = 0f;
    }

    public override void PhysicsProcess(double delta)
    {
        base.PhysicsProcess(delta);
        
        float floorDistance = Player.GetFloorDistance();
        
        float desiredYVelocity = 27 * (RigidHumanoid.HipHeight - floorDistance);

        if (desiredYVelocity > 0)
        {
            Player.ApplyCentralForce(-Player.GetGravity() * Player.Mass);

            float desiredForce = 110 * (desiredYVelocity - Player.LinearVelocity.Y) * Player.Mass;
        
            Player.ApplyCentralForce(Vector3.Up * desiredForce);
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