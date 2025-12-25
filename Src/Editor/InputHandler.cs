
using Godot;

namespace Jomolith.Editor;

public partial class InputHandler : Node
{
    private EditorContext _context = null!;

    private Vector2 _mousePosForRaycast;
    private bool _mouseDownPending;
    private bool _dragPending;
    private bool _dragging;

    public void Setup(EditorContext context)
    {
        _context = context;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed)
        {
            if (key.Keycode == Key.Z && key.CtrlPressed)
            {
                if (key.ShiftPressed)
                    _context.CommandStack.Redo();
                else
                    _context.CommandStack.Undo();
            }
            else if (key.Keycode == Key.Y && key.CtrlPressed)
            {
                _context.CommandStack.Redo();
            }
            else if (key.Keycode == Key.Delete)
            {
                _context.DeleteController.OnDeletePressed();
            }
            else if (key.Keycode == Key.C)
            {
                _context.CreateController.OnCreatePressed();
            }
        }
        else if (@event is InputEventMouseButton mbe)
        {
            if (mbe.Pressed)
            {
                _mousePosForRaycast = _context.EditorViewport.GetGlobalMousePosition();
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
        int? hitId = _context.ViewportService.RaycastObject(_mousePosForRaycast, out Vector3? hitPos);
    
        if (hitPos is not null)
        {
            if (_mouseDownPending)
            {
                if (hitId is not null)
                {
                    _context.SelectionController.OnClick(hitId.Value, Input.IsKeyPressed(Key.Ctrl));
                    _context.MoveController.OnDragStart(hitPos.Value);
                    _dragging = true;
                }
            
                _mouseDownPending = false;
            }
            if (_dragPending && _dragging)
            {
                _context.MoveController.OnDragUpdate(hitPos.Value);
            }
            else if (_dragging)
            {
                _context.MoveController.OnDragEnd(hitPos.Value);
                _dragging = false;
            }
        }
    }
}