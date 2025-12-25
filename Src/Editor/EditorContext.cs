using Godot;
using Jomolith.Editor.Commands;
using Jomolith.Editor.Controllers;
using Jomolith.Editor.Models;
using Jomolith.Editor.Services;

namespace Jomolith.Editor;

public partial class EditorContext : Node
{
    // Collision Layer Constants
    public const int ObjectCollisionMask = 1;
    public const int GizmoCollisionMask = 2;
    public const int GizmoDragCollisionMask = 3;

    [Export] public SubViewportContainer EditorViewport { get; private set; } = null!;
    
    // Core Models
    public SceneModel SceneModel { get; private set; } = null!;
    public SelectionModel SelectionModel { get; private set; } = null!;
    public CommandStack CommandStack { get; private set; } = null!;
    
    public MoveController MoveController { get; private set; } = null!;
    public CreateController CreateController { get; private set; } = null!;
    public DeleteController DeleteController { get; private set; } = null!;
    public SelectionController SelectionController { get; private set; } = null!;
    
    public InputHandler InputHandler { get; private set; } = null!;

    public ViewportInteractionService ViewportService { get; private set; } = null!;

    public override void _Ready()
    {
        InitializeModels();
        InitializeServices();
        InitializeControllers();
        WireConnections();
    }

    private void InitializeModels()
    {
        SceneModel = new SceneModel();
        SelectionModel = new SelectionModel();
        CommandStack = new CommandStack();
    }

    private void InitializeServices()
    {
        ViewportService = new ViewportInteractionService();
        ViewportService.Setup(EditorViewport);
    }

    private void InitializeControllers()
    {
        MoveController = new MoveController();
        MoveController.Setup(this);

        CreateController = new CreateController();
        CreateController.Setup(this);

        DeleteController = new DeleteController();
        DeleteController.Setup(this);

        SelectionController = new SelectionController();
        SelectionController.Setup(this);

        InputHandler = new InputHandler();
        InputHandler.Setup(this);
        
        AddChild(InputHandler);
    }

    private void WireConnections()
    {
        // For future use wiring up signals between controllers
    }

    public override void _ExitTree()
    {
        // Delete any signals between controllers
    }
}