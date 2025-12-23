
using System;
using System.Collections.Generic;
using Godot;
using Jomolith.Editor.Models.Objects;

namespace Jomolith.Editor.Models;

public partial class SceneModel : RefCounted
{
    public event ObjectAddedEventHandler? ObjectAdded;
    public delegate void ObjectAddedEventHandler(ObjectModel obj);

    public event ObjectRemovedEventHandler? ObjectRemoved;
    public delegate void ObjectRemovedEventHandler(int id);

    public event ObjectTransformedEventHandler? ObjectTransformed;
    public delegate void ObjectTransformedEventHandler(int id, Vector3? newPosition = null, Quaternion? newRotation = null, ObjectDimensions? newDimensions = null);

    public event ObjectParentChangedEventHandler? ObjectParentChanged;
    public delegate void ObjectParentChangedEventHandler(int id, int? newParentId);

    private Godot.Collections.Dictionary<int, ObjectModel> Objects { get; set; } = [];
    private int _nextId = 1;

    public ObjectModel GetObject(int id) => Objects[id];

    public int CreateObject(ObjectType shape, Vector3 position, Quaternion rotation, ObjectDimensions dimensions, SurfaceData surfaceData, int? parentId = null, string? resourcePath = null)
    {
        if (parentId is not null && !Objects.ContainsKey(parentId.Value))
        {
            GD.PushError($"CreateObject: parentId {parentId} does not exist.");
            parentId = null;
        }

        int id = _nextId;
        _nextId++;

        ObjectModel obj = new(id, parentId, shape, position, rotation, dimensions, surfaceData, resourcePath);
        Objects.Add(id, obj);

        ObjectAdded?.Invoke(obj);

        return id;
    }

    public void TranslateObject(int id, Vector3 delta)
    {
        Objects[id].Position += delta;

        ObjectTransformed?.Invoke(id, newPosition: Objects[id].Position);
    }
    
    public void RotateObject(int id, Quaternion delta)
    {
        ObjectModel obj = Objects[id];

        obj.Rotation = (delta * obj.Rotation).Normalized();

        ObjectTransformed?.Invoke(id, newRotation: Objects[id].Rotation);
    }

    public void ScaleObject(int id, ObjectDimensions delta)
    {
        ObjectModel obj = Objects[id];

        obj.Dimensions = new ObjectDimensions
        {
            Radius = obj.Dimensions.Radius + delta.Radius,
            Height = obj.Dimensions.Height + delta.Height,
            Size = obj.Dimensions.Size + delta.Size
        };
        
        ObjectTransformed?.Invoke(id, newDimensions: Objects[id].Dimensions);
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

        ObjectParentChanged?.Invoke(id, newParentId);
    }
    
    public void DeleteObjectInternal(int id)
    {
        foreach (int childId in GetChildren(id))
        {
            DeleteObjectInternal(childId);
        }

        Objects.Remove(id);

        ObjectRemoved?.Invoke(id);
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
            Shape = obj.Type,
            Position = obj.Position,
            Rotation = obj.Rotation,
            Dimensions = obj.Dimensions,
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
            Type = snapshot.Shape,
            Position = snapshot.Position,
            Rotation = snapshot.Rotation,
            Dimensions = snapshot.Dimensions,
            SurfaceData = snapshot.SurfaceData,
            ResourcePath = snapshot.ResourcePath,
        };
        
        Objects.Add(obj.Id, obj);

        ObjectAdded?.Invoke(obj);

        // Ensure next ID is stays further than every existing object
        _nextId = Math.Max(_nextId, obj.Id + 1);
    }
}