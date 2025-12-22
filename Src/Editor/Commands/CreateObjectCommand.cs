using Godot;
using Jomolith.Editor.Models;

namespace Jomolith.Editor.Commands;

public class CreateObjectCommand(SceneModel scene, string resourcePath, Transform3D transform, Vector3 scale, int? parentId)
    : ICommand
{
    private int? _id;

    public void Execute()
    {
        _id = scene.CreateObject(resourcePath, transform, scale, parentId);
    }

    public void Undo()
    {
        if (_id is not null)
            scene.DeleteObjectInternal(_id.Value);
    }
}