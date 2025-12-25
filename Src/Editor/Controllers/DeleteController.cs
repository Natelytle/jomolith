
using System.Linq;
using Godot;
using Jomolith.Editor.Commands;

namespace Jomolith.Editor.Controllers;

public partial class DeleteController : RefCounted
{
    private EditorContext _context = null!;

    public void Setup(EditorContext context)
    {
        _context = context;
    }

    public void OnDeletePressed()
    {
        var ids = _context.SelectionModel.SelectedIds;

        if (ids.Count == 0)
            return;
        
        _context.CommandStack.Execute(new DeleteSelectionCommand(_context.SceneModel, _context.SelectionModel, ids.ToArray()));
        _context.SelectionModel.Clear();
    }
}