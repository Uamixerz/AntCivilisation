using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraMovement : MonoBehaviour
{
    private Vector3 dragOrigin;
    private bool isDragging;

    [Header("Vertical|Horizontal move")]
    public float speedCamera = 1f;
    public Transform groundScale;
    private float maxHorizontal;
    private float maxVertical;
    private float minHorizontal;
    private float minVertical;

    [Header("Zoom move")]
    public float zoomSpeed = 8.0f;
    public float minZoomSize = 1f;
    public float maxZoomSize = 34.0f;

    private void Start()
    {
        InitializeCameraLimits();
    }

    private void InitializeCameraLimits()
    {
        maxHorizontal = groundScale.localScale.x / 2;
        maxVertical = groundScale.localScale.y / 2;
        minHorizontal = maxHorizontal;
        minVertical = maxVertical;
    }

    private void Update()
    {
        ZoomCamera();
        DragCamera();
    }

    private void ZoomCamera()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float newSize = Camera.main.orthographicSize - scroll * zoomSpeed;
            if (newSize >= minZoomSize && newSize <= maxZoomSize)
            {
                UpdateCameraParameters(newSize);
                Camera.main.orthographicSize = Mathf.Clamp(newSize, minZoomSize, maxZoomSize);
            }
        }
    }

    private void UpdateCameraParameters(float newSize)
    {
        speedCamera = Mathf.Clamp(newSize / 5, 0.01f, 8f);
        maxHorizontal = minHorizontal - newSize / 2;
        maxVertical = minVertical - newSize / 2;
    }

    private void DragCamera()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            dragOrigin = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 currentMousePos = Input.mousePosition;
            Vector3 difference = currentMousePos - dragOrigin;
            Vector3 move = new Vector3(-difference.x, -difference.y, 0);
            Vector3 localDisplacement = transform.TransformDirection(move * Time.deltaTime * speedCamera);

            Vector3 newLocalPos = transform.localPosition + localDisplacement;

            float clampedX = Mathf.Clamp(newLocalPos.x, -maxHorizontal, maxHorizontal);
            float clampedY = Mathf.Clamp(newLocalPos.y, -maxVertical, maxVertical);

            transform.localPosition = new Vector3(clampedX, clampedY, transform.localPosition.z);

            dragOrigin = currentMousePos;
        }
    }
}
