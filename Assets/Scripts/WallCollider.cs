using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCollider : MonoBehaviour
{
    public Vector3 localNormal = Vector3.right;
    public float epsilon = 0.2f;
    public float friction = 0.2f;

    private static readonly Vector3[] localNormals = new Vector3[]
  {
        Vector3.right,
        Vector3.left,
        Vector3.up,
        Vector3.down,
        Vector3.forward,
        Vector3.back
  };

    private void OnEnable()
    {
        if(WallManager.instance != null)
        {
            WallManager.instance.RegisterWall(this);
        }
    }

    private void OnDisable()
    {
        if(WallManager.instance != null)
        {
            WallManager.instance.UnregisterWall(this);
        }
    }

    // Genera todas las paredes del cubo dinámicamente
    public List<Wall> GetWalls()
    {
        List<Wall> walls = new List<Wall>();

        for (int i = 0; i < localNormals.Length; i++)
        {
            Vector3 normal = transform.rotation * localNormals[i];
            Vector3 halfExtents = transform.localScale * 0.5f;
            float offset = Vector3.Dot(normal, transform.position + Vector3.Scale(localNormals[i], halfExtents));
            walls.Add(new Wall(normal, offset, epsilon, friction));
        }

        return walls;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 center = transform.position;

        // Dibuja el cubo de colisión
        Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, transform.localScale);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        Gizmos.matrix = Matrix4x4.identity;

        // Dibuja las 6 normales (una por cara)
        foreach (var localNormal in localNormals)
        {
            Vector3 normal = transform.rotation * localNormal;
            Vector3 halfExtents = transform.localScale * 0.5f;
            Vector3 faceCenter = center + Vector3.Scale(localNormal, halfExtents);

            Gizmos.DrawLine(faceCenter, faceCenter + normal * 0.5f);
        }
    }
}

public struct Wall
{
    public Vector3 normal;
    public float distance;
    public float epsilon;
    public float friction;

    public Wall(Vector3 normal, float distance, float epsilon, float friction)
    {
        this.normal = normal.normalized;
        this.distance = distance;
        this.epsilon = epsilon;
        this.friction = friction;
    }

}
