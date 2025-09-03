using Godot;
using static Jomolith.Scripts.Humanoid.HumanoidStateMachine;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class Falling(Humanoid player)
    : FallingBase("Falling", player)
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

        float backwardsVelocity = Player.GetLinearVelocity().Project(-Player.PlayerZVector).Length();

        if (ComputeEvent(EventType.FacingLadder) && backwardsVelocity < 0.5)
        {
            InvokeFinished(this, ComputeEvent(EventType.OnFloor) ? StateType.StandClimbing : StateType.Climbing);
        }
        else if (ComputeEvent(EventType.OnFloor))
        {
            InvokeFinished(this, StateType.Landed);
        }
    }
}