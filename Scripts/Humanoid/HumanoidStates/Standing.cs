using Godot;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class Standing(Humanoid player) : Balancing("Standing", player)
{
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

        Vector3 targetMovementVector = Player.GetMoveDirection();
        
        Player.Walk(targetMovementVector, Acceleration);
        
        if (Player.RotationLocked)
            Player.SnapToCamera();
        else
            Player.RotateTo(targetMovementVector);
        
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