using Godot;
using Godot.Collections;

namespace Jomolith.Scripts.Editor;

public partial class EditorCameraController : Node3D
{
    public event ObjectSelectedHandler? OnObjectSelected;
    public delegate void ObjectSelectedHandler(Node3D? selectedNode);

    private const float MovementSpeed = 20;
    private const float ShiftSpeed = 40;
    private const float CtrlSpeed = 5;
    
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

        if (_editorController.IsInFreelookMode)
            ProcessMovement(delta);
    }

    private void ProcessMovement(double delta)
    {
        Vector2 movementVector = Vector2.Zero;
        float movementSpeed = MovementSpeed;

        movementVector = Input.GetVector("left", "right", "forward", "backward");

        if (Input.IsActionPressed("shift_lock"))
        {
            movementSpeed = ShiftSpeed;
        }
        else if (Input.IsKeyPressed(Key.Ctrl))
        {
            movementSpeed = CtrlSpeed;
        }

        GlobalPosition += _camera.GlobalBasis.Z * movementVector.Y * movementSpeed * (float)delta;
        GlobalPosition += _camera.GlobalBasis.X * movementVector.X * movementSpeed * (float)delta;
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (_editorController.IsInFreelookMode)
        {
            if (e is InputEventMouseMotion motion)
            {
                Vector3 cameraRotation = _camera.RotationDegrees;
                cameraRotation.X = float.Clamp(cameraRotation.X + -motion.Relative.Y, -80, 80);
                _camera.RotationDegrees = cameraRotation;

                SetRotationDegrees(new Vector3(RotationDegrees.X, RotationDegrees.Y - motion.Relative.X, RotationDegrees.Z));
            }
        }
        else if (e is InputEventMouseButton click)
        {
            if (click.ButtonIndex == MouseButton.Left && click.Pressed)
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
            Vector3 raycastTo = raycastFrom + _camera.ProjectRayNormal(GetTree().Root.GetMousePosition()) * 100;
            
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