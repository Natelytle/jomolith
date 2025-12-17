using System;
using Godot;
using static Jomolith.Scripts.Humanoid.Humanoid;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class Climbing(Humanoid player, StateType priorState)
    : Balancing("Climbing", player, priorState, 2250f, 50f)
{
    public override void OnEnter()
    {
        Player.AnimationPlayer.SetCurrentAnimation("Climb");
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

        // Counteract gravity and adjust horizontal speed to 0.
        Player.ApplyCentralForce(-Player.GetGravity() * Player.Mass);
        Vector3 correctionVector = new(-Player.LinearVelocity.X, 0, -Player.LinearVelocity.Z);
        float adjustmentForce = Math.Min(correctionVector.Length() * 50f, 14000f) * Player.Mass;
        Player.ApplyCentralForce(correctionVector.Normalized() * adjustmentForce);
        Player.AngularVelocity = new Vector3(Player.AngularVelocity.X, 0, Player.AngularVelocity.Z);

        Climb(Player.MoveDirection);

        Player.AnimationPlayer.SetSpeedScale(Player.LinearVelocity.Y / 14.5f);
        
        // Transition to other states
        if (ComputeEvent(EventType.AwayLadder))
        {
            InvokeFinished(this, StateType.Falling);
        }
        else if (ComputeEvent(EventType.OnFloor))
        {
            InvokeFinished(this, StateType.StandClimbing);
        }
        else if (ComputeEvent(EventType.JumpCommand))
        {
            InvokeFinished(this, StateType.Jumping);
        }
    }

    private void Climb(Vector3 target)
    {
        Vector3 velocityVector = new(Player.LinearVelocity.X, 0, Player.LinearVelocity.Z);

        if (target.Length() > 0)
        {
            float angle = target.AngleTo(-Player.Basis.Z);

            bool isClimbDownAngle = angle > float.DegreesToRadians(100f);

            velocityVector += isClimbDownAngle ? Player.GlobalBasis.Y * -Player.WalkSpeed * 0.7f : Player.GlobalBasis.Y * Player.WalkSpeed * 0.7f;
        }
		
        Player.SetLinearVelocity(velocityVector);
    }
}