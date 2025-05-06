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
            // Proyectar el punto al plano
            Vector3 projected = center - wall.normal * d;

            // Transformar el punto proyectado al espacio local de la pared
            Vector3 localPoint = wallTransform.InverseTransformPoint(projected);

            // Ahora verificar si está dentro del área escalada (p.ej. dentro del rectángulo)
            Vector3 halfSize = wallTransform.localScale * 0.5f;

            if (Mathf.Abs(localPoint.x) <= halfSize.x && Mathf.Abs(localPoint.y) <= halfSize.y && Mathf.Abs(localPoint.z) <= halfSize.z)
            {
                collisionNormal = d > 0 ? wall.normal : -wall.normal;
                contactPoint = projected;
                penetration = radius - Mathf.Abs(d);
                return true;
            }
        }

        collisionNormal = Vector3.zero;
        contactPoint = Vector3.zero;
        penetration = 0f;
        return false;
    }
}
