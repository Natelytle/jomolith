using Godot;

namespace Jomolith.Editor.Models;

public partial class EditorState : RefCounted
{
    public SceneModel Scene = null!;
    private SelectionModel Selection = null!;
}