using Godot;
using Jomolith.Editor.Commands;
using Jomolith.Editor.Models;

namespace Jomolith.Editor.Controllers;

public partial class MoveController : RefCounted
{
    private EditorState _state = null!;
    private CommandStack _commandStack = null!;

    public void Setup(EditorState state, CommandStack commandStack)
    {
        _state = state;
        _commandStack = commandStack;
    }

    public void OnDragStart(Vector3 hitPos)
    {
        
    }
    
    public void OnDragUpdate(Vector3 hitPos)
    {
        
    }
    
    public void OnDragEnd(Vector3 hitPos)
    {
        
    }
}