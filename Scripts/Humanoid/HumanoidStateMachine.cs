using System;
using Godot;
using Jomolith.Scripts.Humanoid.HumanoidStates;

namespace Jomolith.Scripts.Humanoid;

public partial class HumanoidStateMachine : Node
{
    [Export]
    public StateType InitialState { get; set; }
    
    [Export]
    public Humanoid Player { get; set; }

    public HumanoidState CurrentState { get; private set; }

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
                state = new Landed(Player);
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
        CurrentState = GetState(InitialState);
    }

    public override void _Process(double delta)
    {
        CurrentState?.Process(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        CurrentState?.PhysicsProcess(delta);
    }

    private void OnStateFinished(HumanoidState state, StateType newStateType)
    {
        if (state != CurrentState)
            return;

        HumanoidState newState = GetState(newStateType);

        if (newState is null)
            return;

        CurrentState?.OnExit();

        newState.OnEnter();

        CurrentState = newState;
    }
}