using System;
using System.Collections.Generic;
using Godot;
using static Jomolith.Scripts.Utils.MathUtils;

namespace Jomolith.Scripts.Humanoid;

public partial class Humanoid : RigidBody3D
{
	private const float GroundCheckerEpsilon = 0.01f;
	private const int GroundCheckerCountX = 3;
	private const int GroundCheckerCountZ = 3;

	public enum GroundingState {
		Airborne,
		Coyote,
		Grounded
	}

	// Nodes
	private BoxShape3D _torsoCollisionBox;
	private RayCast3D[][] _groundCheckers;
	private CollisionShape3D _collisionShape;
	private HumanoidCamera _camera;

	// Avatar information
	private Vector3 _groundCheckerNormal = Vector3.Down;
	private const float TorsoHeight = 3.0f;
	private const float HipHeight = 2.0f;
	private const float MaxStairLength = 1.95f;
	private const float HitboxHeight = 3.0f;
	private const float HitboxWidth = 2.0f;
	private const float HitboxDepth = 1.0f;

	// Directional info
	private static Vector3 WorldXVector => Vector3.Right;
	private static Vector3 WorldYVector => Vector3.Up;
	private static Vector3 WorldZVector => Vector3.Forward;

	private static Basis WorldBasis => new(WorldXVector, WorldYVector, -WorldZVector);

	private Vector3 PlayerXVector => Basis.X;
	private Vector3 PlayerYVector => Basis.Y;
	private Vector3 PlayerZVector => -Basis.Z;
	private Basis PlayerBasis => Basis;

	public override void _Ready()
	{
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
				ray.ExcludeParent = true;
				ray.Enabled = true;

				Vector3 yOffset = WorldYVector * GroundCheckerEpsilon + WorldYVector * (HipHeight - TorsoHeight) / 2.0f;
				Vector3 xOffset = WorldXVector * HitboxWidth * ((x - (GroundCheckerCountX - 1) / 2.0f) / (GroundCheckerCountX - 1));
				Vector3 zOffset = WorldZVector * HitboxDepth * ((z - (GroundCheckerCountZ - 1) / 2.0f) / (GroundCheckerCountZ - 1));

				// Adjust the outer rays inwards a bit to prevent weird behaviour at platform edges.
				xOffset *= 1 - 0.05f / (HitboxWidth / 2.0f);
				zOffset *= 1 - 0.05f / (HitboxDepth / 2.0f);

				// Position the rays in a 3x3 grid at hip height
				ray.Transform = new Transform3D(Basis.Identity, yOffset + xOffset + zOffset);
				ray.TargetPosition = -Basis.Y * (HipHeight + MaxStairLength) + -Basis.Y * GroundCheckerEpsilon;

				_groundCheckers[x][z] = ray;

				AddChild(ray);
			}
		}

		_torsoCollisionBox = new BoxShape3D();
		_torsoCollisionBox.Size = new Vector3(HitboxWidth, HitboxHeight, HitboxDepth);

		_collisionShape = new CollisionShape3D();
		_collisionShape.Shape = _torsoCollisionBox;
		_collisionShape.Transform = new Transform3D(Basis.Identity, WorldYVector * HipHeight / 2.0f);
		AddChild(_collisionShape);

		_camera = new HumanoidCamera();
		_camera.Subject = this;
		AddChild(_camera);
	}

	// Movement info
	private const float GroundAcceleration = 800f;
	private const float AirAcceleration = 150f;
	private const float MaxSpeed = 16f;
	private const float JumpPower = 55f;
	private const float MaxSlope = 89f;

	private const double CoyoteTime = 0.1f;
	private double _coyoteTick;
	private GroundingState _grounding = GroundingState.Airborne;

	public override void _PhysicsProcess(double delta)
	{
		ProcessGroundCheckers();
		ApplyUprightForce();
		
		if (_grounding == GroundingState.Coyote)
		{
			_coyoteTick -= delta;

			if (_coyoteTick <= 0)
				_grounding = GroundingState.Airborne;
		}
		
		Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
		Vector3 direction = (WorldBasis.Rotated(Vector3.Up, _camera.Rotation.Y) * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		
		if (_camera.CameraLocked)
			SnapToCamera();
		else
			RotateToDirection(direction);
		
		Vector3 targetMovementVector = direction * MaxSpeed;

		if (_grounding == GroundingState.Grounded)
			ApplyAcceleration(targetMovementVector, GroundAcceleration);
		else if (_grounding is GroundingState.Coyote or GroundingState.Airborne)
			ApplyAcceleration(targetMovementVector, AirAcceleration);

		if (Input.IsActionPressed("jump") && _grounding is GroundingState.Grounded or GroundingState.Coyote)
		{
			Jump();
		}
	}

	private void ProcessGroundCheckers() 
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

		// We're considered touching the ground if our legs are colliding with the ground and we have no upwards velocity.
		bool touchingGround = medianLength < HipHeight + 0.05 && (LinearVelocity.Y < 5 || _grounding == GroundingState.Grounded);

		if (!touchingGround) {
			if (_grounding == GroundingState.Grounded)
			{
				_grounding = GroundingState.Coyote;
				_coyoteTick = CoyoteTime;
			}
			
			return;
		}

		_grounding = GroundingState.Grounded;

		// Counteract gravity and adjust our legs to the floor.
		ApplyCentralForce(-GetGravity() * Mass);
		SetAxisVelocity(PlayerYVector * (HipHeight - medianLength) * 20);
	}

	private void Jump()
	{
		SetAxisVelocity(WorldYVector * JumpPower);
		_grounding = GroundingState.Airborne;
	}

	private void RotateToDirection(Vector3 target)
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

	private void SnapToCamera()
	{
		Vector3 currentRotation = Rotation;
		currentRotation.Y = _camera.Rotation.Y;
		Rotation = currentRotation;
	}

	private void ApplyAcceleration(Vector3 target, float acceleration)
	{
		Vector3 correctionVector = target - new Vector3(LinearVelocity.X, 0, LinearVelocity.Z);

		float length = MathF.Min(acceleration, 100 * correctionVector.Length());

		correctionVector = correctionVector.Normalized() * length;
		
		ApplyCentralForce(correctionVector);
	}

	private void ApplyUprightForce()
	{
		Vector3 cross = PlayerYVector.Cross(WorldYVector).Normalized();

		float angle = PlayerYVector.AngleTo(WorldYVector);
		
		SetAngularVelocity(cross * angle * 50);
	}
}