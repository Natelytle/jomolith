using Godot;

namespace Jomolith.Editor.Models.Objects;

public partial class ObjectModel : RefCounted
{
    public int Id;
    public int? ParentId;
    
    public ObjectType Type;
    public Vector3 Position;
    public Quaternion Rotation;
    public ObjectDimensions Dimensions;
    public SurfaceData SurfaceData;
    
    // Used for meshes only
    public string? ResourcePath;

    public ObjectModel(int id, int? parentId, ObjectType type, Vector3 position, Quaternion rotation, ObjectDimensions dimensions, SurfaceData surfaceData, string? resourcePath)
    {
        Id = id;
        ParentId = parentId;
        Type = type;
        Position = position;
        Rotation = rotation;
        Dimensions = dimensions;
        SurfaceData = surfaceData;
        ResourcePath = resourcePath;
    }
    
    public ObjectModel()
    {
    }
}