using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CameraFollow : MonoBehaviour
{
    private Transform parentToFollow; 
    public Vector3 offsetPosition;

    private Transform mainCamera;

    public float cameraAngle;

    void Start()
    {
        parentToFollow = FindAnyObjectByType<PlayerController>().transform;

        transform.position = parentToFollow.position;
        mainCamera = GetComponentInChildren<Camera>().transform;
        mainCamera.position = transform.position + offsetPosition;
    }

    void LateUpdate()
    {
        transform.position = parentToFollow.position;
        if (Input.GetKey(KeyCode.A))
        {
            transform.RotateAround(transform.position, new Vector3(0, 1, 0), cameraAngle);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.RotateAround(transform.position, new Vector3(0, 1, 0), -cameraAngle);
        }
    }
}

