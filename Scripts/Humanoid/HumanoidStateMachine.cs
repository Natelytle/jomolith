using System;
using Godot;
using Jomolith.Scripts.Humanoid.HumanoidStates;

namespace Jomolith.Scripts.Humanoid;

public partial class HumanoidStateMachine : Node
{
    [Export]
    public StateType InitialState { get; set; }
    
    [Export]
    public RigidHumanoid Player { get; set; }

    public HumanoidState CurrentState { get; private set; }
    private StateType _currentStateType;

    public enum StateType
    {
        None,
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
        HumanoidState state;
        
        switch (stateType)
        {
            case StateType.Running:
                state = new Running(Player, _currentStateType);
                break;
            case StateType.Coyote:
                state = new Coyote(Player, _currentStateType);
                break;
            case StateType.Falling:
                state = new Falling(Player, _currentStateType);
                break;
            case StateType.Climbing:
                state = new Climbing(Player, _currentStateType);
                break;
            case StateType.StandClimbing:
                state = new StandClimbing(Player, _currentStateType);
                break;
            case StateType.Jumping:
                state = new Jumping(Player, _currentStateType);
                break;
            case StateType.Landed:
                state = new Landed(Player, _currentStateType);
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
        CurrentState?.PrePhysicsProcess(delta);
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
        _currentStateType = newStateType;
    }
}