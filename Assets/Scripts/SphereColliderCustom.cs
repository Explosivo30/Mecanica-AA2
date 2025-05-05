using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereColliderCustom : MonoBehaviour
{
    public float radius = 0.5f;

    public Vector3 Center => transform.position;

    public bool CollidesWith(Wall wall, out Vector3 collisionNormal, out Vector3 contactPoint, out float penetration)
    {
        float d = Vector3.Dot(wall.normal, Center) - wall.distance;

        if (Mathf.Abs(d) < radius)
        {
            collisionNormal = d > 0 ? wall.normal : -wall.normal;
            contactPoint = Center - collisionNormal * d;
            penetration = radius - Mathf.Abs(d);
            return true;
        }

        collisionNormal = Vector3.zero;
        contactPoint = Vector3.zero;
        penetration = 0;
        return false;
    }
}
