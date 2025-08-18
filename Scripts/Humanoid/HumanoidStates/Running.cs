using Godot;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class Running(Humanoid player) 
    : Moving("Running", player)
{
    public override void OnEnter()
    {
        Player.GetPhysicsMaterialOverride().Friction = 0.3f;
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
        bool touchingGround = floorDistance < Humanoid.HipHeight + 0.05;

        if (touchingGround)
        {
            // Counteract gravity and adjust our legs to the floor.
            Player.ApplyCentralForce(-Player.GetGravity() * Player.Mass);
            Player.SetAxisVelocity(Player.PlayerYVector * (Humanoid.HipHeight - floorDistance) * 20);
        }
        
        // Transition to other states
        if (Player.IsClimbing())
        {
            InvokeFinished(this, "StandClimbing");
        }
        else if (Input.IsActionPressed("jump"))
        {
            Player.Jump();
            InvokeFinished(this, "Falling");
        }
        else if (!touchingGround)
        {
            InvokeFinished(this, "Coyote");
        }
    }
}