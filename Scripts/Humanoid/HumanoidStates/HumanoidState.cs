
using System;
using Godot;
using static Jomolith.Scripts.Humanoid.HumanoidStateMachine;

namespace Jomolith.Scripts.Humanoid.HumanoidStates;

public abstract class HumanoidState(string stateName, RigidHumanoid player, StateType priorState)
{
    public readonly string StateName = stateName;
    protected readonly RigidHumanoid Player = player;

    protected double Timer { get; set; }

    public abstract void OnEnter();
    public abstract void OnExit();
    public abstract void Process(double delta);
    public abstract void PhysicsProcess(double delta);

    public void PrePhysicsProcess(double delta)
    {
        Timer -= delta;
    }

    protected void InvokeFinished(HumanoidState state, StateType stateType)
    {
        Finished?.Invoke(state, stateType);
    }

    public event FinishedEventHandler Finished;
    public delegate void FinishedEventHandler(HumanoidState state, StateType stateType);
    
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
            case EventType.OnFloor: returnValue = Player.HasFloor() && Player.LinearVelocity.Y <= 0; break;
            case EventType.OffFloor: returnValue = !Player.HasFloor(); break;
            case EventType.TimerUp: returnValue = Timer <= 0; break;
            default:
                throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
        }

        return returnValue;
    }
}