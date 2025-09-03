using System;
using System.Collections.Generic;
using Godot;
using Jomolith.Scripts.Humanoid.HumanoidStates;
using static Jomolith.Scripts.Utils.MathUtils;

namespace Jomolith.Scripts.Humanoid;

public partial class Humanoid : RigidBody3D
{
	private const float GroundCheckerEpsilon = 0.01f;
	private const int GroundCheckerCountX = 3;
	private const int GroundCheckerCountZ = 3;

	// Nodes
	private BoxShape3D _torsoCollisionBox;
	private RayCast3D[][] _groundCheckers;
	private RayCast3D _climbCheckerUp;
	private RayCast3D _climbCheckerDown;
	private HumanoidCamera _camera;
	private HumanoidStateMachine _stateMachine;

	// Avatar information
	private Vector3 _groundCheckerNormal = Vector3.Down;
	private const float TorsoHeight = 3.0f;
	public const float HipHeight = 2.0f;
	private const float MaxStairLength = 1.95f;
	private const float LadderCheckerGap = 3.5f;
	private const float MaxLadderHeight = 2.5f;
	private const float MinLadderGap = 0.05f;
	private const float HitboxHeight = 3.0f;
	private const float HitboxWidth = 2.0f;
	private const float HitboxDepth = 1.0f;

	// Directional info
	private static Vector3 WorldXVector => Vector3.Right;
	public static Vector3 WorldYVector => Vector3.Up;
	private static Vector3 WorldZVector => Vector3.Forward;

	public static Basis WorldBasis => new(WorldXVector, WorldYVector, -WorldZVector);

	private Vector3 PlayerXVector => Basis.X;
	public Vector3 PlayerYVector => Basis.Y;
	public Vector3 PlayerZVector => -Basis.Z;
	private Basis PlayerBasis => Basis;
	
	public bool RotationLocked => _camera.CameraLocked;
	public Vector3 CameraRotation => _camera.Rotation;
	public float CameraZoom => _camera.Zoom;
	public HumanoidState State => _stateMachine.CurrentState;

	public override void _Ready()
	{
		_groundCheckers = new RayCast3D[GroundCheckerCountX][];

		for (int x = 0; x < GroundCheckerCountX; x++) 
		{
			_groundCheckers[x] = new RayCast3D[GroundCheckerCountZ];

			for (int z = 0; z < GroundCheckerCountZ; z++)
			{
				RayCast3D ray = new();

				Vector3 yOffset = PlayerYVector * GroundCheckerEpsilon + WorldYVector * (HipHeight - TorsoHeight) / 2.0f;
				Vector3 xOffset = PlayerXVector * HitboxWidth * ((x - (GroundCheckerCountX - 1) / 2.0f) / (GroundCheckerCountX - 1));
				Vector3 zOffset = PlayerZVector * HitboxDepth * ((z - (GroundCheckerCountZ - 1) / 2.0f) / (GroundCheckerCountZ - 1));

				// Adjust the outer rays inwards a bit to prevent weird behaviour at platform edges.
				xOffset *= 1 - 0.05f / (HitboxWidth / 2.0f);
				zOffset *= 1 - 0.05f / (HitboxDepth / 2.0f);

				// Position the rays in a 3x3 grid at hip height
				ray.Position = yOffset + xOffset + zOffset;
				ray.TargetPosition = -Basis.Y * (HipHeight + MaxStairLength) + -Basis.Y * GroundCheckerEpsilon;

				_groundCheckers[x][z] = ray;

				AddChild(ray);
			}
		}
		
		_climbCheckerUp = new RayCast3D();

		Vector3 climbRayZOffset = PlayerZVector * 0.75f;
		Vector3 climbRayYOffset = -PlayerYVector * (HipHeight + TorsoHeight) / 2.0f + PlayerYVector * 0.5f;

		_climbCheckerUp.Position = climbRayZOffset + climbRayYOffset;
		_climbCheckerUp.TargetPosition = PlayerYVector * LadderCheckerGap;
		AddChild(_climbCheckerUp);

		// this one is just flipped
		_climbCheckerDown = new RayCast3D();
		_climbCheckerDown.Position = _climbCheckerUp.Position + _climbCheckerUp.TargetPosition;
		_climbCheckerDown.TargetPosition = -_climbCheckerUp.TargetPosition;
		AddChild(_climbCheckerDown);
		
		// State machine
		_stateMachine = new HumanoidStateMachine();
		_stateMachine.Player = this;

		_stateMachine.InitialState = HumanoidStateMachine.StateType.Falling; 
		
		AddChild(_stateMachine);

		_camera = (HumanoidCamera)GetNode("HumanoidCamera");
	}

	// Movement info
	private const float WalkSpeed = 16.0f;
	private const float JumpPower = 55.0f;
	private const float MaxSlope = 89.0f;
	
	// State information
	private int _rotationLockTick;

	public override void _PhysicsProcess(double delta)
	{
		if (RotationLocked && _rotationLockTick <= 0)
		{
			AxisLockAngularY = true;
			_rotationLockTick = 2;
		}
		else if (AxisLockAngularY)
			AxisLockAngularY = false;

		if (RotationLocked)
		{
			Vector3 currentRotation = Rotation;
			currentRotation.Y = _camera.Rotation.Y;
			Rotation = currentRotation;

			_rotationLockTick--;
		}
	}

	public Vector3 GetMoveDirection()
	{
		Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
		Vector3 direction = (WorldBasis.Rotated(Vector3.Up, _camera.Rotation.Y) * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		return direction;
	}

	public float GetFloorDistance() 
	{
		List<float> averages = [];
		
		foreach (var groundCheckerX in _groundCheckers) 
		{
			float sum = 0;
			float div = 0;

			foreach (var groundCheckerZ in groundCheckerX)
			{
				if (!groundCheckerZ.IsColliding()) 
					continue;

				float collisionSlope = groundCheckerZ.GetCollisionNormal().AngleTo(WorldYVector);

				if (collisionSlope > Single.DegreesToRadians(MaxSlope))
					continue;

				float length = groundCheckerZ.GlobalTransform.Origin.DistanceTo(groundCheckerZ.GetCollisionPoint()) - GroundCheckerEpsilon;

				sum += length;
				div += 1;
			}

			if (div > 0)
				averages.Add(sum / div);
		}

		float medianLength = averages.Count > 0 ? GetMedian(averages.ToArray()) : 4;

		return medianLength;
	}

	public bool FacingLadder()
	{
		if (!(_climbCheckerUp.IsColliding() && _climbCheckerDown.IsColliding()))
			return false;

		float length = (_climbCheckerUp.GetCollisionPoint() - _climbCheckerDown.GetCollisionPoint()).Length();

		return length is > MinLadderGap; // and < MaxLadderHeight;
	}

	public float GetWalkSpeed() => WalkSpeed;

	public void Jump()
	{
		SetAxisVelocity(WorldYVector * JumpPower);
	}

	public void LadderJump()
	{
		Vector3 backwardsVector = -GetPlayerHeading();

		Vector3 directionVector = (WorldYVector + backwardsVector).Normalized();

		SetAxisVelocity(directionVector * JumpPower);
	}

	public Vector3 GetPlayerHeading()
	{
		Vector3 directionVector = PlayerZVector + PlayerYVector;
		Plane plane = new(Vector3.Up);
		return plane.Project(directionVector).Normalized();
	}
}