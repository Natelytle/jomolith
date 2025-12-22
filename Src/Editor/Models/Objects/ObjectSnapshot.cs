using Godot;

namespace Jomolith.Editor.Models.Objects;

public struct ObjectSnapshot
{
    public int Id;
    public int? ParentId;
    public Transform3D Transform;
    public ObjectType Shape;
    public SurfaceData SurfaceData;
    public string? ResourcePath;
}