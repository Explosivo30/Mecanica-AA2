using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCollider : MonoBehaviour
{
    public float epsilon = 0.2f;
    public float elasticity = 0.5f;
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

    private void Start()
    {
        if (WallManager.instance != null)
        {
            WallManager.instance.RegisterWall(this);
        }

        // Automatically add a BoxCollider if one doesn't exist
        if (GetComponent<Collider>() == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = false;
        }
    }

    private void OnDisable()
    {
        if (WallManager.instance != null)
        {
            WallManager.instance.UnregisterWall(this);
        }
    }

    // Genera todas las paredes del cubo dinámicamente
   public List<Wall> GetWalls()
{
        List<Wall> walls = new List<Wall>();

        // El transform del objeto contiene su posición, rotación y escala en el mundo.
        Transform objectTransform = transform;

        for (int i = 0; i < localNormals.Length; i++)
        {
            // 1. Transformar la normal de la cara del espacio local al espacio mundial.
            // TransformDirection aplica solo la rotación del objeto, lo cual es correcto para las normales.
            // La normal resultante estará en coordenadas mundiales y será unitaria si localNormals[i] es unitaria.
            Vector3 worldNormal = objectTransform.TransformDirection(localNormals[i]);

            // 2. Calcular un punto en la superficie de esta cara en el espacio mundial.
            // localNormals[i] * 0.5f nos da un punto en el centro de la cara de un cubo unitario (tamaño 1x1x1)
            // en el espacio local del objeto. Por ejemplo, para localNormals[i] = Vector3.right (1,0,0),
            // el localPointOnFace es (0.5, 0, 0).
            Vector3 localPointOnFace = localNormals[i] * 0.5f;

            // TransformPoint aplica la escala, rotación y traslación del objeto para convertir
            // el punto local al espacio mundial.
            // Si el objeto tiene una escala de (sx, sy, sz), localPointOnFace (0.5,0,0) se convertirá en un punto
            // que está a 0.5 * sx unidades a lo largo del eje X local rotado, desde el pivote del objeto.
            Vector3 worldPointOnFace = objectTransform.TransformPoint(localPointOnFace);

            // 3. Calcular la distancia 'd' del plano desde el origen del mundo.
            // La ecuación de un plano es: normal_mundial · punto_en_plano_mundial = d
            // Esta 'd' es la proyección del vector de posición worldPointOnFace sobre la worldNormal.
            float distanceToOrigin = Vector3.Dot(worldNormal, worldPointOnFace);

            walls.Add(new Wall(worldNormal, distanceToOrigin, epsilon, friction, elasticity));
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
    public float elasticity;

    public Wall(Vector3 normal, float distance, float epsilon, float friction, float elasticity)
    {
        this.normal = normal.normalized;
        this.distance = distance;
        this.epsilon = epsilon;
        this.friction = friction;
        this.elasticity = elasticity;
    }

}
