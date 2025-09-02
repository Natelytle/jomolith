using Godot;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class Falling(Humanoid player)
    : Moving("Falling", player, 143f, kP: 5000f)
{
    public override void OnEnter()
    {
        Player.GetPhysicsMaterialOverride().Friction = 0f;
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

        float floorDistance = Player.GetFloorDistance();
        
        // We are touching the ground if floor distance is the same as our leg length with a bit of margin for error.
        // To prevent jumping from snapping to floors, we also make sure we have basically no upwards linear velocity.
        bool touchingGround = floorDistance < Humanoid.HipHeight + 0.05 && Player.LinearVelocity.Y < 5;

        if (touchingGround)
        {
            float impactForce = -Player.LinearVelocity.Y * Player.Mass;
            Player.ApplyCentralImpulse(Humanoid.WorldYVector * impactForce);
        }

        float backwardsVelocity = Player.GetLinearVelocity().Project(-Player.PlayerZVector).Length();

        // Transition to other states
        if (Player.IsClimbing() && backwardsVelocity < 0.5)
        {
            InvokeFinished(this, touchingGround ? "StandClimbing" : "Climbing");
        }
        else if (touchingGround)
        {
            InvokeFinished(this, "Running");
        }
    }
}