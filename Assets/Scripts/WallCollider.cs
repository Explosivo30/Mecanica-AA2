using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCollider : MonoBehaviour
{
    public Vector3 localNormal = Vector3.right;
    public float epsilon = 0.2f;

    public Vector3 Normal => transform.rotation * localNormal.normalized;
    public float Distance => Vector3.Dot(Normal, transform.position);

    public Wall ToWall()
    {
        return new Wall(Normal, Distance, epsilon);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector3 worldNormal = Normal;
        Vector3 center = transform.position;

        // Draw normal line
        Gizmos.DrawLine(center, center + worldNormal * 1.5f);

        // Draw aligned wall box (rotate to match the custom local normal)
        // Build a rotation that aligns Z+ with our wall normal
        Quaternion lookRotation = Quaternion.LookRotation(worldNormal);
        Matrix4x4 matrix = Matrix4x4.TRS(center, lookRotation, transform.localScale);
        Gizmos.matrix = matrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        Gizmos.matrix = Matrix4x4.identity;
    }
}

public struct Wall
{
    public Vector3 normal;
    public float distance;
    public float epsilon;

    public Wall(Vector3 normal, float distance, float epsilon)
    {
        this.normal = normal.normalized;
        this.distance = distance;
        this.epsilon = epsilon;
    }

}
