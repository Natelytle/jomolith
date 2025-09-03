using Godot;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class Balancing(string stateName, Humanoid player, float kP, float kD)
    : HumanoidState(stateName, player)
{
    private int _tick;
    private Vector3 _lastTorque;

    private const int BalanceRate = 2;

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
        // if (_tick > 0)
        // {
        //     _tick--;
        //     
        //     Player.ApplyTorque(_lastTorque);
        //     return;
        // }
        
        Vector3 worldUp = Vector3.Up;
        Vector3 playerUp = Player.GlobalTransform.Basis.Y;

        Vector3 tiltWorld = worldUp.Cross(playerUp);

        Vector3 angVelWorld = Player.AngularVelocity;

        Basis rootBasis = Player.GlobalTransform.Basis;
        Vector3 tiltLocal = tiltWorld * rootBasis;
        Vector3 angVelLocal = angVelWorld * rootBasis;

        Vector3 controlTorqueLocal = -kP * (Player.GetInertia() * tiltLocal);
        Vector3 torqueLocal = controlTorqueLocal - kD * (Player.GetInertia() * angVelLocal);

        Vector3 appliedTorque = rootBasis * torqueLocal;

        // We don't want to add torque counter to our player turning when walking.
        appliedTorque.Y = 0;

        Player.ApplyTorque(appliedTorque);
        // _lastTorque = appliedTorque;
        //             
        // _tick = BalanceRate;
    }
}