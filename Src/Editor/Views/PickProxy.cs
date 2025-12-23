using Godot;
using Jomolith.Editor.Models.Objects;

namespace Jomolith.Editor.Views;

public partial class PickProxy : StaticBody3D
{
    public int ObjectId;

    public CollisionShape3D Collider;
    public ObjectType Type;
}