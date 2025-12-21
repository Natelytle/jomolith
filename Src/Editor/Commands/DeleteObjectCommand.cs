using System.Collections.Generic;
using Jomolith.Editor.Models;
using static Jomolith.Editor.Models.SceneModel;

namespace Jomolith.Editor.Commands;

// TODO: use EditorState and clear selection upon deletion
public class DeleteObjectCommand(SceneModel scene, int id) 
    : Command
{
    private SceneModel _scene = scene;
    private int _id = id;

    private List<ObjectSnapshot> _snapshots = [];

    public void Execute()
    {
        _scene.CollectSubtreeSnapshots(_id, _snapshots);
        
        _scene.DeleteObjectInternal(_id);
    }

    public void Undo()
    {
        foreach (ObjectSnapshot snapshot in _snapshots)
        {
            _scene.RestoreObjectFromSnapshot(snapshot);
        }
    }
}