using Godot;
using Jomolith.Editor.Models;

namespace Jomolith.Editor.Commands;

public class CreateObjectCommand(SceneModel scene, string resourcePath, Transform3D transform, int? parentId)
    : Command
{
    private SceneModel _scene = scene;
    private string _resourcePath = resourcePath;
    private Transform3D _transform = transform;
    private int? _parentId = parentId;

    private int? _id;

    public void Execute()
    {
        _id = _scene.CreateObject(_resourcePath, _transform, _parentId);
    }

    public void Undo()
    {
        if (_id is not null)
            _scene.DeleteObjectInternal(_id.Value);
    }
}