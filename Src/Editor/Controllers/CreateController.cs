using Godot;
using Jomolith.Editor.Commands;
using Jomolith.Editor.Models.Objects;

namespace Jomolith.Editor.Controllers;

public partial class CreateController : RefCounted
{
    private EditorContext _context = null!;

    public void Setup(EditorContext context)
    {
        _context = context;
    }

    public void OnCreatePressed()
    {
        _context.CommandStack.Execute(new CreateObjectCommand(_context.SceneModel, ObjectType.Block, new Vector3(GD.Randf() * 10, GD.Randf() * 10, GD.Randf() * 10), Quaternion.Identity, new ObjectDimensions { Size = Vector3.One }, new SurfaceData(), null, null));
        _context.SelectionModel.Clear();
    }
}