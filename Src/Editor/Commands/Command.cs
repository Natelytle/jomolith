namespace Jomolith.Editor.Commands;

public interface Command
{
    public void Execute();
    public void Undo();
}