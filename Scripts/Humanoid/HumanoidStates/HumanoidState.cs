
using Godot;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public abstract class HumanoidState(string stateName, Humanoid player)
{
    public readonly string StateName = stateName;
    protected readonly Humanoid Player = player;

    public abstract void OnEnter();
    public abstract void OnExit();
    public abstract void Process(double delta);
    public abstract void PhysicsProcess(double delta);

    protected void InvokeFinished(HumanoidState state, string name)
    {
        Finished?.Invoke(state, name);
    }

    public event FinishedEventHandler Finished;
    public delegate void FinishedEventHandler(HumanoidState state, string name);
}