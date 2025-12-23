using System.Collections.Generic;
using Godot;
using Jomolith.Editor.Models;
using Jomolith.Editor.Models.Objects;
using Mesh = Godot.Mesh;

namespace Jomolith.Editor.Views;

public partial class SceneView : Node3D
{
    private Dictionary<int, ViewMesh> _idToObjectsDict = [];
    private Dictionary<int, PickProxy> _pickProxyToObjectsDict = [];

    public void ConnectToModel(SceneModel model)
    {
        model.ObjectAdded += OnObjectAdded;
        model.ObjectRemoved += OnObjectRemoved;
        model.ObjectTransformed += OnObjectTransformed;
    }

    private void OnObjectAdded(ObjectModel obj)
    {
        // Mesh and collision are created separately to allow for future mesh batching.
        ViewMesh viewMesh = CreateViewMesh(obj);
        PickProxy pickProxy = CreatePickProxy(obj);
        
        AddChild(viewMesh);
        AddChild(pickProxy);
        
        _idToObjectsDict.Add(obj.Id, viewMesh);
        _pickProxyToObjectsDict.Add(obj.Id, pickProxy);
    }

    private void OnObjectRemoved(int id)
    {
        ViewMesh mesh = _idToObjectsDict[id];
        PickProxy pickProxy = _pickProxyToObjectsDict[id];
        
        mesh.QueueFree();
        pickProxy.QueueFree();

        _idToObjectsDict.Remove(id);
        _pickProxyToObjectsDict.Remove(id);
    }

    private void OnObjectTransformed(int id, Vector3? position, Quaternion? rotation, ObjectDimensions? dimensions)
    {
        ViewMesh mesh = _idToObjectsDict[id];
        PickProxy pickProxy = _pickProxyToObjectsDict[id];

        if (position is not null)
        {
            mesh.GlobalPosition = position.Value;
            pickProxy.GlobalPosition = position.Value;
        }

        if (rotation is not null)
        {
            mesh.GlobalRotation = rotation.Value.GetEuler();
            pickProxy.GlobalRotation = rotation.Value.GetEuler();
        }

        if (dimensions is not null)
        {
            mesh.Mesh = GetMesh(mesh.Type, dimensions.Value);
            pickProxy.Collider.Shape = GetShape3D(mesh.Type, dimensions.Value);
        }

        _idToObjectsDict[id] = mesh;
        _pickProxyToObjectsDict[id] = pickProxy;
    }

    private ViewMesh CreateViewMesh(ObjectModel obj)
    {
        ViewMesh viewMesh = new()
        {
            GlobalPosition = obj.Position,
            GlobalRotation = obj.Rotation.GetEuler(),
            Mesh = GetMesh(obj.Type, obj.Dimensions)
        };
        
        viewMesh.Type = ObjectType.Block;

        return viewMesh;
    }

    private Mesh GetMesh(ObjectType type, ObjectDimensions dimensions)
    {
        Mesh? mesh = null;

        if (type == ObjectType.Block)
        {
            mesh = new BoxMesh
            {
                Size = dimensions.Size
            };
        }

        return mesh ?? new Mesh();
    }

    private PickProxy CreatePickProxy(ObjectModel obj)
    {
        PickProxy pickProxy = new()
        {
            GlobalPosition = obj.Position,
            GlobalRotation = obj.Rotation.GetEuler(),
            ObjectId = obj.Id
        };

        CollisionShape3D shape = new()
        {
            Shape = GetShape3D(obj.Type, obj.Dimensions)
        };
        
        pickProxy.AddChild(shape);

        pickProxy.Collider = shape;
        pickProxy.Type = obj.Type;

        return pickProxy;
    }
    
    private Shape3D GetShape3D(ObjectType type, ObjectDimensions dimensions)
    {
        Shape3D? shape = null;

        if (type == ObjectType.Block)
        {
            shape = new BoxShape3D
            {
                Size = dimensions.Size
            };
        }

        return shape ?? new BoxShape3D();
    }
}