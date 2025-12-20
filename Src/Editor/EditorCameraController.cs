using Godot;
using Godot.Collections;

namespace Jomolith.Editor;

public partial class EditorCameraController : Node3D
{
    [Export] private float Sensitivity { get; set; } = 0.1f;

    public event ObjectSelectedHandler? OnObjectSelected;
    public delegate void ObjectSelectedHandler(Node3D? selectedNode);

    private float _moveSpeed = 20;
    private const float MinSpeed = 2;
    private const float MaxSpeed = 500;
    
    private bool _sendRaycast;
    
    private EditorController _editorController;
    private Camera3D _camera;

    public override void _Ready()
    {
        _editorController = (EditorController)GetParent();
        _camera = (Camera3D)GetNode("Camera3D");
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionPressed("right_click"))
        {
            if (Input.GetMouseMode() is not Input.MouseModeEnum.Captured)
            {
                Input.SetMouseMode(Input.MouseModeEnum.Captured);
            }

            _editorController.IsInFreelookMode = true;
        }
        else
        {
            if (Input.GetMouseMode() is not Input.MouseModeEnum.Visible)
            {
                Input.SetMouseMode(Input.MouseModeEnum.Visible);
            }

            _editorController.IsInFreelookMode = false;
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
        if (_editorController.IsInFreelookMode)
        {
            if (e is InputEventMouseMotion motion)
            {
                Vector3 cameraRotation = _camera.RotationDegrees;
                cameraRotation.X = float.Clamp(cameraRotation.X + -motion.Relative.Y * Sensitivity, -80, 80);
                _camera.RotationDegrees = cameraRotation;

                SetRotationDegrees(new Vector3(RotationDegrees.X, RotationDegrees.Y - motion.Relative.X * Sensitivity, RotationDegrees.Z));
            }
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

            if (mbEvent.ButtonIndex == MouseButton.Left && mbEvent.Pressed)
            {
                if (_editorController.CurrentEditorMode == EditorController.EditorMode.Select)
                {
                    _sendRaycast = true;
                }
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_sendRaycast)
        {
            _sendRaycast = false;

            Node3D? selectedNode = null;

            PhysicsDirectSpaceState3D spaceState3D = GetWorld3D().DirectSpaceState;

            Vector3 raycastFrom = _camera.ProjectRayOrigin(GetTree().Root.GetMousePosition());
            Vector3 raycastTo = raycastFrom + _camera.ProjectRayNormal(GetTree().Root.GetMousePosition()) * 1000;
            
            PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(raycastFrom, raycastTo, 1);

            Dictionary? result = spaceState3D.IntersectRay(query);

            if (result.Count > 0)
            {
                selectedNode = (Node3D)result["collider"];
            }
            
            OnObjectSelected?.Invoke(selectedNode);
        }
    }
}