using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsBody : MonoBehaviour
{
    public float mass = 1f;
    public Vector3 velocity;
    public float angularVelocity = 0f;
    public float friction = 0.1f;
    public float gravity = -9.81f;
    public float elasticity = 0.5f;

    private SphereColliderCustom sphereCollider;
    private LevelLoader endLevel;

    private Vector3 groundNormalVector = Vector3.up;
    private float collisionFactor = 0.5f;
    private float minAirHeight = 1.0f;
    private float dragCoefficient = 0.47f; 
    private float airDensity = 1.2f; // kg/m^3, densidad del aire
    private float crossSectionalArea = 0.01f; 

    void Start()
    {
        sphereCollider = GetComponent<SphereColliderCustom>();
        endLevel = FindObjectOfType<LevelLoader>();
    }

    void FixedUpdate()
    {
        ApplyGravity();
        ApplyAirResistance();
        HandleCollisions();
        ApplyRollingResistance();
        ApplyMovement();
    }

    void ApplyGravity()
    {
        Vector3 gravityDirection = groundNormalVector.normalized;
        float theta = Vector3.Angle(Vector3.down, gravityDirection) * Mathf.Deg2Rad;

        // Calcula las componentes paralela y normal de la gravedad
        Vector3 gravityForceParallel = mass * Mathf.Abs(gravity) * Mathf.Sin(theta) * -gravityDirection;
        Vector3 gravityForceNormal = mass * Mathf.Abs(gravity) * Mathf.Cos(theta) * -gravityDirection;

        // Aplica solo la componente paralela como fuerza
        velocity += gravityForceParallel * Time.fixedDeltaTime / mass;
    }

    void ApplyAirResistance()
    {
        if (transform.position.y > minAirHeight)
        {
            float speed = velocity.magnitude;
            float dragForceMagnitude = 0.5f * airDensity * speed * speed * crossSectionalArea * dragCoefficient;
            Vector3 dragForce = -dragForceMagnitude * velocity.normalized;
            velocity += dragForce * Time.fixedDeltaTime / mass;
        }
    }

    void HandleCollisions()
    {
        var walls = WallManager.instance?.Walls;
        if (walls == null) return;

        foreach (var wallObj in walls)
        {
            foreach (Wall wall in wallObj.GetWalls())
            {
                if (sphereCollider.CollidesWith(wall, wallObj.transform, out Vector3 normal, out Vector3 contactPoint, out float penetration))
                {
                    float vDotN = Vector3.Dot(velocity, normal);
                    if (vDotN < 0)
                    {
                        // Refleja la velocidad segun elasticidad
                        velocity -= (1 + elasticity) * vDotN * normal;
                        endLevel?.RegisterStickContact();
                    }

                    // Corrige la posicion para evitar que la bola se hunda en la pared
                    transform.position += normal * penetration;

                    // Solo se resuelve una colision por frame
                    break;
                }
            }
        }
    }

    void ApplyRollingResistance()
    {
        if (velocity.magnitude > 0)
        {
            float normalForce = mass * Mathf.Abs(gravity);
            float rollingResistance = -friction * normalForce * sphereCollider.radius;
            float momentumOfInertia = (2.0f / 5.0f) * mass * Mathf.Pow(sphereCollider.radius, 2);
            float angularAccel = rollingResistance / momentumOfInertia;

            // Ajusta la velocidad lineal para cumplir con ω = v/r
            float angularSpeed = angularVelocity * sphereCollider.radius;
            float linearSpeed = velocity.magnitude;

            // Sincroniza la velocidad lineal con la velocidad angular
            angularVelocity = linearSpeed / sphereCollider.radius;

            // Reduce la velocidad gradualmente por resistencia al rodamiento
            velocity -= velocity.normalized * (angularAccel * Time.fixedDeltaTime * sphereCollider.radius);
        }
    }

    void ApplyMovement()
    {
        transform.position += velocity * Time.fixedDeltaTime;
    }
}
