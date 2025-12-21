using System;
using Godot;
using Jomolith.Utils;

namespace Jomolith.Editor.Gizmos;

public abstract partial class Gizmo : Node3D
{
    public Camera3D EditorCamera { get; set; } = null!;

    protected Node3D[] ObjectsToManipulate = [];
    protected Transform3D[] OriginalObjectTransforms = [];

    public bool IsActive { get; protected set; }

    public override void _Ready()
    {
        InitializeSubGizmos();
    }

    public void SetTargets(Node3D[] targets)
    {
        Console.WriteLine("Called!!!");

        ObjectsToManipulate = targets;
        OriginalObjectTransforms = new Transform3D[ObjectsToManipulate.Length];

        UpdateGizmoPosition();
    }
    
    private Aabb GetBoundingBox()
    {
        Aabb? boundingBox = null;

        foreach (Node3D obj in ObjectsToManipulate)
        {
            if (obj is CollisionObject3D collisionObject3D)
            {
                foreach (Node? child in collisionObject3D.GetChildren())
                {
                    if (child is CollisionShape3D shape && shape.Shape is not null)
                    {
                        Aabb aabb = shape.GlobalTransform * shape.Shape.GetDebugMesh().GetAabb();

                        boundingBox = boundingBox?.Merge(aabb) ?? aabb;
                    }
                }
            }
        }

        return boundingBox!.Value;
    }

    protected virtual void BeginManipulation()
    {
        IsActive = true;
        
        // Store original states
        for (int i = 0; i < ObjectsToManipulate.Length; i++)
        {
            OriginalObjectTransforms[i] = ObjectsToManipulate[i].GlobalTransform;
        }
        
        OnManipulationBegin();
    }
    
    protected virtual void EndManipulation()
    {
        if (!IsActive) return;
        
        IsActive = false;
        OnManipulationEnd();
    }
    
    protected virtual void UpdateGizmoScale()
    {
        float distance = GlobalPosition.DistanceTo(EditorCamera.GlobalPosition);
        float scale = distance / 15.0f + 0.1f;
        
        ApplyScaleToSubGizmos(Vector3.One * scale);
    }
    
    protected virtual void UpdateGizmoPosition()
    {
        if (ObjectsToManipulate.Length == 0) return;

        Aabb boundingBox = GetBoundingBox();
        GlobalPosition = boundingBox.GetCenter();
        
        OnGizmoPositionUpdated(boundingBox);
    }

    protected bool GetCursorPlanePosition(out Godot.Collections.Dictionary result)
    {
        Vector2 mousePos = EditorCamera.GetViewport().GetMousePosition();
        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

        // Raycast from camera
        result = RaycastUtils.DoScreenRaycast(mousePos, EditorCamera, spaceState, 4);

        return result.Count > 0;
    }

    public abstract void SetSubGizmoActive(Node3D clickedNode, Vector3 clickLocation);
    protected abstract void InitializeSubGizmos();
    protected abstract void PerformManipulation(double delta);
    protected virtual void ApplyScaleToSubGizmos(Vector3 scale) { }
    protected virtual void OnManipulationBegin() { }
    protected virtual void OnManipulationEnd() { }
    protected virtual void OnGizmoPositionUpdated(Aabb boundingBox) { }
}