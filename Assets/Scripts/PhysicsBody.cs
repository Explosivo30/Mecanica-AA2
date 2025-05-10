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

    private float minAirHeight = 1.0f;
    private float dragCoefficient = 0.47f; 
    private float airDensity = 1.2f; // kg/m^3, densidad del aire
    private float crossSectionalArea = 0.01f; 

    void Start()
    {
        sphereCollider = GetComponent<SphereColliderCustom>();
    }

    void Update()
    {
        ApplyGravity();
        ApplyAirResistance();
        ApplyRollingResistance();
        HandleCollisions();
        ApplyMovement();
    }

    void ApplyGravity()
    {
        Vector3 gravityForce = new Vector3(0, mass * gravity, 0);
        velocity += (gravityForce * Time.deltaTime) / mass;
    }

    void ApplyAirResistance()
    {
        if (transform.position.y > minAirHeight)
        {
            float speed = velocity.magnitude;
            float dragForceMagnitude = 0.5f * airDensity * speed * speed * crossSectionalArea * dragCoefficient;
            Vector3 dragForce = -dragForceMagnitude * velocity.normalized;
            velocity += dragForce * Time.deltaTime / mass;
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
            velocity += velocity.normalized * (angularAccel * Time.deltaTime * sphereCollider.radius);
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
                    friction = wall.friction;
                    if(elasticity != wall.elasticity)
                    {
                        Debug.Log(wall.elasticity);
                    }
                    elasticity = wall.elasticity;
                    float vDotN = Vector3.Dot(velocity, normal);
                    if (vDotN < 0)
                    {
                        // Refleja la velocidad segun elasticidad
                        velocity -= (1 + elasticity) * vDotN * normal;
                    }

                    // Corrige la posicion para evitar que la bola se hunda en la pared
                    transform.position += normal * penetration;

                    // Solo se resuelve una colision por frame
                    break;
                }
            }
        }
    }

    void ApplyMovement()
    {
        transform.position += velocity * Time.deltaTime;
    }
}
