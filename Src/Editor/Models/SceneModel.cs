
using System;
using System.Collections.Generic;
using Godot;
using Jomolith.Editor.Models.Objects;

namespace Jomolith.Editor.Models;

public partial class SceneModel : RefCounted
{
    [Signal] public delegate void ObjectAddedEventHandler(int id);
    [Signal] public delegate void ObjectRemovedEventHandler(int id);
    [Signal] public delegate void ObjectTransformedEventHandler(int id, Transform3D transform);
    [Signal] public delegate void ObjectParentChangedEventHandler(int id, int newParentId);

    private Godot.Collections.Dictionary<int, ObjectModel> Objects { get; set; } = [];
    private int _nextId = 1;

    public ObjectModel GetObject(int id) => Objects[id];

    public int CreateObject(Transform3D transform, ObjectType shape, SurfaceData surfaceData, int? parentId = null, string? resourcePath = null)
    {
        if (parentId is not null && !Objects.ContainsKey(parentId.Value))
        {
            GD.PushError($"CreateObject: parentId {parentId} does not exist.");
            parentId = null;
        }

        int id = _nextId;
        _nextId++;

        Objects.Add(id, new ObjectModel(id, parentId, transform, shape, surfaceData, resourcePath));

        EmitSignal(SignalName.ObjectAdded, id);

        return id;
    }

    public void TranslateObject(int id, Vector3 delta)
    {
        Objects[id].Transform.Origin += delta;

        EmitSignal(SignalName.ObjectTransformed, id, Objects[id].Transform);
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

        EmitSignal(SignalName.ObjectParentChanged, id, newParentId ?? -1);
    }
    
    public void DeleteObjectInternal(int id)
    {
        foreach (int childId in GetChildren(id))
        {
            DeleteObjectInternal(childId);
        }

        Objects.Remove(id);

        EmitSignal(SignalName.ObjectRemoved, id);
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

    public void CollectSubtreeSnapshots(int rootId, List<ObjectSnapshot> outSnapshots)
    {
        var obj = Objects[rootId];
        
        outSnapshots.Add(new ObjectSnapshot
        {
            Id = obj.Id,
            ParentId = obj.ParentId,
            Transform = obj.Transform,
            Shape = obj.Shape,
            SurfaceData = obj.SurfaceData,
            ResourcePath = obj.ResourcePath,
        });

        foreach (int childId in GetChildren(rootId))
        {
            CollectSubtreeSnapshots(childId, outSnapshots);
        }
    }

    public void RestoreObjectFromSnapshot(ObjectSnapshot snapshot)
    {
        ObjectModel obj = new()
        {
            Id = snapshot.Id,
            ParentId = snapshot.ParentId,
            Transform = snapshot.Transform,
            Shape = snapshot.Shape,
            SurfaceData = snapshot.SurfaceData,
            ResourcePath = snapshot.ResourcePath,
        };
        
        Objects.Add(obj.Id, obj);

        EmitSignal(SignalName.ObjectAdded, obj.Id);

        // Ensure next ID is stays further than every existing object
        _nextId = Math.Max(_nextId, obj.Id + 1);
    }
}