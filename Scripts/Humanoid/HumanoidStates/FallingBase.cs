
using Godot;
using static Jomolith.Scripts.Humanoid.HumanoidStateMachine;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class FallingBase(string stateName, RigidHumanoid player, StateType priorState, float kP = 10000f)
    : Moving(stateName, player, priorState, 143.0f, kP, 100f)
{
    public override void OnEnter()
    {
        Player.GetPhysicsMaterialOverride().Friction = 0.3f;
    }

    public override void PhysicsProcess(double delta)
    {
        base.PhysicsProcess(delta);
        
        // We don't want to check for if the player is locked, this is how lodges work.
        Vector3 playerHeading = Player.GetPlayerHeading();
        float angle = playerHeading.SignedAngleTo(Player.GetMoveDirection(), Vector3.Up);
        float desiredRotationalVelocity = float.Abs(angle) > 1 ? 8.0f * float.Sign(angle) : 8.0f * angle;

        float desiredTorque = 100f * Player.GetInertia().Y * (desiredRotationalVelocity - Player.AngularVelocity.Y);
	
        Player.ApplyTorque(Vector3.Up * desiredTorque);
    }
}