
using System.Collections.Generic;
using Jomolith.Editor.Models;
using static Jomolith.Editor.Models.SceneModel;

namespace Jomolith.Editor.Commands;

public class DeleteObjectsCommand(SceneModel scene, int[] ids) 
    : ICommand
{
    private readonly List<ObjectSnapshot> _snapshots = [];

    public void Execute()
    {
        // Filter our IDs to only the roots of each subtree.
        HashSet<int> deleteSet = new(ids);
        List<int> rootIds = [];

        foreach (int id in deleteSet)
        {
            int? current = scene.GetObject(id).ParentId;
            bool hasDeletedAncestor = false;

            while (current is not null)
            {
                if (deleteSet.Contains(current.Value))
                {
                    hasDeletedAncestor = true;
                    break;
                }

                current = scene.GetObject(current.Value).ParentId;
            }
            
            if (!hasDeletedAncestor)
                rootIds.Add(id);
        }

        foreach (int id in rootIds)
            scene.CollectSubtreeSnapshots(id, _snapshots);
        
        foreach (int id in rootIds)
            scene.DeleteObjectInternal(id);
    }

    public void Undo()
    {
        foreach (ObjectSnapshot snapshot in _snapshots)
        {
            scene.RestoreObjectFromSnapshot(snapshot);
        }
    }
}