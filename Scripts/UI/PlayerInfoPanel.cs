using System;
using System.Globalization;
using Godot;

namespace Jomolith.Scripts.UI;

public partial class PlayerInfoPanel : Control
{
	private Label _frameTime;
	private Label _rootPosition;
	private Label _rootRotation;
	private Label _rootLinearVelocity;
	private Label _rootAngularVelocity;
	private Label _cameraRotation;
	private Label _humanoidState;
	private Label _zoom;

	[Export] 
	private Humanoid.KineticHumanoid _player;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_frameTime = (Label)GetNode("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/Frame Time/Value");
		_rootPosition = (Label)GetNode("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/Root Position/Value");
		_rootRotation = (Label)GetNode("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/Root Rotation/Value");
		_rootLinearVelocity = (Label)GetNode("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/Root Linear Velocity/Value");
		_rootAngularVelocity = (Label)GetNode("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/Root Angular Velocity/Value");
		_cameraRotation = (Label)GetNode("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/Camera Rotation/Value");
		_humanoidState = (Label)GetNode("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/Humanoid State/Value");
		_zoom = (Label)GetNode("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/Zoom/Value");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		_frameTime.SetText(delta.ToString("0.000000"));
		_rootPosition.SetText(FormatVectorPosition(_player.GlobalPosition));
		_rootRotation.SetText(FormatVectorAngle(_player.GlobalRotation));
		_rootLinearVelocity.SetText(FormatVectorPosition(_player.LinearVelocity));
		_rootAngularVelocity.SetText(FormatVectorPosition(_player.AngularVelocity));
		 _cameraRotation.SetText(FormatVectorAngle(_player.CameraRotation));
		_humanoidState.SetText(_player.State.StateName);
		_zoom.SetText(_player.CameraZoom.ToString("0.000"));
	}

	private static string FormatVectorPosition(Vector3 vector)
	{
		string x = float.Round(vector.X, 3, MidpointRounding.ToZero).ToString("0.000");
		string y = float.Round(vector.Y, 3, MidpointRounding.ToZero).ToString("0.000");
		string z = float.Round(vector.Z, 3, MidpointRounding.ToZero).ToString("0.000");

		string final = x.PadLeft(7) + ", " + y.PadLeft(7) + ", " + z.PadLeft(7);

		return final;
	}
	
	private static string FormatVectorAngle(Vector3 vector)
	{
		string x = float.Round(float.RadiansToDegrees(vector.X), 3, MidpointRounding.ToZero).ToString("0.000");
		string y = float.Round(float.RadiansToDegrees(vector.Y), 3, MidpointRounding.ToZero).ToString("0.000");
		string z = float.Round(float.RadiansToDegrees(vector.Z), 3, MidpointRounding.ToZero).ToString("0.000");

		string final = x.PadLeft(7) + ", " + y.PadLeft(7) + ", " + z.PadLeft(7);

		return final;
	}
}