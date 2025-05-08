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
        direction.y = 0;  

        float forceMagnitude = Mathf.Clamp(direction.magnitude * forceMultiplier, 0, maxForce);
        Vector3 force = direction.normalized * forceMagnitude;

        
        aimLine.SetPosition(0, ball.transform.position);
        aimLine.SetPosition(1, ball.transform.position + force);

        Debug.Log("Dragging... Force: " + force);
    }


    void EndDrag()
    {
        isDragging = false;
        aimLine.enabled = false;

        Vector3 direction = startDragPosition - endDragPosition;
        direction.y = 0;  // Ignora la componente vertical

        float forceMagnitude = Mathf.Clamp(direction.magnitude * forceMultiplier, 0, maxForce);
        Vector3 force = direction.normalized * forceMagnitude;

        // Aplica la fuerza
        ball.velocity += force;
        Debug.Log("End Drag, Applied Force: " + force);
    }


    Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);
            return hit.point;
        }
        Debug.LogWarning("No ground detected!");
        return Vector3.zero;
    }

}
