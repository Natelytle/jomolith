using Godot;
using Jomolith.Editor.Commands;
using Jomolith.Editor.Controllers;

namespace Jomolith.Editor;

public partial class InputHandler : Node
{
    [Export] private Camera3D _camera = null!;
    private MoveController _moveController = null!;
    private SelectionController _selectionController = null!;
    private DeleteController _deleteController = null!;
    private CommandStack _commandStack = null!;

    private Vector2 _mousePosForRaycast;
    private bool _mouseDownPending;
    private bool _dragPending;
    private bool _dragging;

    public void Setup(MoveController moveController, SelectionController selectionController, DeleteController deleteController, CommandStack commandStack)
    {
        _moveController = moveController;
        _selectionController = selectionController;
        _deleteController = deleteController;
        _commandStack = commandStack;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed)
        {
            if (key.Keycode == Key.Z && key.CtrlPressed)
            {
                if (key.ShiftPressed)
                    _commandStack.Redo();
                else
                    _commandStack.Undo();
            }
            else if (key.Keycode == Key.Y && key.CtrlPressed)
            {
                _commandStack.Redo();
            }
            else if (key.Keycode == Key.Delete)
            {
                _deleteController.OnDeletePressed();
            }
        }
        else if (@event is InputEventMouseButton mbe)
        {
            if (mbe.Pressed)
            {
                _mousePosForRaycast = _camera.GetViewport().GetMousePosition();
                _mouseDownPending = true;
                _dragPending = true;
            }
            else
            {
                _mouseDownPending = false;
                _dragPending = false;
            }
        }
        else if (@event is InputEventMouseButton motion)
        {
            _mousePosForRaycast = motion.Position;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        int? hitId = RaycastFromMouse(_mousePosForRaycast, out Vector3? hitPos);

        if (hitPos is not null)
        {
            if (_mouseDownPending)
            {
                if (hitId is not null)
                {
                    _selectionController.OnClick(hitId.Value, Input.IsKeyPressed(Key.Ctrl));
                    _moveController.OnDragStart(hitPos.Value);
                    _dragging = true;
                }
            
                _mouseDownPending = false;
            }
            if (_dragPending && _dragging)
            {
                _moveController.OnDragUpdate(hitPos.Value);
            }
            else if (_dragging)
            {
                _moveController.OnDragEnd(hitPos.Value);
                _dragging = false;
            }
        }
    }
    
    private int? RaycastFromMouse(Vector2 mousePosForRaycast, out Vector3? hitPos)
    {
        Vector3 from = _camera.ProjectRayOrigin(mousePosForRaycast);
        Vector3 to = from + _camera.ProjectRayNormal(mousePosForRaycast) * 1000f;

        var spaceState = _camera.GetWorld3D().DirectSpaceState;
        
        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(from, to, EditorMain.ObjectCollisionMask);
        query.CollideWithAreas = false;
        query.CollideWithBodies = true;

        var result = spaceState.IntersectRay(query);

        if (result.Count > 0)
        {
            hitPos = (Vector3)result["position"];
            Node3D collider = (Node3D)result["collider"];
            int objectId = collider.Get("ObjectId").AsInt32();
            return objectId;
        }

        hitPos = null;
        return null;
    }
}