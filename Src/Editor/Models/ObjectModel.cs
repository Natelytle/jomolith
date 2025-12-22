
using Godot;
using System.Collections.Generic;

namespace Jomolith.Editor.Models;

public partial class ObjectModel : RefCounted
{
    public int Id;
    public int? ParentId;
    public Transform3D Transform;
    public Vector3 Scale;
    public string ResourcePath = null!;
    public Dictionary<string, Variant> Properties = [];

    public ObjectModel(int id, int? parentId, Transform3D transform, Vector3 scale, string resourcePath)
    {
        Id = id;
        ParentId = parentId;
        Transform = transform;
        Scale = scale;
        ResourcePath = resourcePath;
    }
    
    public ObjectModel()
    {
    }
}