using Godot;

namespace Jomolith.Editor.Gizmos.SubGizmos;

public partial class TransformSubgizmo : Node3D
{
    [Signal]
    public delegate void HandleClickedEventHandler(TransformSubgizmo subgizmo);

    public StaticBody3D Handle { get; private set; } = null!;
    public CollisionShape3D HandleCollisionShape { get; private set; } = null!;
    
    // We send a ray from our cursor to this plane to determine where we're bringing our gizmo
    public StaticBody3D CursorTransformPlane { get; private set; } = null!;
    public CollisionShape3D CursorTransformCollisionShape { get; private set; } = null!;

    // Should be static but idk brah i be coolin
    public Vector3 MoveAxis { get; private set; }

    public override void _Ready()
    {
        Handle = GetNode<StaticBody3D>("SelectionHandle");
        HandleCollisionShape = GetNode<CollisionShape3D>("SelectionHandle/CollisionShape3D");
        CursorTransformPlane = GetNode<StaticBody3D>("CursorPositionTransformPlane");
        CursorTransformCollisionShape = GetNode<CollisionShape3D>("CursorPositionTransformPlane/CollisionShape3D");

        MoveAxis = -GlobalBasis.Z;
        Handle.InputEvent += HandleOnInputEvent;
    }

    private void HandleOnInputEvent(Node camera, InputEvent @event, Vector3 eventposition, Vector3 normal, long shapeidx)
    {
        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
        {
            EmitSignal(SignalName.HandleClicked, this);
            CursorTransformPlane.GlobalPosition = eventposition;
        }
    }

    public Vector3 MousePlanePositionToUnits(Vector3 localPosition)
    {
        return MoveAxis * localPosition.Dot(MoveAxis);
    }
}