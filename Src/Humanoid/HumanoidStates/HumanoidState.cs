using System;
using Godot;
using static Jomolith.Src.Humanoid.Humanoid;

namespace Jomolith.Src.Humanoid.HumanoidStates;

public abstract class HumanoidState(string stateName, Humanoid player, StateType priorState)
{
    public readonly string StateName = stateName;
    protected readonly Humanoid Player = player;

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

    public event FinishedEventHandler? Finished;
    public delegate void FinishedEventHandler(HumanoidState state, StateType stateType);
    
    protected enum EventType
    {
        JumpCommand,
        Tipped,
        Upright,
        FacingLadder,
        AwayLadder,
        OnFloor,
        InFloor,
        OffFloor,
        TimerUp,
        IsIdle
    }

    protected bool ComputeEvent(EventType eventType)
    {
        bool returnValue = false;

        switch (eventType)
        {
            case EventType.JumpCommand: returnValue = Input.IsActionPressed("jump"); break;
            case EventType.Tipped: break;
            case EventType.Upright: break;
            case EventType.FacingLadder: returnValue = Player.IsClimbing; break;
            case EventType.AwayLadder: returnValue = !Player.IsClimbing; break;
            case EventType.OnFloor: returnValue = Player.FloorPart is not null && Player.LinearVelocity.Dot(Player.FloorNormal ?? Vector3.Up) <= 0; break;
            case EventType.InFloor: returnValue = Player.FloorLocation is not null && Player.FloorLocation?.Y + 3 > Player.Position.Y; break;
            case EventType.OffFloor: returnValue = Player.FloorPart is null; break;
            case EventType.TimerUp: returnValue = Timer <= 0; break;
            case EventType.IsIdle: returnValue = (Player.LinearVelocity - Player.FloorVelocity ?? Vector3.Zero).Length() < 0.01; break;
            default:
                throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
        }

        return returnValue;
    }
}