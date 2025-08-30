using Godot;

namespace Jomolith.Scripts;

public partial class CharacterRigidBody : CharacterBody3D
{
	[Export] public float Mass = 1.0f;

	private Vector3 currentVelocity;
	private Vector3 currentRotationalVelocity;

	private Vector3 continuousForce;
	private Vector3 continuousTorque;

	public override void _PhysicsProcess(double delta)
	{
		currentVelocity += continuousForce * (float)delta / Mass;
		currentRotationalVelocity += continuousTorque * (float)delta / Mass;
		
		currentVelocity += GetGravity() * (float)delta;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		KinematicCollision3D collisions = MoveAndCollide(currentVelocity);
	}

	public void ApplyForceImpulse(Vector3 force)
	{
		currentVelocity += force / Mass;
	}

	public void ApplyTorqueImpulse(Vector3 torque)
	{
		currentRotationalVelocity += torque / Mass;
	}

	public void ApplyContinuousForce(Vector3 force)
	{
		continuousForce += force;
	}

	public void ApplyContinuousTorque(Vector3 torque)
	{
		continuousTorque += torque;
	}

	private void ResolveCollisions(KinematicCollision3D collisions)
	{
		
	}
}