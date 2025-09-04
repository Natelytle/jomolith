
using System;
using Godot;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public abstract class HumanoidState(string stateName, Humanoid player)
{
    public readonly string StateName = stateName;
    protected readonly Humanoid Player = player;

    protected double Timer { get; set; }

    public abstract void OnEnter();
    public abstract void OnExit();
    public abstract void Process(double delta);
    public abstract void PhysicsProcess(double delta);

    protected void InvokeFinished(HumanoidState state, HumanoidStateMachine.StateType stateType)
    {
        Finished?.Invoke(state, stateType);
    }

    public event FinishedEventHandler Finished;
    public delegate void FinishedEventHandler(HumanoidState state, HumanoidStateMachine.StateType stateType);
    
    protected enum EventType 
    {
        JumpCommand,
        Tipped,
        Upright,
        FacingLadder,
        AwayLadder,
        OnFloor,
        OffFloor,
        TimerUp,
    }

    protected bool ComputeEvent(EventType eventType)
    {
        bool returnValue = false;

        switch (eventType)
        {
            case EventType.JumpCommand: returnValue = Input.IsActionPressed("jump"); break;
            case EventType.Tipped: break;
            case EventType.Upright: break;
            case EventType.FacingLadder: returnValue = Player.FacingLadder(); break;
            case EventType.AwayLadder: returnValue = !Player.FacingLadder(); break;
            case EventType.OnFloor: returnValue = Player.GetFloorDistance() < Humanoid.HipHeight + 1 && Player.LinearVelocity.Y <= 0; break;
            case EventType.OffFloor: returnValue = Player.GetFloorDistance() > Humanoid.HipHeight + 0.05; break;
            case EventType.TimerUp: returnValue = Timer <= 0; break;
            default:
                throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
        }

        return returnValue;
    }
}