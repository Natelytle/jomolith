using System;
using System.Collections.Generic;
using Godot;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public partial class HumanoidStateMachine : Node
{
    [Export]
    public StateType InitialState { get; set; }
    
    [Export]
    public Humanoid Player { get; set; }

    private HumanoidState _currentState;

    public enum StateType
    {
        Running,
        Coyote,
        Falling,
        Climbing,
        StandClimbing,
        Jumping,
        Landed
    }

    private HumanoidState GetState(StateType stateType)
    {
        HumanoidState state = null;
        
        switch (stateType)
        {
            case StateType.Running:
                state = new Running(Player);
                break;
            case StateType.Coyote:
                state = new Coyote(Player);
                break;
            case StateType.Falling:
                state = new Falling(Player);
                break;
            case StateType.Climbing:
                state = new Climbing(Player);
                break;
            case StateType.StandClimbing:
                state = new StandClimbing(Player);
                break;
            case StateType.Jumping:
                break;
            case StateType.Landed:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(stateType), stateType, null);
        }

        if (state != null)
            state.Finished += OnStateFinished;
        
        return state;
    }

    public override void _Ready()
    {
        _currentState = GetState(InitialState);
    }

    public override void _Process(double delta)
    {
        _currentState?.Process(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        _currentState?.PhysicsProcess(delta);
    }

    private void OnStateFinished(HumanoidState state, StateType newStateType)
    {
        if (state != _currentState)
            return;

        HumanoidState newState = GetState(newStateType);

        if (newState is null)
            return;

        _currentState?.OnExit();

        newState.OnEnter();

        _currentState = newState;
    }
}