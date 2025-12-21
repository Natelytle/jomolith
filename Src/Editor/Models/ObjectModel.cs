
using Godot;
using System.Collections.Generic;

namespace Jomolith.Editor.Models;

public partial class ObjectModel : RefCounted
{
    public int Id;
    public int? ParentId;
    public Transform3D Transform;
    public string ResourcePath = null!;
    public Dictionary<string, Variant> Properties = [];

    public ObjectModel(int id, int? parentId, Transform3D transform, string resourcePath)
    {
        Id = id;
        ParentId = parentId;
        Transform = transform;
        ResourcePath = resourcePath;
    }
    
    public ObjectModel()
    {
    }
}