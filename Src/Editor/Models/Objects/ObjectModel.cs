using Godot;

namespace Jomolith.Editor.Models.Objects;

public partial class ObjectModel : RefCounted
{
    public int Id;
    public int? ParentId;
    public Transform3D Transform;
    
    public ObjectType Shape;
    public SurfaceData SurfaceData;
    
    // Used for meshes only
    public string? ResourcePath;

    public ObjectModel(int id, int? parentId, Transform3D transform, ObjectType shape, SurfaceData surfaceData, string? resourcePath)
    {
        Id = id;
        ParentId = parentId;
        Transform = transform;
        Shape = shape;
        SurfaceData = surfaceData;
        ResourcePath = resourcePath;
    }
    
    public ObjectModel()
    {
    }
}