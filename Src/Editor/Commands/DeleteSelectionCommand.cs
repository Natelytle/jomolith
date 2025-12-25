
using System.Linq;
using Jomolith.Editor.Models;

namespace Jomolith.Editor.Commands;

public class DeleteSelectionCommand(SceneModel scene, SelectionModel selection, int[] ids) : ICommand
{
    private readonly DeleteObjectsCommand _deleteCommand = new(scene, ids);

    private readonly int[] _previousIds = selection.SelectedIds.ToArray();
    private readonly int? _previousPrimaryId = selection.PrimaryId;

    public void Execute()
    {
        _deleteCommand.Execute();
        selection.Clear();
    }

    public void Undo()
    {
        _deleteCommand.Undo();
        selection.AddMany(_previousIds, _previousPrimaryId);
    }
}