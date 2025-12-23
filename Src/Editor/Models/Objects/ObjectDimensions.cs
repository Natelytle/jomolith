using Godot;

namespace Jomolith.Editor.Models.Objects;

public struct ObjectDimensions
{
    public float Radius; // Spheres and Cylinders
    public float Height; // Cylinders
    public Vector3 Size; // Blocks and Wedges
}