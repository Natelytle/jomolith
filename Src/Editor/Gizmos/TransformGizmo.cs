
using System;
using Godot;
using Jomolith.Editor.Gizmos.SubGizmos;

namespace Jomolith.Editor.Gizmos;

public partial class TransformGizmo : Node3D
{
    [Export] public Camera3D EditorCamera { get; private set; } = null!;
    [Export] public Node3D TestObject { get; private set; } = null!;

    private Node3D[] _objectsToMove = null!;
    private Vector3[] _objectOriginalPositions = null!;
    private Vector3 _objectOffset;

    private TransformSubgizmo? _activeSubgizmo;

    public override void _Ready()
    {
        // x, y, z
        foreach (var node in GetChildren())
        {
            if (node is TransformSubgizmo subgizmo)
            {
                subgizmo.HandleClicked += BeginTransform;
            }
        }

        Node3D[] targets = [TestObject];
        
        SetTargets(targets);
    }

    public override void _PhysicsProcess(double delta)
    {
        SetGizmoScales();

        if (_activeSubgizmo is not null && Input.IsMouseButtonPressed(MouseButton.Left))
        {
            Vector3 toCamera = (EditorCamera.GlobalPosition - _activeSubgizmo.CursorTransformPlane.GlobalPosition).Normalized();
            Vector3 projected = toCamera - _activeSubgizmo.MoveAxis * toCamera.Dot(_activeSubgizmo.MoveAxis);

            ((WorldBoundaryShape3D)_activeSubgizmo.CursorTransformCollisionShape.Shape).Plane = new Plane(projected);

            Vector2 mousePos = EditorCamera.GetViewport().GetMousePosition();
            
            // Raycast from camera through mouse position
            Vector3 from = EditorCamera.ProjectRayOrigin(mousePos);
            Vector3 to = from + EditorCamera.ProjectRayNormal(mousePos) * 1000f;

            PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(from, to, 2);
            query.CollideWithAreas = false;
            query.CollideWithBodies = true;

            PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
            Godot.Collections.Dictionary result = spaceState.IntersectRay(query);

            if (result.Count <= 0) 
                return;
            
            Vector3 localHitPosition = result["position"].AsVector3() - result["collider"].As<Node3D>().GlobalPosition;

            _objectOffset = _activeSubgizmo.MousePlanePositionToUnits(localHitPosition);

            for (int i = 0; i < _objectsToMove.Length; i++)
            {
                _objectsToMove[i].GlobalPosition = _objectOriginalPositions[i] + _objectOffset;
            }
            
            SetGizmoPositions();
        }
        else
        {
            CommitTransform();
            _activeSubgizmo = null;
        }
    }

    public void SetTargets(Node3D[] targets)
    {
        _objectsToMove = targets;
        _objectOriginalPositions = new Vector3[_objectsToMove.Length];

        SetGizmoPositions();
    }

    private void SetGizmoScales()
    {
        Vector3 distance = GlobalPosition - EditorCamera.GlobalPosition;
        Vector3 sizeMultiplier = Vector3.One * distance.Length() / 15.0f;
        
        GetNode<TransformSubgizmo>("TransformSubgizmoPosX").Scale = sizeMultiplier;
        GetNode<TransformSubgizmo>("TransformSubgizmoPosY").Scale = sizeMultiplier;
        GetNode<TransformSubgizmo>("TransformSubgizmoPosZ").Scale = sizeMultiplier;
        GetNode<TransformSubgizmo>("TransformSubgizmoNegX").Scale = sizeMultiplier;
        GetNode<TransformSubgizmo>("TransformSubgizmoNegY").Scale = sizeMultiplier;
        GetNode<TransformSubgizmo>("TransformSubgizmoNegZ").Scale = sizeMultiplier;
    }

    private void SetGizmoPositions()
    {
        Aabb boundingBox = GetBoundingBox();
        
        GlobalPosition = boundingBox.GetCenter();
        Vector3 halfSize = boundingBox.Size / 2.0f;

        GetNode<TransformSubgizmo>("TransformSubgizmoPosX").Position = halfSize with { Y = 0, Z = 0 };
        GetNode<TransformSubgizmo>("TransformSubgizmoPosY").Position = halfSize with { X = 0, Z = 0 };
        GetNode<TransformSubgizmo>("TransformSubgizmoPosZ").Position = halfSize with { X = 0, Y = 0 };
        GetNode<TransformSubgizmo>("TransformSubgizmoNegX").Position = -halfSize with { Y = 0, Z = 0 };
        GetNode<TransformSubgizmo>("TransformSubgizmoNegY").Position = -halfSize with { X = 0, Z = 0 };
        GetNode<TransformSubgizmo>("TransformSubgizmoNegZ").Position = -halfSize with { X = 0, Y = 0 };
    }

    private void BeginTransform(TransformSubgizmo subgizmo)
    {
        _activeSubgizmo = subgizmo;
        _activeSubgizmo.CursorTransformCollisionShape.SetDisabled(false);

        for (int i = 0; i < _objectsToMove.Length; i++)
        {
            _objectOriginalPositions[i] = _objectsToMove[i].GlobalPosition;
        }
    }

    private void CommitTransform()
    {
        _activeSubgizmo?.CursorTransformCollisionShape.SetDisabled(true);
        _activeSubgizmo = null;

        for (int i = 0; i < _objectsToMove.Length; i++)
        {
            _objectOriginalPositions[i] = _objectsToMove[i].GlobalPosition;
        }

        _objectOffset = Vector3.Zero;
    }

    private Aabb GetBoundingBox()
    {
        Aabb? boundingBox = null;

        foreach (Node3D obj in _objectsToMove)
        {
            if (obj is CollisionObject3D collisionObject3D)
            {
                foreach (Node? child in collisionObject3D.GetChildren())
                {
                    if (child is CollisionShape3D shape && shape.Shape is not null)
                    {
                        Aabb aabb = shape.GlobalTransform * shape.Shape.GetDebugMesh().GetAabb();

                        boundingBox ??= aabb;
                        boundingBox = boundingBox.Value.Merge(aabb);
                    }
                }
            }
        }

        return boundingBox!.Value;
    }
}