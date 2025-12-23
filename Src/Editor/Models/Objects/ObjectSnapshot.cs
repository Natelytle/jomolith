using Godot;

namespace Jomolith.Editor.Models.Objects;

public struct ObjectSnapshot
{
    public int Id;
    public int? ParentId;
    
    public ObjectType Shape;
    public Vector3 Position;
    public Quaternion Rotation;
    public ObjectDimensions Dimensions;
    public SurfaceData SurfaceData;
    
    // Used for meshes only
    public string? ResourcePath;
}