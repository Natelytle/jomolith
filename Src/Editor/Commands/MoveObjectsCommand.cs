using Godot;
using Jomolith.Editor.Models;

namespace Jomolith.Editor.Commands;

public class MoveObjectsCommand(SceneModel scene, int[] ids, Vector3 delta)
    : ICommand
{
    public void Execute()
    {
        foreach (int id in ids)
            scene.TranslateObject(id, delta);
    }

    public void Undo()
    {
        foreach (int id in ids)
            scene.TranslateObject(id, -delta);
    }
}