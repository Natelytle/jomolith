using Godot;

namespace Jomolith.Editor.Controllers;

public partial class CameraController : Camera3D
{
    [Export] public float Sensitivity { get; set; } = 0.1f;

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

        GlobalPosition += GlobalBasis.Z * movementVector.Y * _moveSpeed * (float)delta;
        GlobalPosition += GlobalBasis.X * movementVector.X * _moveSpeed * (float)delta;
    }

    public override void _UnhandledInput(InputEvent e)
    { 
        if (_cameraRotate && e is InputEventMouseMotion motion)
        {
            Vector3 cameraRotation = RotationDegrees;
            cameraRotation.X = float.Clamp(cameraRotation.X + -motion.Relative.Y * Sensitivity, -80, 80);
            RotationDegrees = cameraRotation;

            SetRotationDegrees(new Vector3(RotationDegrees.X, RotationDegrees.Y - motion.Relative.X * Sensitivity, RotationDegrees.Z));
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