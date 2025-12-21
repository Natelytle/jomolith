using Godot;

namespace Jomolith.Editor2.Gizmos.SubGizmos;

public partial class TransformSubGizmo : Node3D
{
    [Signal]
    public delegate void HandleClickedEventHandler(TransformSubGizmo subGizmo);
    
    [Export]
    public Color HandleColour { get; private set; }

    public StaticBody3D Handle { get; private set; } = null!;
    public CollisionShape3D HandleCollisionShape { get; private set; } = null!;
    
    // We send a ray from our cursor to this plane to determine where we're bringing our gizmo
    public StaticBody3D CursorTransformPlane { get; private set; } = null!;
    public CollisionShape3D CursorTransformCollisionShape { get; private set; } = null!;

    // Should be static but idk brah i be coolin
    public Vector3 MoveAxis { get; private set; }

    public override void _Ready()
    {
        Handle = GetNode<StaticBody3D>("SelectionHandle");
        HandleCollisionShape = GetNode<CollisionShape3D>("SelectionHandle/CollisionShape3D");
        CursorTransformPlane = GetNode<StaticBody3D>("CursorPositionTransformPlane");
        CursorTransformCollisionShape = GetNode<CollisionShape3D>("CursorPositionTransformPlane/CollisionShape3D");

        MoveAxis = -GlobalBasis.Z;

        foreach (Node? child in Handle.GetChildren())
        {
            if (child is MeshInstance3D mesh)
            {
                StandardMaterial3D material = (StandardMaterial3D)mesh.GetActiveMaterial(0);
                material.AlbedoColor = HandleColour with { A = 0.8f };
            }
        }
    }

    public Vector3 MousePlanePositionToUnits(Vector3 localPosition)
    {
        return MoveAxis * localPosition.Dot(MoveAxis);
    }
}