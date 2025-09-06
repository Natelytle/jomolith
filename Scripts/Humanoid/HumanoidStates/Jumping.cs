using Godot;
using static Jomolith.Scripts.Humanoid.HumanoidStateMachine;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class Jumping(KineticHumanoid player, StateType priorState) 
    : FallingBase("Jumping", player, priorState)
{
    private Vector3 _jumpDirection;
    private readonly StateType _priorState = priorState;

    public override void OnEnter()
    {
        base.OnEnter();

        if (_priorState is StateType.Climbing or StateType.StandClimbing)
        {
            Vector3 backwardsVector = -Player.GetPlayerHeading();

            _jumpDirection = (Vector3.Up + backwardsVector).Normalized();
        }
        else
        {
            _jumpDirection = Vector3.Up;
        }
    }

    public override void PhysicsProcess(double delta)
    {
        base.PhysicsProcess(delta);

        Player.SetAxisVelocity(_jumpDirection * Player.GetJumpPower());

        InvokeFinished(this, StateType.Falling);
    }
}