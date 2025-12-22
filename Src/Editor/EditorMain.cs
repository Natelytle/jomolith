using Godot;
using Jomolith.Editor.Commands;
using Jomolith.Editor.Controllers;
using Jomolith.Editor.Models;

namespace Jomolith.Editor;

public partial class EditorMain : Node
{
	public const int ObjectCollisionMask = 1;
	public const int GizmoCollisionMask = 2;
	public const int GizmoDragCollisionMask = 3;

	public SceneModel SceneModel = new();
	public SelectionModel SelectionModel = new();
	public CommandStack CommandStack = new();
	
	public MoveController MoveController = new();
	public DeleteController DeleteController = new();
	public SelectionController SelectionController = new();
	
	public InputHandler InputHandler = new();
	
	public override void _Ready()
	{
		EditorState editorState = new();
		
		editorState.Setup(SceneModel, SelectionModel);

		MoveController.Setup(editorState, CommandStack);
		DeleteController.Setup(editorState, CommandStack);
		SelectionController.Setup(editorState);

		InputHandler.Setup(MoveController, SelectionController, DeleteController, CommandStack);
	}
}