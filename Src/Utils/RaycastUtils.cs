using Godot;
using Godot.Collections;

namespace Jomolith.Utils;

public static class RaycastUtils
{
    public static Dictionary DoScreenRaycast(Vector2 mousePos, Camera3D camera, PhysicsDirectSpaceState3D spaceState3D, uint collisionMask = 4294967295U)
    {
        Vector3 from = camera.ProjectRayOrigin(mousePos);
        Vector3 to = from + camera.ProjectRayNormal(mousePos) * 1000f;

        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(from, to, collisionMask);
        query.CollideWithAreas = false;
        query.CollideWithBodies = true;

        Dictionary result = spaceState3D.IntersectRay(query);

        return result;
    }
}