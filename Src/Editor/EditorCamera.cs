
using Godot;

namespace Jomolith.Editor;

public partial class EditorCamera : Node3D
{
    [Export] private float _sensitivity;
    [Export] private Camera3D _camera = null!;

    private float _moveSpeed = 20;
    private const float MinSpeed = 2;
    private const float MaxSpeed = 500;

    private bool _cameraRotate;

    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionPressed("right_click"))
        {
            if (Input.GetMouseMode() is not Input.MouseModeEnum.Captured)
            {
                Input.SetMouseMode(Input.MouseModeEnum.Captured);
            }

            _cameraRotate = true;
        }
        else
        {
            if (Input.GetMouseMode() is not Input.MouseModeEnum.Visible)
            {
                Input.SetMouseMode(Input.MouseModeEnum.Visible);
            }

            _cameraRotate = false;
        }

        ProcessMovement(delta);
    }

    private void ProcessMovement(double delta)
    {
        Vector2 movementVector = Input.GetVector("left", "right", "forward", "backward");

        GlobalPosition += _camera.GlobalBasis.Z * movementVector.Y * _moveSpeed * (float)delta;
        GlobalPosition += _camera.GlobalBasis.X * movementVector.X * _moveSpeed * (float)delta;
    }

    public override void _UnhandledInput(InputEvent e)
    { 
        if (_cameraRotate && e is InputEventMouseMotion motion)
        {
            Vector3 cameraRotation = _camera.RotationDegrees;
            cameraRotation.X = float.Clamp(cameraRotation.X + -motion.Relative.Y * _sensitivity, -80, 80);
            _camera.RotationDegrees = cameraRotation;

            SetRotationDegrees(new Vector3(RotationDegrees.X, RotationDegrees.Y - motion.Relative.X * _sensitivity, RotationDegrees.Z));
        }

        if (e is InputEventMouseButton mbEvent)
        {
            if (mbEvent.ButtonIndex == MouseButton.WheelUp) {
                _moveSpeed *= 1.1f;
                _moveSpeed = float.Min(_moveSpeed, MaxSpeed);
            } else if (mbEvent.ButtonIndex == MouseButton.WheelDown) {
                _moveSpeed /= 1.1f;
                _moveSpeed = float.Max(_moveSpeed, MinSpeed);
            }
        }
    }
}