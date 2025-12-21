
using System;
using Godot;
using Jomolith.Editor2.Gizmos.SubGizmos;

namespace Jomolith.Editor2.Gizmos;

public partial class TransformGizmo : Gizmo
{
    private TransformSubGizmo? _activeSubgizmo;
    private Vector3 _manipulationOffset;
    
    private TransformSubGizmo _posX = null!;
    private TransformSubGizmo _posY = null!;
    private TransformSubGizmo _posZ = null!;
    private TransformSubGizmo _negX = null!;
    private TransformSubGizmo _negY = null!;
    private TransformSubGizmo _negZ = null!;

    public override void SetSubGizmoActive(Node3D clickedNode, Vector3 clickLocation)
    {
        // Determine which sub-gizmo was clicked
        if (_posX.IsAncestorOf(clickedNode) || clickedNode == _posX)
        {
            OnSubGizmoClicked(_posX, clickLocation);
        }
        else if (_posY.IsAncestorOf(clickedNode) || clickedNode == _posY)
        {
            OnSubGizmoClicked(_posY, clickLocation);
        }
        else if (_posZ.IsAncestorOf(clickedNode) || clickedNode == _posZ)
        {
            OnSubGizmoClicked(_posZ, clickLocation);
        }
        else if (_negX.IsAncestorOf(clickedNode) || clickedNode == _negX)
        {
            OnSubGizmoClicked(_negX, clickLocation);
        }
        else if (_negY.IsAncestorOf(clickedNode) || clickedNode == _negY)
        {
            OnSubGizmoClicked(_negY, clickLocation);
        }
        else if (_negZ.IsAncestorOf(clickedNode) || clickedNode == _negZ)
        {
            OnSubGizmoClicked(_negZ, clickLocation);
        }
    }

    protected override void InitializeSubGizmos()
    {
        // Cache references
        _posX = GetNode<TransformSubGizmo>("TransformSubgizmoPosX");
        _posY = GetNode<TransformSubGizmo>("TransformSubgizmoPosY");
        _posZ = GetNode<TransformSubGizmo>("TransformSubgizmoPosZ");
        _negX = GetNode<TransformSubGizmo>("TransformSubgizmoNegX");
        _negY = GetNode<TransformSubGizmo>("TransformSubgizmoNegY");
        _negZ = GetNode<TransformSubGizmo>("TransformSubgizmoNegZ");
    }
    
    protected override void ApplyScaleToSubGizmos(Vector3 scale)
    {
        _posX.Scale = scale;
        _posY.Scale = scale;
        _posZ.Scale = scale;
        _negX.Scale = scale;
        _negY.Scale = scale;
        _negZ.Scale = scale;
    }
    
    protected override void PerformManipulation(double delta)
    {
        if (_activeSubgizmo == null) return;

        UpdateCursorPlane(_activeSubgizmo);

        // Raycast to plane
        if (!GetCursorPlanePosition(out var result))
            return;

        // Calculate movement
        Vector3 hitPosition = result["position"].AsVector3();
        Vector3 localHit = hitPosition - _activeSubgizmo.CursorTransformPlane.GlobalPosition;
        _manipulationOffset = _activeSubgizmo.MousePlanePositionToUnits(localHit);

        // Apply to all objects
        for (int i = 0; i < ObjectsToManipulate.Length; i++)
        {
            ObjectsToManipulate[i].SetTransform(OriginalObjectTransforms[i] with
            {
                Origin = OriginalObjectTransforms[i].Origin + _manipulationOffset
            });
        }

        UpdateGizmoPosition();
    }

    public override void _PhysicsProcess(double delta)
    {
        UpdateGizmoScale();

        if (_activeSubgizmo is not null && Input.IsMouseButtonPressed(MouseButton.Left))
        {
            PerformManipulation(delta);
        }
        else
        {
            EndManipulation();
        }
    }

    private void OnSubGizmoClicked(TransformSubGizmo subgizmo, Vector3 clickLocation)
    {
        _activeSubgizmo = subgizmo;
        _activeSubgizmo.CursorTransformPlane.GlobalPosition = clickLocation;
        BeginManipulation();
    }

    protected override void OnManipulationBegin()
    {
        _activeSubgizmo?.CursorTransformCollisionShape.SetDisabled(false);
    }

    protected override void OnManipulationEnd()
    {
        if (_activeSubgizmo != null)
        {
            _activeSubgizmo.CursorTransformCollisionShape.SetDisabled(true);
            _activeSubgizmo = null;
        }

        _manipulationOffset = Vector3.Zero;
    }
    
    private void UpdateCursorPlane(TransformSubGizmo subGizmo)
    {
        Vector3 toCamera = (EditorCamera.GlobalPosition - subGizmo.CursorTransformPlane.GlobalPosition).Normalized();
        Vector3 projected = toCamera - subGizmo.MoveAxis * toCamera.Dot(subGizmo.MoveAxis);

        ((WorldBoundaryShape3D)subGizmo.CursorTransformCollisionShape.Shape).Plane = new Plane(projected);
    }
}