using Godot;
using Jomolith.Editor.Commands;
using Jomolith.Editor.Models;

namespace Jomolith.Editor.Controllers;

public partial class SelectionController : RefCounted
{
    private EditorState _state = null!;

    public void Setup(EditorState state)
    {
        _state = state;
    }

    public void OnClick(int id, bool additive)
    {
        
    }
}