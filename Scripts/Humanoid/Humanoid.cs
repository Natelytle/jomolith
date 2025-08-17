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
	private CollisionShape3D _collisionShape;
	private HumanoidCamera _camera;
	private HumanoidStateMachine _stateMachine;

	// Avatar information
	private Vector3 _groundCheckerNormal = Vector3.Down;
	private const float TorsoHeight = 3.0f;
	public const float HipHeight = 2.0f;
	private const float MaxStairLength = 1.95f;
	private const float MaxLadderHeight = 2.5f;
	private const float MinLadderGap = 0.05f;
	private const float HitboxHeight = 3.0f;
	private const float HitboxWidth = 2.0f;
	private const float HitboxDepth = 1.0f;

	// Directional info
	private static Vector3 WorldXVector => Vector3.Right;
	public static Vector3 WorldYVector => Vector3.Up;
	private static Vector3 WorldZVector => Vector3.Forward;

	private static Basis WorldBasis => new(WorldXVector, WorldYVector, -WorldZVector);

	private Vector3 PlayerXVector => Basis.X;
	public Vector3 PlayerYVector => Basis.Y;
	public Vector3 PlayerZVector => -Basis.Z;
	private Basis PlayerBasis => Basis;

	public bool RotationLocked => _camera.CameraLocked;

	public override void _Ready()
	{
		CanSleep = false;

		PhysicsMaterial physicsMaterial = new PhysicsMaterial();
		physicsMaterial.Friction = 0.3f;

		SetPhysicsMaterialOverride(physicsMaterial);

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
		_climbCheckerUp.TargetPosition = PlayerYVector * MaxLadderHeight;
		AddChild(_climbCheckerUp);

		// this one is just flipped
		_climbCheckerDown = new RayCast3D();
		_climbCheckerDown.Position = _climbCheckerUp.Position + _climbCheckerUp.TargetPosition;
		_climbCheckerDown.TargetPosition = -_climbCheckerUp.TargetPosition;
		AddChild(_climbCheckerDown);

		_torsoCollisionBox = new BoxShape3D();
		_torsoCollisionBox.Size = new Vector3(HitboxWidth, HitboxHeight, HitboxDepth);

		_collisionShape = new CollisionShape3D();
		_collisionShape.Shape = _torsoCollisionBox;
		_collisionShape.Transform = new Transform3D(Basis.Identity, WorldYVector * HipHeight / 2.0f);
		AddChild(_collisionShape);

		_camera = new HumanoidCamera();
		_camera.Subject = this;
		AddChild(_camera);
		
		// State machine
		_stateMachine = new HumanoidStateMachine();

		HumanoidState falling = new Falling(this);

		_stateMachine.AddChild(falling);
		_stateMachine.AddChild(new Standing(this));
		_stateMachine.AddChild(new Coyote(this));
		_stateMachine.AddChild(new Climbing(this));
		_stateMachine.AddChild(new StandClimbing(this));

		_stateMachine.InitialState = falling; 
		
		AddChild(_stateMachine);
	}

	// Movement info
	private const float WalkSpeed = 16f;
	private const float JumpPower = 55f;
	private const float MaxSlope = 89f;

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

		// // We're considered touching the ground if our legs are colliding with the ground and we have no upwards velocity.
		// bool touchingGround = medianLength < HipHeight + 0.05 && (LinearVelocity.Y < 5 || _grounding == GroundingState.Grounded);
		//
		// if (!touchingGround) {
		// 	if (_grounding != GroundingState.Grounded) 
		// 		return;
		//
		// 	_grounding = GroundingState.Coyote;
		// 	_coyoteTick = CoyoteTime;
		//
		// 	return;
		// }
		//
		// if (_grounding is GroundingState.Climbing)
		// {
		// 	_grounding = GroundingState.ClimbStanding;
		// 	return;
		// }
		//
		// _grounding = GroundingState.Grounded;
		//
		// // Counteract gravity and adjust our legs to the floor.
		// ApplyCentralForce(-GetGravity() * Mass);
		// SetAxisVelocity(PlayerYVector * (HipHeight - medianLength) * 20);
	}

	public bool IsClimbing()
	{
		if (!(_climbCheckerUp.IsColliding() && _climbCheckerDown.IsColliding()))
		{
			// switch (_grounding)
			// {
			// 	case GroundingState.Climbing:
			// 		_grounding = GroundingState.Airborne;
			// 		break;
			// 	case GroundingState.ClimbStanding:
			// 		_grounding = GroundingState.Grounded;
			// 		break;
			// }

			return false;
		}

		float length = (_climbCheckerUp.GetCollisionPoint() - _climbCheckerDown.GetCollisionPoint()).Length();

		return length > MinLadderGap;

		//
		// if (length < MinLadderGap)
		// {
		// 	switch (_grounding)
		// 	{
		// 		case GroundingState.Climbing:
		// 			_grounding = GroundingState.Airborne;
		// 			break;
		// 		case GroundingState.ClimbStanding:
		// 			_grounding = GroundingState.Grounded;
		// 			break;
		// 	}
		//
		// 	return;
		// }

		//
		// _grounding = GroundingState.Climbing;
		// ApplyCentralForce(-GetGravity() * Mass);
		//
		// // We want to arrest movement in the X and Y directions, with acceleration of up to 140 studs/s^2
		// Vector3 correctionVector = new Vector3(-LinearVelocity.X, 0, -LinearVelocity.Z);
		// float speed = Math.Min(correctionVector.Length() * 50f, 14000f);
		// correctionVector = correctionVector.Normalized() * speed;
		//
		// ApplyCentralForce(correctionVector);
	}

	public void Walk(Vector3 direction, float acceleration)
	{
		Vector3 target = direction * WalkSpeed;

		Vector3 correctionVector = target - new Vector3(LinearVelocity.X, 0, LinearVelocity.Z);

		float length = Math.Min(acceleration, 100 * correctionVector.Length());

		correctionVector = correctionVector.Normalized() * length;
		
		ApplyCentralForce(correctionVector);
	}

	public void Jump()
	{
		SetAxisVelocity(WorldYVector * JumpPower);
	}

	public void RotateTo(Vector3 target)
	{
		Vector3 otherRotation = AngularVelocity.RemoveAngularComponent(WorldBasis.Y);

		if (target.Length() == 0)
		{
			AngularVelocity = otherRotation;
			return;
		}

		Vector3 directionVector = PlayerZVector + PlayerYVector;
		Plane plane = new Plane(Vector3.Up);
		directionVector = plane.Project(directionVector);

		float angle = directionVector.SignedAngleTo(target, Vector3.Up);
		
		SetAngularVelocity(otherRotation + WorldBasis.Y * angle * 10.0f);
	}

	public void SnapToCamera()
	{
		Vector3 currentRotation = Rotation;
		currentRotation.Y = _camera.Rotation.Y;
		Rotation = currentRotation;
	}
	
	public void ApplyUprightForce()
	{
		Vector3 cross = PlayerYVector.Cross(WorldYVector).Normalized();

		float angle = PlayerYVector.AngleTo(WorldYVector);
		
		SetAngularVelocity(cross * angle * 50);
	}
}