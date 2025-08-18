using System.Collections.Generic;
using Godot;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public partial class HumanoidStateMachine : Node
{
    [Export]
    public string InitialState { get; set; }

    private HumanoidState _currentState;
    private Dictionary<string, HumanoidState> _statesDictionary = new();

    public void AddState(HumanoidState state)
    {
        _statesDictionary.Add(state.StateName.ToLower(), state);

        state.Finished += OnStateFinished;
    }

    public override void _Ready()
    {
        if (InitialState is not null)
            _currentState = _statesDictionary[InitialState.ToLower()];
    }

    public override void _Process(double delta)
    {
        _currentState?.Process(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        _currentState?.PhysicsProcess(delta);
    }

    private void OnStateFinished(HumanoidState state, string newStateName)
    {
        if (state != _currentState)
            return;

        HumanoidState newState = _statesDictionary[newStateName.ToLower()];

        if (newState is null)
            return;
        
        if (_currentState is not null)
            _currentState.OnExit();
        
        newState.OnEnter();

        _currentState = newState;
    }
}