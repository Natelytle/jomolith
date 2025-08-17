
using Godot;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public abstract partial class HumanoidState(string stateName, Humanoid player) : Node
{
    public readonly string StateName = stateName;
    protected Humanoid Player = player;

    public abstract void OnEnter();
    public abstract void OnExit();
    public abstract void Process(double delta);
    public abstract void PhysicsProcess(double delta);
    
    [Signal]
    public delegate void FinishedEventHandler(HumanoidState state, string name);
}