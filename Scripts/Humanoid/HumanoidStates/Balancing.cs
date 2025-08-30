using Godot;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class Balancing : HumanoidState
{
    private readonly float _kP;
    private readonly float _kD;

    private int _tick;
    private Vector3 _lastTorque;

    private const int BalanceRate = 1;

    protected Balancing(string stateName, Humanoid player, float kP = 2250.0f, float kD = 50.0f)
        : base(stateName, player)
    {
        _kP = kP;
        _kD = kD;
    }

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
        if (_tick > 0)
        {
            _tick--;
            
            Player.ApplyTorque(_lastTorque);
            return;
        }
        
        Vector3 worldUp = Vector3.Up;
        Vector3 playerUp = Player.GlobalTransform.Basis.Y;

        Vector3 tiltWorld = worldUp.Cross(playerUp);

        Vector3 angVelWorld = Player.AngularVelocity;

        Basis rootBasis = Player.GlobalTransform.Basis;
        Vector3 tiltLocal = tiltWorld * rootBasis;
        Vector3 angVelLocal = angVelWorld * rootBasis;

        Vector3 controlTorqueLocal = -_kP * (Humanoid.TempInertia * tiltLocal);
        Vector3 torqueLocal = controlTorqueLocal - _kD * (Humanoid.TempInertia * angVelLocal);

        Vector3 appliedTorque = rootBasis * torqueLocal;

        Player.ApplyTorque(appliedTorque);
        _lastTorque = appliedTorque;
                    
        _tick = BalanceRate;
    }
}