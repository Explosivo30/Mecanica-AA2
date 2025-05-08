using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereColliderCustom : MonoBehaviour
{
    public float radius = 0.5f;
    public Vector3 Center => transform.position;

    // Array estático para las normales locales canónicas de un cubo.
    // Necesario para identificar la cara específica del WallCollider.
    private static readonly Vector3[] cubeLocalNormals = new Vector3[]
    {
        Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back
    };

    // Necesitamos pasar el Transform del objeto WallCollider para derivar la geometría de la cara.
    public bool CollidesWith(Wall wall, Transform wallObjectTransform, out Vector3 collisionNormalOut, out Vector3 contactPointOut, out float penetrationOut)
    {
        Vector3 sphereCenter = Center;
        collisionNormalOut = Vector3.zero;
        contactPointOut = Vector3.zero;
        penetrationOut = 0f;

        // 1. Distancia del centro de la esfera al plano infinito de la pared
        float signedDistanceToPlane = Vector3.Dot(wall.normal, sphereCenter) - wall.distance;

        if (Mathf.Abs(signedDistanceToPlane) > radius + wall.epsilon)
        {
            return false; // Demasiado lejos del plano
        }

        // --- Inicio de la lógica para reconstruir la geometría de la cara ---
        Vector3 identifiedLocalNormalForFace = Vector3.zero;
        bool foundLocalNormal = false;

        // Identificar cuál de las 6 normales locales del WallCollider corresponde a wall.normal
        for (int i = 0; i < cubeLocalNormals.Length; i++)
        {
            // Transformar la normal local canónica al espacio mundial usando la rotación del WallCollider
            if (Vector3.Dot(wallObjectTransform.TransformDirection(cubeLocalNormals[i]), wall.normal) > 0.999f)
            {
                identifiedLocalNormalForFace = cubeLocalNormals[i];
                foundLocalNormal = true;
                break;
            }
        }

        if (!foundLocalNormal)
        {
            Debug.LogError("SphereColliderCustom: No se pudo identificar la normal local para la cara de la pared. La 'wall.normal' podría no coincidir con ninguna cara transformada del 'wallObjectTransform'.");
            return false; // No se pudo identificar la cara, no se puede proceder.
        }

        // Calcular el centro de la cara en el espacio mundial
        // (localNormal * 0.5f es un punto en el centro de la cara de un cubo unitario local)
        Vector3 worldFaceCenter = wallObjectTransform.TransformPoint(identifiedLocalNormalForFace * 0.5f);

        // Determinar las tangentes mundiales y las semiextensiones de la cara
        Vector3 worldTangentU, worldTangentV;
        float faceHalfExtentU, faceHalfExtentV;
        Vector3 wallLossyScale = wallObjectTransform.lossyScale; // Escala mundial del WallCollider

        if (Mathf.Abs(identifiedLocalNormalForFace.x) > 0.9f) // Cara cuya normal local es (+/-X)
        {
            // Tangente U local es el eje Y local, Tangente V local es el eje Z local
            worldTangentU = wallObjectTransform.TransformDirection(Vector3.up).normalized;
            // Para V, es más robusto usar Cross product para asegurar ortogonalidad con la normal mundial de la cara
            worldTangentV = Vector3.Cross(wall.normal, worldTangentU).normalized;
            faceHalfExtentU = wallLossyScale.y * 0.5f;
            faceHalfExtentV = wallLossyScale.z * 0.5f;
        }
        else if (Mathf.Abs(identifiedLocalNormalForFace.y) > 0.9f) // Cara cuya normal local es (+/-Y)
        {
            // Tangente U local es el eje X local, Tangente V local es el eje Z local
            worldTangentU = wallObjectTransform.TransformDirection(Vector3.right).normalized;
            worldTangentV = Vector3.Cross(wall.normal, worldTangentU).normalized;
            faceHalfExtentU = wallLossyScale.x * 0.5f;
            faceHalfExtentV = wallLossyScale.z * 0.5f;
        }
        else // Cara cuya normal local es (+/-Z)
        {
            // Tangente U local es el eje X local, Tangente V local es el eje Y local
            worldTangentU = wallObjectTransform.TransformDirection(Vector3.right).normalized;
            worldTangentV = Vector3.Cross(wall.normal, worldTangentU).normalized;
            faceHalfExtentU = wallLossyScale.x * 0.5f;
            faceHalfExtentV = wallLossyScale.y * 0.5f;
        }
        // --- Fin de la lógica para reconstruir la geometría de la cara ---

        // 3. Proyectar el centro de la esfera sobre el plano infinito de la pared
        Vector3 pointOnPlane = sphereCenter - wall.normal * signedDistanceToPlane;

        // 4. Transformar el vector (desde el centro de la cara reconstruido hasta pointOnPlane)
        // a las coordenadas de la cara.
        Vector3 vectorFromFaceCenterToPointOnPlane = pointOnPlane - worldFaceCenter;
        float uCoordinate = Vector3.Dot(vectorFromFaceCenterToPointOnPlane, worldTangentU);
        float vCoordinate = Vector3.Dot(vectorFromFaceCenterToPointOnPlane, worldTangentV);

        // 5. Encontrar el punto más cercano en el rectángulo de la cara
        float clampedU = Mathf.Clamp(uCoordinate, -faceHalfExtentU, faceHalfExtentU);
        float clampedV = Mathf.Clamp(vCoordinate, -faceHalfExtentV, faceHalfExtentV);
        Vector3 closestPointOnFaceRectangle = worldFaceCenter + worldTangentU * clampedU + worldTangentV * clampedV;

        // 6. Prueba de colisión final entre la esfera y closestPointOnFaceRectangle
        Vector3 vectorFromSphereCenterToClosestPoint = closestPointOnFaceRectangle - sphereCenter;
        float distanceSqToClosestPoint = vectorFromSphereCenterToClosestPoint.sqrMagnitude;

        if (distanceSqToClosestPoint <= (radius + wall.epsilon) * (radius + wall.epsilon))
        {
            float distanceToClosestPoint = Mathf.Sqrt(distanceSqToClosestPoint);
            if (distanceToClosestPoint < 1e-5f)
            {
                collisionNormalOut = -wall.normal; // Centro de la esfera en la superficie, usa normal de pared
            }
            else
            {
                collisionNormalOut = (sphereCenter - closestPointOnFaceRectangle).normalized;
            }
            contactPointOut = closestPointOnFaceRectangle;
            penetrationOut = radius - distanceToClosestPoint;
            if (penetrationOut < 0) penetrationOut = 0;

            return true;
        }
        return false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        // Asegúrate de que el script está en un GameObject con un transform para que Center funcione.
        if (transform != null)
        {
            Gizmos.DrawWireSphere(Center, radius);
        }
    }
}