using Godot;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class Coyote(Humanoid player) : Balancing("Coyote", player, 5000f, 50f)
{
    private const float Acceleration = 150f;
    private const double CoyoteTime = 0.125d; 

    private double _coyoteTimer;

    public override void OnEnter()
    {
        _coyoteTimer = 0;
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
        
        Vector3 targetMovementVector = Player.GetMoveDirection();
        
        Player.Walk(targetMovementVector, Acceleration);

        if (Player.RotationLocked)
            Player.SnapToCamera();
        else
            Player.RotateTo(targetMovementVector);
        
        _coyoteTimer += delta;
        
        // Transition to other states
        if (Player.IsClimbing())
        {
            InvokeFinished(this, "Climbing");
        }
        else if (touchingGround)
        {
            InvokeFinished(this, "Standing");
        }
        else if (_coyoteTimer > CoyoteTime)
        {
            InvokeFinished(this, "Falling");
        }
        else if (Input.IsActionPressed("jump"))
        {
            Player.Jump();
            InvokeFinished(this, "Falling");
        }
    }
}