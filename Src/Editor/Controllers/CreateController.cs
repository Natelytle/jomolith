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
        Vector3 randomPosition = new(GD.Randf() * 10, GD.Randf() * 10, GD.Randf() * 10);
        Quaternion randomRotation = Quaternion.FromEuler(new Vector3(GD.Randf() * 2 * Mathf.Pi, GD.Randf() * 2 * Mathf.Pi, GD.Randf() * 2 * Mathf.Pi));
        Vector3 randomScale = new(GD.Randf() * 3, GD.Randf() * 3, GD.Randf() * 3);
        SurfaceType surfaceType = (SurfaceType)(GD.Randi() % 2);
        
        _context.CommandStack.Execute(new CreateObjectCommand(_context.SceneModel, ObjectType.Block, randomPosition, randomRotation, new ObjectDimensions { Size = randomScale }, new SurfaceData { SurfaceVariant = surfaceType }, null, null));
        _context.SelectionModel.Clear();
    }
}