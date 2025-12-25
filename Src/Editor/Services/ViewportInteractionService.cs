using Godot;
using Jomolith.Editor.Views;

namespace Jomolith.Editor.Services;

public partial class ViewportInteractionService : RefCounted
{
    private SubViewportContainer _viewportContainer = null!;
    private SubViewport _viewport = null!;
    private Camera3D _camera = null!;
    
    public void Setup(SubViewportContainer viewportContainer)
    {
        _viewportContainer = viewportContainer;
        _viewport = viewportContainer.GetChild<SubViewport>(0);
        _camera = _viewport.GetNode<Camera3D>("Camera3D");
    }
    
    public int? RaycastObject(Vector2 viewportPos, out Vector3? hitPos)
    {
        Vector3 from = _camera.ProjectRayOrigin(viewportPos);
        Vector3 to = from + _camera.ProjectRayNormal(viewportPos) * 1000f;

        var spaceState = _camera.GetWorld3D().DirectSpaceState;
        
        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(from, to, EditorContext.ObjectCollisionMask);
        query.CollideWithAreas = false;
        query.CollideWithBodies = true;

        var result = spaceState.IntersectRay(query);

        if (result.Count > 0)
        {
            hitPos = (Vector3)result["position"];
            Variant collider = result["collider"];

            if (collider.Obj is PickProxy proxy)
            {
                int objectId = proxy.ObjectId;
                return objectId;
            }
        }

        hitPos = null;
        return null;
    }
}