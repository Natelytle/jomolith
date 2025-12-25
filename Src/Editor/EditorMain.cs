using Godot;
using Jomolith.Editor.Commands;
using Jomolith.Editor.Controllers;
using Jomolith.Editor.Models;
using Jomolith.Editor.Views;

namespace Jomolith.Editor;

public partial class EditorMain : Node
{
	[Export] private EditorContext Context { get; set; } = null!;
	[Export] private SubViewport Viewport { get; set; } = null!;
	[Export] private SceneView SceneView { get; set; } = null!;
	
	public override void _Ready()
	{
		SceneView.ConnectToModel(Context.SceneModel);
	}
}