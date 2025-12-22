using Godot;

namespace Jomolith.Editor.Models;

public partial class EditorState : RefCounted
{
    public SceneModel Scene = null!;
    public SelectionModel Selection = null!;

    public void Setup(SceneModel scene, SelectionModel selection)
    {
        Scene = scene;
        Selection = selection;
    }
}