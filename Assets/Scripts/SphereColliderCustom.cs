using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereColliderCustom : MonoBehaviour
{
    public float radius = 0.5f;

    public Vector3 Center => transform.position;

    public bool CollidesWith(Wall wall, Transform wallTransform, out Vector3 collisionNormal, out Vector3 contactPoint, out float penetration)
    {
        Vector3 center = Center;

        // Distancia de la esfera al plano de la pared
        float d = Vector3.Dot(wall.normal, center) - wall.distance;

        if (Mathf.Abs(d) < radius + wall.epsilon)
        {
            // Punto más cercano al plano
            Vector3 projected = center - wall.normal * d;

            // Obtenemos el centro de la cara
            Vector3 halfExtents = Vector3.Scale(wallTransform.lossyScale, wall.normal.normalized) * 0.5f;
            Vector3 faceCenter = wallTransform.position + wall.normal.normalized * Vector3.Dot(halfExtents, wall.normal.normalized);

            // Base ortonormal sobre la cara
            Vector3 tangent1;
            if (Mathf.Abs(Vector3.Dot(wall.normal, Vector3.up)) < 0.99f)
                tangent1 = Vector3.Cross(wall.normal, Vector3.up);
            else
                tangent1 = Vector3.Cross(wall.normal, Vector3.right);
            tangent1.Normalize();
            Vector3 tangent2 = Vector3.Cross(wall.normal, tangent1).normalized;

            // Coordenadas del punto proyectado en la cara
            Vector3 toProjected = projected - faceCenter;
            float u = Vector3.Dot(toProjected, tangent1);
            float v = Vector3.Dot(toProjected, tangent2);

            // Tamaños reales de la cara (según orientación y escala)
            Vector3 worldScale = wallTransform.lossyScale;
            float width = Mathf.Abs(Vector3.Dot(worldScale, tangent1));
            float height = Mathf.Abs(Vector3.Dot(worldScale, tangent2));

            if (Mathf.Abs(u) <= width * 0.5f && Mathf.Abs(v) <= height * 0.5f)
            {
                collisionNormal = d > 0 ? wall.normal : -wall.normal;
                contactPoint = projected;
                penetration = radius - Mathf.Abs(d);
                Debug.Log("Touching (accurate scaled)");
                return true;
            }
        }

        collisionNormal = Vector3.zero;
        contactPoint = Vector3.zero;
        penetration = 0f;
        
        return false;
    }
}
