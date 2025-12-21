
using System;
using System.Collections.Generic;
using Godot;

namespace Jomolith.Editor.Models;

public partial class SceneModel : RefCounted
{
    public Godot.Collections.Dictionary<int, ObjectModel> Objects { get; private set; } = [];
    private int _nextId = 1;

    public ObjectModel GetObject(int id) => Objects[id];

    public int CreateObject(string resourcePath, Transform3D transform, int? parentId = null)
    {
        if (parentId is not null && !Objects.ContainsKey(parentId.Value))
        {
            GD.PushError($"CreateObject: parentId {parentId} does not exist.");
            parentId = null;
        }

        int id = _nextId;
        _nextId++;

        Objects.Add(id, new ObjectModel(id, parentId, transform, resourcePath));

        return id;
    }

    public int[] GetChildren(int parentId)
    {
        List<int> children = [];

        foreach (int id in Objects.Keys)
        {
            if (Objects[id].ParentId == parentId)
            {
                children.Add(id);
            }
        }

        return children.ToArray();
    }

    public void SetParent(int id, int? newParentId)
    {
        if (newParentId is not null && !Objects.ContainsKey(newParentId.Value))
        {
            GD.PushError($"SetParent: parentId {newParentId} does not exist.");
            return;
        }
        
        // Prevent cyclical parenting
        int? currentId = newParentId;

        while (currentId is not null)
        {
            if (currentId == id)
            {
                GD.PushError($"SetParent: Cycle detected in parenting {newParentId} to {id}.");
                return;
            }

            currentId = Objects[currentId.Value].ParentId;
        }

        Objects[id].ParentId = newParentId;
    }

    public void DeleteObjectInternal(int id)
    {
        foreach (int childId in GetChildren(id))
        {
            DeleteObjectInternal(childId);
        }

        Objects.Remove(id);
    }

    public void CollectSubtreeSnapshots(int rootId, List<ObjectSnapshot> outSnapshots)
    {
        var obj = Objects[rootId];
        
        outSnapshots.Add(new ObjectSnapshot
        {
            Id = obj.Id,
            ParentId = obj.ParentId,
            Properties = obj.Properties,
            ResourcePath = obj.ResourcePath,
            Transform = obj.Transform
        });

        foreach (int childId in GetChildren(rootId))
        {
            CollectSubtreeSnapshots(childId, outSnapshots);
        }
    }

    public void RestoreObjectFromSnapshot(ObjectSnapshot snapshot)
    {
        ObjectModel obj = new ObjectModel
        {
            Id = snapshot.Id,
            ParentId = snapshot.ParentId,
            Transform = snapshot.Transform,
            ResourcePath = snapshot.ResourcePath,
            Properties = snapshot.Properties
        };
        
        Objects.Add(obj.Id, obj);

        // Ensure next ID is stays further than every existing object
        _nextId = Math.Max(_nextId, obj.Id + 1);
    }
        
    public struct ObjectSnapshot
    {
        public int Id;
        public int? ParentId;
        public Transform3D Transform;
        public string ResourcePath;
        public Dictionary<string, Variant> Properties;
    }
}