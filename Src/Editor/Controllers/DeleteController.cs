using Godot;
using Jomolith.Editor.Commands;
using Jomolith.Editor.Models;

namespace Jomolith.Editor.Controllers;

public partial class DeleteController : RefCounted
{
    private EditorState _state = null!;
    private CommandStack _commandStack = null!;

    public void Setup(EditorState state, CommandStack commandStack)
    {
        _state = state;
        _commandStack = commandStack;
    }

    public void OnDeletePressed()
    {
        var ids = _state.Selection.SelectedIds;
        if (ids.Count == 0) return;
        
        _commandStack.Execute(new DeleteObjectsCommand(_state.Scene, ids.ToArray()));
        _state.Selection.Clear();
    }
}