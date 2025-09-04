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
        
        Vector3 playerUp = Player.GlobalTransform.Basis.Y;
        Vector3 tiltWorld = Vector3.Up.Cross(playerUp);
        Vector3 angVelWorld = Player.AngularVelocity;
        Vector3 externalTorqueWorld = Player.GetExternalTorque() - _lastTorque;

        Basis rootBasis = Player.GlobalTransform.Basis;

        Vector3 tiltLocal = tiltWorld * rootBasis;
        Vector3 angVelLocal = angVelWorld * rootBasis;
        Vector3 externalTorqueLocal = externalTorqueWorld * rootBasis;

        Vector3 inertiaVector = Player.GetInertia();

        Vector3 controlTorqueLocal = -kP * (inertiaVector * tiltLocal);
        Vector3 torqueLocal = externalTorqueLocal + controlTorqueLocal;
            
        torqueLocal -= kD * (inertiaVector * angVelLocal);
        
        Vector3 appliedTorque = rootBasis * torqueLocal - externalTorqueWorld;

        // We don't want to add torque counter to our player turning when walking.
        appliedTorque.Y = 0;

        Player.ApplyTorque(appliedTorque);
        // _lastTorque = appliedTorque;
        //             
        // _tick = BalanceRate;
    }
}