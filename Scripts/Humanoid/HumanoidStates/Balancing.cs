using Godot;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public class Balancing : HumanoidState
{
    private float _kP = 2250.0f;
    private float _kD = 50.0f;

    public Balancing(string stateName, Humanoid player, float kP, float kD)
        : base(stateName, player)
    {
        _kP = kP;
        _kD = kD;
    }
    
    public Balancing(string stateName, Humanoid player)
    : base(stateName, player) { }

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
        Vector3 worldUp = Vector3.Up;
        Vector3 playerUp = Player.GlobalTransform.Basis.Y;

        Vector3 tiltWorld = worldUp.Cross(playerUp);

        Vector3 angVelWorld = Player.AngularVelocity;

        Basis rootBasis = Player.GlobalTransform.Basis;
        Vector3 tiltLocal = tiltWorld * rootBasis;
        Vector3 angVelLocal = angVelWorld * rootBasis;

        Vector3 inertia = new(2.16f, 0.41f, 2.41f);

        Vector3 controlTorqueLocal = -_kP * (inertia * tiltLocal);
        Vector3 torqueLocal = controlTorqueLocal - _kD * (inertia * angVelLocal);

        Vector3 torqueWorld = rootBasis * torqueLocal;

        Player.ApplyTorque(torqueWorld);
    }
}