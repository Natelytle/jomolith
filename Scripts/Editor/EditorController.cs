using Godot;

namespace Jomolith.Scripts.Editor;

public partial class EditorController : Node3D
{
	public event EditorModeChangedHandler? OnEditorModeChanged;
	public delegate void EditorModeChangedHandler();

	public enum EditorMode
	{
		Select,
		Translate,
		Rotate,
		Scale
	}

	public EditorMode CurrentEditorMode;

	public bool IsInFreelookMode;

	public void ChangeEditorMode(EditorMode newMode)
	{
		CurrentEditorMode = newMode;
		OnEditorModeChanged?.Invoke();
	}

	public override void _Input(InputEvent e)
	{
		GetNode("EditorViewport")._Input(e);
	}
}