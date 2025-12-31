using System;
using UnityEngine;

public class CircleCode : MonoBehaviour
{
    LineRenderer circleRenderer;
    public Transform centerPoint;
    public float radiusX= 0.1f;
    public float radiusY = 0.1f;
    public int steps = 32;
    [HideInInspector] public float irisScaleX = 1f;
    [HideInInspector] public float irisScaleY = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        circleRenderer = GetComponent<LineRenderer>();
        circleRenderer.useWorldSpace = false;
        circleRenderer.startWidth = 0.05f;
        circleRenderer.endWidth = 0.05f;
    }

    // Update is called once per frame
    void Update()
    {   
        if (centerPoint == null)
        {
            return;
        }

        circleRenderer.positionCount = steps + 1;
        Vector3 center = centerPoint.localPosition;

        float scaledX = radiusX * irisScaleX;
        float scaledY = radiusY * irisScaleY;

        for (int currentStep = 0; currentStep <= steps; currentStep++)
        {
            float circumferenceProgress = (float)currentStep/steps;
            float currentAngle = circumferenceProgress * 2f * Mathf.PI;

            float x = Mathf.Cos(currentAngle) * scaledX;
            float y = Mathf.Sin(currentAngle) * scaledY;

            circleRenderer.SetPosition(currentStep, center + new Vector3(x, y, 0f));
        }
    }
}
