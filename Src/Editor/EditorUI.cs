using System;
using Godot;
using static Jomolith.Editor.EditorController;

namespace Jomolith.Editor;

public partial class EditorUI : Control
{
	private EditorController? _editorController;

	public override void _Ready()
	{
		_editorController = (EditorController)GetParent();

		((Button)GetNode("Panel/VBoxContainer/SelectButton")).Pressed += () => OnModeButtonPressed(EditorMode.Select);
		((Button)GetNode("Panel/VBoxContainer/MoveButton")).Pressed += () => OnModeButtonPressed(EditorMode.Translate);
		((Button)GetNode("Panel/VBoxContainer/RotateButton")).Pressed += () => OnModeButtonPressed(EditorMode.Rotate);
		((Button)GetNode("Panel/VBoxContainer/ScaleButton")).Pressed += () => OnModeButtonPressed(EditorMode.Scale);
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	private void OnModeButtonPressed(EditorMode mode)
	{
		_editorController?.ChangeEditorMode(mode);
	}
}