using System;
using System.Collections.Generic;
using Godot;
using Jomolith.Editor.Gizmos;
using Godot.Collections;
using Jomolith.Utils;

namespace Jomolith.Editor;

public partial class Editor : Node3D
{
	[Export] private Camera3D _editorCamera = null!;
	[Export] private Node3D _towerRoot = null!;
	[Export] private EditorUI _editorUI = null!;

	[Export] private PackedScene? _transformGizmoScene;
	[Export] private PackedScene? _rotateGizmoScene;
	[Export] private PackedScene? _scaleGizmoScene;

	private List<Node3D> _selectedObjects = [];
	private Gizmo? _activeGizmo;
	private GizmoMode _currentMode = GizmoMode.Select;
	
	private TransformGizmo? _transformGizmo;
	// private RotateGizmo? _rotateGizmo;
	// private ScaleGizmo? _scaleGizmo;

	private Vector2? _queuedSelectRaycastPosition;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		InitializeGizmos();
		HideAllGizmos();

		_editorUI.OnModeButtonPressed += SwitchGizmoMode;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true } mouseButton)
		{
			// Don't select while dragging gizmo
			if (_activeGizmo?.IsActive ?? false)
				return;

			_queuedSelectRaycastPosition = mouseButton.Position;
		}
	}
	
	// Use physics process to call the direct space state
	public override void _PhysicsProcess(double delta)
	{
		if (_queuedSelectRaycastPosition is not null)
		{
			Vector2 position = _queuedSelectRaycastPosition.Value;
			_queuedSelectRaycastPosition = null;

			// Don't select if we're moving a gizmo
			if (_activeGizmo?.IsActive ?? false)
				return;
			
			bool gizmoSelected = TrySelectGizmoAtMouse(position);

			if (gizmoSelected)
				return;

			bool multiSelect = Input.IsKeyPressed(Key.Ctrl);
			TrySelectObjectAtMouse(position, multiSelect);
		}
	}

	private void InitializeGizmos()
	{
		if (_transformGizmoScene is not null)
		{
			_transformGizmo = _transformGizmoScene.Instantiate<TransformGizmo>();
			AddChild(_transformGizmo);
			_transformGizmo.EditorCamera = _editorCamera;
		}
		
		// Add rotate and scale
	}

	private void HideAllGizmos()
	{
		if (_transformGizmo != null) _transformGizmo.Visible = false;
		
		// Add rotate and scale
	}
	
	private void SwitchGizmoMode(GizmoMode mode)
	{
		if (_currentMode == mode)
			return;

		_currentMode = mode;
		HideAllGizmos();

		_activeGizmo = mode switch
		{
			GizmoMode.Transform => _transformGizmo,
			// GizmoMode.Rotate => _rotateGizmo,
			// GizmoMode.Scale => _scaleGizmo,
			_ => null
		};

		// Update gizmo with current selection
		UpdateGizmoTargets();

		GD.Print($"Switched to {mode} mode");
	}

	private bool TrySelectGizmoAtMouse(Vector2 mousePos)
	{
		if (_activeGizmo is null || _activeGizmo.Visible == false)
			return false;

		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

		// Raycast from camera
		Dictionary result = RaycastUtils.DoScreenRaycast(mousePos, _editorCamera, spaceState, 2);

		if (result.Count > 0)
		{
			Node3D obj = result["collider"].As<Node3D>();
			Vector3 location = result["position"].AsVector3();

			if (_activeGizmo?.IsAncestorOf(obj) ?? false)
			{
				_activeGizmo?.SetSubGizmoActive(obj, location);
				return true;
			}
		}

		return false;
	}

	private void TrySelectObjectAtMouse(Vector2 mousePos, bool multiSelect)
	{
		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

		// Raycast from camera
		Dictionary result = RaycastUtils.DoScreenRaycast(mousePos, _editorCamera, spaceState, 1);

		if (result.Count > 0)
		{
			Node3D hitObject = result["collider"].As<Node3D>();
            
			if (IsSelectableObject(hitObject))
			{
				if (multiSelect)
				{
					ToggleSelection(hitObject);
				}
				else
				{
					SelectObject(hitObject);
				}
			}
		}
		else if (!multiSelect)
		{
			ClearSelection();
		}
	}

	
	
	private bool IsSelectableObject(Node3D obj)
	{
		// Don't select gizmos
		if (obj.IsAncestorOf(_transformGizmo) || obj == _transformGizmo) // ||
		    // obj.IsAncestorOf(_rotateGizmo) || obj == _rotateGizmo ||
		    // obj.IsAncestorOf(_scaleGizmo) || obj == _scaleGizmo)
			return false;

		// Only select objects under level root
		return _towerRoot.IsAncestorOf(obj);
	}
	
	private void SelectObject(Node3D obj)
	{
		_selectedObjects.Clear();
		_selectedObjects.Add(obj);

		UpdateGizmoTargets();
	}

	private void ToggleSelection(Node3D obj)
	{
		if (_selectedObjects.Contains(obj))
		{
			_selectedObjects.Remove(obj);
		}
		else
		{
			_selectedObjects.Add(obj);
		}

		UpdateGizmoTargets();
	}
	
	private void ClearSelection()
	{
		_selectedObjects.Clear();

		UpdateGizmoTargets();
	}

	private void UpdateGizmoTargets()
	{
		Node3D[] targets = _selectedObjects.ToArray();

		if (_activeGizmo != null)
		{
			_activeGizmo.SetTargets(targets);
			_activeGizmo.SetVisible(targets.Length > 0);
		}
	}

	public enum GizmoMode
	{
		Select,
		Transform,
		Rotate,
		Scale
	}
}