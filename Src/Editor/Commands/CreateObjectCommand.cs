using Godot;
using Jomolith.Editor.Models;
using Jomolith.Editor.Models.Objects;

namespace Jomolith.Editor.Commands;

public class CreateObjectCommand(SceneModel scene, Transform3D transform, ObjectType shape, SurfaceData surfaceData, int? parentId, string? resourcePath)
    : ICommand
{
    private int? _id;

    public void Execute()
    {
        _id = scene.CreateObject(transform, shape, surfaceData, parentId, resourcePath);
    }

    public void Undo()
    {
        if (_id is not null)
            scene.DeleteObjectInternal(_id.Value);
    }
}