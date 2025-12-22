using Godot;
using Jomolith.Editor.Models;

namespace Jomolith.Editor.Views;

public partial class SceneView : Node3D
{
    public void ConnectToModel(SceneModel model)
    {
        model.ObjectAdded += OnObjectAdded;
        model.ObjectRemoved += OnObjectRemoved;
        model.ObjectTransformed += OnObjectTransformed;
        model.ObjectParentChanged += OnObjectParentChanged;
    }

    private void OnObjectAdded(int id)
    {
        
    }

    private void OnObjectRemoved(int id)
    {
        
    }

    private void OnObjectTransformed(int id, Transform3D transform)
    {
        
    }

    private void OnObjectParentChanged(int id, int newParentId)
    {
        
    }
}