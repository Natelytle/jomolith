using Godot;

namespace Jomolith.Scripts;

public partial class CharacterRigidBody : CharacterBody3D
{
    [Export] public float Mass = 1.0f;
    [Export] public float Restitution = 0.3f; // Bounciness factor
    [Export] public float Friction = 0.5f;
    [Export] public float AngularDamping = 0.98f; // Reduces spinning over time

    public Vector3 LinearVelocity { get; set; }
    public Vector3 AngularVelocity { get; set; }

    private Vector3 _continuousForce;
    private Vector3 _continuousTorque;

    private Vector3 _centerOfMass = Vector3.Zero;

    // Moment of inertia approximation (can be made more sophisticated)
    private float MomentOfInertia => Mass * 0.4f; // Approximation for a sphere

    public override void _PhysicsProcess(double delta)
    {
        AccumulateForce(_continuousForce, delta);
        AccumulateTorque(_continuousTorque, delta);

        _continuousForce = Vector3.Zero;
        _continuousTorque = Vector3.Zero;
        
        LinearVelocity += GetGravity() * (float)delta;

        Velocity = LinearVelocity;

        Rotate(AngularVelocity.Normalized(), AngularVelocity.Length() * (float)delta);
        bool collided = MoveAndSlide();
        
        if (collided)
        {
            Vector3 positionAdjustment = Vector3.Zero;
            Vector3 rotationAdjustment = Vector3.Zero;

            for (int i = 0; i < GetSlideCollisionCount(); i++)
            {
                var collision = GetSlideCollision(i);
                Vector3 normal = collision.GetNormal();
                float depth = collision.GetDepth();
                Vector3 collisionPoint = collision.GetPosition();
                
                // First, we move the player out of the wall
                positionAdjustment += normal * depth;
                
                // Calculate collision response
                HandleCollisionResponse(collision, delta);
            }
            
            // Apply position adjustment if needed
            if (positionAdjustment.Length() > 0.001f)
            {
                GlobalPosition += positionAdjustment * 0.5f; // Reduce to prevent over-correction
            }
        }

        // Apply angular damping
        AngularVelocity *= AngularDamping;
        
        LinearVelocity = Velocity;
    }

    private void HandleCollisionResponse(KinematicCollision3D collision, double delta)
    {
        Vector3 normal = collision.GetNormal();
        Vector3 collisionPoint = collision.GetPosition();
        Vector3 centerOfMassWorld = GlobalPosition + GlobalTransform.Basis * _centerOfMass;
        
        // Vector from center of mass to collision point
        Vector3 r = collisionPoint - centerOfMassWorld;
        
        // Velocity at collision point due to linear + angular motion
        Vector3 velocityAtPoint = LinearVelocity + AngularVelocity.Cross(r);
        
        // Relative velocity along the normal
        float relativeVelocityNormal = velocityAtPoint.Dot(normal);
        
        // Don't resolve if velocities are separating
        if (relativeVelocityNormal > 0) return;
        
        // Calculate impulse scalar
        float impulseScalar = CalculateImpulseScalar(relativeVelocityNormal, normal, r);
        
        // Apply impulse to linear velocity
        Vector3 impulse = impulseScalar * normal;
        LinearVelocity += impulse / Mass;
        
        // Apply impulse to angular velocity (torque = r × impulse)
        Vector3 torqueImpulse = r.Cross(impulse);
        AngularVelocity += torqueImpulse / MomentOfInertia;
        
        // Apply friction
        ApplyFriction(collision, impulseScalar);
    }

    private float CalculateImpulseScalar(float relativeVelocityNormal, Vector3 normal, Vector3 r)
    {
        // Standard impulse calculation for rigid body collision
        float rCrossN = r.Cross(normal).Length();
        float invMassSum = 1.0f / Mass;
        float angularEffect = (rCrossN * rCrossN) / MomentOfInertia;
        
        return -(1 + Restitution) * relativeVelocityNormal / (invMassSum + angularEffect);
    }

    private void ApplyFriction(KinematicCollision3D collision, float normalImpulse)
    {
        Vector3 normal = collision.GetNormal();
        Vector3 collisionPoint = collision.GetPosition();
        Vector3 centerOfMassWorld = GlobalPosition + GlobalTransform.Basis * _centerOfMass;
        Vector3 r = collisionPoint - centerOfMassWorld;
        
        // Get velocity at contact point
        Vector3 velocityAtPoint = LinearVelocity + AngularVelocity.Cross(r);
        
        // Get tangent velocity (perpendicular to normal)
        Vector3 tangentVelocity = velocityAtPoint - velocityAtPoint.Dot(normal) * normal;
        
        if (tangentVelocity.Length() < 0.001f) return;
        
        Vector3 tangentDirection = tangentVelocity.Normalized();
        
        // Calculate friction impulse
        float rCrossT = r.Cross(tangentDirection).Length();
        float invMassSum = 1.0f / Mass;
        float angularEffect = (rCrossT * rCrossT) / MomentOfInertia;
        
        float frictionImpulse = -tangentVelocity.Length() / (invMassSum + angularEffect);
        
        // Coulomb friction limit
        float maxFrictionImpulse = Friction * Mathf.Abs(normalImpulse);
        frictionImpulse = Mathf.Clamp(frictionImpulse, -maxFrictionImpulse, maxFrictionImpulse);
        
        Vector3 frictionForce = frictionImpulse * tangentDirection;
        
        // Apply friction to velocities
        LinearVelocity += frictionForce / Mass;
        AngularVelocity += r.Cross(frictionForce) / MomentOfInertia;
    }

    public void ApplyCentralImpulse(Vector3 force)
    {
        AccumulateForce(force, 1);
    }

    public void ApplyTorqueImpulse(Vector3 torque)
    {
        AccumulateTorque(torque, 1);
    }

    public void ApplyCentralForce(Vector3 force)
    {
        _continuousForce += force;
    }

    public void ApplyTorque(Vector3 torque)
    {
        _continuousTorque += torque;
    }

    private void AccumulateForce(Vector3 force, double delta)
    {
        LinearVelocity += force * (float)delta / Mass;
    }
    
    private void AccumulateTorque(Vector3 torque, double delta)
    {
        AngularVelocity += torque * (float)delta / MomentOfInertia;
    }

    // Helper method to apply force at a specific point (useful for explosions, etc.)
    public void ApplyForceAtPoint(Vector3 force, Vector3 worldPoint)
    {
        Vector3 centerOfMassWorld = GlobalPosition + GlobalTransform.Basis * _centerOfMass;
        Vector3 r = worldPoint - centerOfMassWorld;
        
        // Apply linear force
        ApplyCentralImpulse(force);
        
        // Apply torque from the offset
        Vector3 torque = r.Cross(force);
        ApplyTorqueImpulse(torque);
    }
}