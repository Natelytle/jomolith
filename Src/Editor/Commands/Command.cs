namespace Jomolith.Editor.Commands;

public interface ICommand
{
    public void Execute();
    public void Undo();
}