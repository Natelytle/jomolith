using System;
using Godot;

namespace Jomolith.Scripts;

public partial class HumanoidCamera : Node
{
    private const float MinDistance = 0.5f;
    private const float MaxDistance = 400.0f;
    private const float FirstPersonThreshold = 1.0f;
    private const float ShiftLockOffset = 1.75f;

    [Export] public float MouseSensitivity { get; set; } = 0.68f;
    private Vector2 _mousePixelsToUnits = new(0.002f * float.Pi, 0.0015f * float.Pi);

    [Export] public KineticHumanoid.KineticHumanoid Subject { get; set; }

    private bool _firstPerson;
    private bool _shiftLock;
    public bool CameraLocked => _firstPerson || _shiftLock;
    public Vector3 Rotation => CameraAnchor.Rotation;

    private bool _rightClick;

    private Node3D CameraAnchor => (Node3D)GetNode("CameraAnchor");
    private SpringArm3D CameraSpringArm => (SpringArm3D)GetNode("CameraAnchor/CameraSpringarm");
    private Camera3D Camera => (Camera3D)GetNode("CameraAnchor/CameraSpringarm/Camera3D");
    
    public HumanoidCamera()
    {
        CameraSpringArm.SpringLength = _currentDistance;

        Camera.Current = true;
        Camera.Fov = 70;
    }
    
    private float _currentDistance = 12.5f;
    private float _horizontalOffset;

    public override void _Process(double delta)
    {
        // Move the camera to the head of our subject.
        if (Subject is not null)
        {
            CameraAnchor.GlobalPosition = ((Node3D)Subject.GetNode("HumanoidCameraPosition")).GlobalPosition;
        }

        CameraSpringArm.SpringLength = Single.Min((CameraAnchor.GlobalPosition - Camera.GlobalPosition).Length(),
            CameraSpringArm.SpringLength);

        // Update spring arm length
        float amount = 1 - float.Pow(0.5f, (float)delta * 30);
        CameraSpringArm.SpringLength = float.Lerp(CameraSpringArm.SpringLength, _currentDistance, amount);
    }
    
    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsAction("zoom_in") && e.IsPressed())
        {
            float newDistance = _currentDistance - (1 + _currentDistance * 0.5f);

            SetCameraToDistance(newDistance);
        }

        if (e.IsAction("zoom_out") && e.IsPressed())
        {
            float newDistance;

            if (_firstPerson)
                newDistance = FirstPersonThreshold;
            else
                newDistance = _currentDistance + (1 + _currentDistance * 0.5f);
            
            SetCameraToDistance(newDistance);
        }
        
        if (e.IsActionPressed("shift_lock") && e.IsPressed())
        {
            ToggleShiftLock();
        }

        if (e.IsActionPressed("right_click") && e.IsPressed())
        {
            _rightClick = true;
            UpdateMouseBehaviour();
        }

        if (e.IsActionReleased("right_click"))
        {
            _rightClick = false;
            UpdateMouseBehaviour();
        }
        
        if (Input.MouseMode is Input.MouseModeEnum.Captured && e is InputEventMouseMotion motion)
        {
            Move(motion);
        }
    }

    private void SetCameraToDistance(float newDistance)
    {
        newDistance = Math.Clamp(newDistance, MinDistance, MaxDistance);

        if (newDistance < FirstPersonThreshold)
        {
            _currentDistance = 0f;
            EnterFirstPerson();
        }
        else
        {
            _currentDistance = newDistance;
            
            if (_firstPerson)
                ExitFirstPerson();
        }
    }

    private void UpdateSpringLength()
    {
        
    }

    private void EnterFirstPerson()
    {
        _firstPerson = true;
        
        // Reset shift lock camera
        _horizontalOffset = 0f;
        
        UpdateMouseBehaviour();
    }

    private void ExitFirstPerson()
    {
        _firstPerson = false;

        if (_shiftLock)
            _horizontalOffset = ShiftLockOffset;
        
        UpdateMouseBehaviour();
    }

    private void ToggleShiftLock()
    {
        _shiftLock = !_shiftLock;

        if (_shiftLock && !_firstPerson)
            _horizontalOffset = ShiftLockOffset;
        else
            _horizontalOffset = 0f;
        
        UpdateMouseBehaviour();
    }

    private void UpdateMouseBehaviour()
    {
        if (_shiftLock || _firstPerson || _rightClick)
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
        else
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
    }

    private void Move(InputEventMouseMotion motion)
    {
        Vector2 moveVector = motion.Relative * _mousePixelsToUnits * MouseSensitivity;

        float yRotation = CameraAnchor.Rotation.Y - moveVector.X;
        float xRotation = CameraAnchor.Rotation.X - moveVector.Y;

        // Clamp to 80 degrees.
        xRotation = Math.Clamp(xRotation, float.DegreesToRadians(-80), float.DegreesToRadians(80));
        
        CameraAnchor.SetRotation(new Vector3(xRotation, yRotation, 0f));
    }
}