using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PhysicsBody ball;
    public float maxForce = 10f;
    public float forceMultiplier = 1f;
    public LineRenderer aimLine;

    private Vector3 startDragPosition;
    private Vector3 endDragPosition;
    private bool isDragging = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag();
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            UpdateDrag();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndDrag();
        }
    }

    void StartDrag()
    {
        isDragging = true;
        startDragPosition = GetMouseWorldPosition();
        aimLine.positionCount = 2;
        aimLine.enabled = true;
    }

    void UpdateDrag()
    {
        endDragPosition = GetMouseWorldPosition();
        Vector3 direction = startDragPosition - endDragPosition;
        float forceMagnitude = Mathf.Clamp(direction.magnitude * forceMultiplier, 0, maxForce);
        Vector3 force = direction.normalized * forceMagnitude;

        // Actualiza la linea de dirección
        aimLine.SetPosition(0, ball.transform.position);
        aimLine.SetPosition(1, ball.transform.position + force);
    }

    void EndDrag()
    {
        isDragging = false;
        aimLine.enabled = false;

        Vector3 direction = startDragPosition - endDragPosition;
        float forceMagnitude = Mathf.Clamp(direction.magnitude * forceMultiplier, 0, maxForce);
        Vector3 force = direction.normalized * forceMagnitude;

        // Aplica la fuerza a la bola
        ball.velocity += force;
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.point;
        }
        return Vector3.zero;
    }
}
