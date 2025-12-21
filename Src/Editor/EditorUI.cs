
using Godot;
using static Jomolith.Editor.Editor;

namespace Jomolith.Editor;

public partial class EditorUI : Control
{
	[Signal] public delegate void OnModeButtonPressedEventHandler(GizmoMode mode);
	[Export] private Editor? Editor { get; set; }
	
	public override void _Ready()
	{
		((Button)GetNode("Panel/VBoxContainer/SelectButton")).Pressed += () => ModeButtonPressed(GizmoMode.Select);
		((Button)GetNode("Panel/VBoxContainer/MoveButton")).Pressed += () => ModeButtonPressed(GizmoMode.Transform);
		((Button)GetNode("Panel/VBoxContainer/RotateButton")).Pressed += () => ModeButtonPressed(GizmoMode.Rotate);
		((Button)GetNode("Panel/VBoxContainer/ScaleButton")).Pressed += () => ModeButtonPressed(GizmoMode.Scale);
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	private void ModeButtonPressed(GizmoMode mode)
	{
		// int is because you can only emit Variant<> with signals.
		EmitSignal(SignalName.OnModeButtonPressed, (int)mode);
	}
}