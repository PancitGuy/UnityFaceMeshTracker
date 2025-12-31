using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using JetBrains.Annotations;
using NUnit.Framework.Constraints;
using UnityEngine.InputSystem.Processors;

public class FaceMeshTracker : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public UDPReceive udpReceive;
    public GameObject[] facePoints;
    public Transform faceRoot;
    public float depthScale = 5.0f;

    public bool ShowSpheres = true;

    private float smoothDepth = 0f;
    public float depthMultiplier = 1.0f;
    public float smoothFactor = 15f;

    public CircleCode leftIris;
    public CircleCode rightIris;

    public float imageWidth = 1280f;
    public float imageHeight = 720f;
    
    public float zNeutralOffset = 0f;
    public bool autoCalibrateZ = true;
    void Start()
    {
        //allows user to toggle visibility with spheres, looping
        //over the game object array to disable rendering in gameview
        //one by one
        if (!ShowSpheres)
        {
            for (int i = 0; i < facePoints.Length; i++)
            {
                if (facePoints[i] != null)
                {
                    facePoints[i].GetComponent<Renderer>().enabled = false;
                }
            }
        }
    }

    //Update is called once per frame. Uses TryParse to ensure safety
    //with accessing values and conversion to float-types. Lerp is also
    //used to ensure smooth transitions between key positions of the points
    //and the parent FaceMesh, although sync with the original videos may
    //vary.
    void Update()
    {
        string data = udpReceive.receivedData;

        if (!string.IsNullOrEmpty(data))
        {
            string[] components = data.Split('|');

            if(components.Length < 3)
            {
                return;
            }

            //initializes the width and height based on differing
            //video widths and heights.
            if (float.TryParse(components[0], out float width))
            {
                imageWidth = width;
            }

            if (float.TryParse(components[1], out float height))
            {
                imageHeight = height;
            }

            string landmark = components[2].Trim('[', ']');
            string[] points = landmark.Split(',');
            //print(data);

            //adjusts the FaceMesh parent's coordinates based on their
            //landmark values. Z has a higher landmark value to properly
            //coordinate their z_position. x and z are divided by the width of
            //the video, y divided by the height, to normalize their values(They were multiplied
            //by said values in FaceMeshCode when sending their data coords)
            //Function will not happen if points.Length is less than 6, or if
            //TryParses fail to return the converted float value.   
            //Note: 152 is the landmark value for the nose tip, using it as a
            //reference point for face depth
            if (points.Length >= 6 &&
                float.TryParse(points[3], out float headX) &&
                float.TryParse(points[4], out float headY) &&
                float.TryParse(points[152 * 3 + 2], out float headZ))
            {
                float nx = headX/imageWidth * 2f - 1f;
                float ny = headY/imageHeight * 2f - 1f;
                headZ = -headZ/imageWidth;
                ny = -ny;

                //calibrating zRange to ensure face moves both in front and
                //behind its calibrated center point
                if (autoCalibrateZ)
                {
                    zNeutralOffset = headZ;
                    autoCalibrateZ = false;
                }

                //increases the range where face can move up and down, left and right
                float movementScale = 5.0f;
                float centeredZ = headZ - zNeutralOffset;
                smoothDepth = Mathf.Lerp(smoothDepth, centeredZ, Time.deltaTime * 10f);
                float smoothedZ = Mathf.Lerp(faceRoot.localPosition.z, smoothDepth, Time.deltaTime * smoothFactor);
                
                Vector3 rootPosition = new Vector3(nx * movementScale, ny * movementScale, smoothedZ * depthMultiplier);

                //Landmark values for eye adjustments
                Vector3 leftOuterEye = facePoints[33].transform.localPosition;
                Vector3 leftInnerEye = facePoints[133].transform.localPosition;
                Vector3 rightOuterEye = facePoints[362].transform.localPosition;
                Vector3 rightInnerEye = facePoints[263].transform.localPosition;

                Vector3 leftUpperEye = facePoints[159].transform.localPosition;
                Vector3 leftBottomEye = facePoints[145].transform.localPosition;
                Vector3 rightUpperEye = facePoints[386].transform.localPosition;
                Vector3 rightBottomEye = facePoints[374].transform.localPosition;

                float leftEyeWidth = Vector3.Distance(leftOuterEye, leftInnerEye);
                float rightEyeWidth = Vector3.Distance(rightOuterEye, rightInnerEye);

                float leftEyeHeight = Vector3.Distance(leftUpperEye, leftBottomEye);
                float rightEyeHeight = Vector3.Distance(rightUpperEye, rightBottomEye);

                //gives out average width for the eyes to fit appropriately within the eyelids
                float avgWidth = (leftEyeWidth + rightEyeWidth)/2f;
                float leftIrisTargetScale = Mathf.Lerp(leftIris.irisScaleX, avgWidth * 1.2f, Time.deltaTime * smoothFactor);
                float rightIrisTargetScale = Mathf.Lerp(leftIris.irisScaleX, avgWidth * 1.2f, Time.deltaTime * smoothFactor);
                leftIris.irisScaleX = leftIrisTargetScale;
                rightIris.irisScaleX = rightIrisTargetScale;

                //adjusts the Y radius of the "iris" (a.k.a. the ellipse) based on
                //the distance between the upper eyelid landmark and the
                //lower eyelid landmark to to simulate blinking.
                //InverseLerp is used to calculate the ratio of the height to use as the bound.
                float eyeBlink(float eyeHeight)
                {
                    float bound = Mathf.InverseLerp(0.05f, 0.15f, eyeHeight);
                    bound = Mathf.Pow(bound, 1.5f);
                    return Mathf.Lerp(0.0001f, 1f, bound);
                }

                float leftEyeDistance = eyeBlink(leftEyeHeight);
                float rightEyeDistance = eyeBlink(rightEyeHeight);

                leftEyeDistance = Mathf.Min(leftEyeDistance, leftIris.irisScaleX);
                rightEyeDistance = Mathf.Min(rightEyeDistance, rightIris.irisScaleX);

                //assigns radiusY to the irises in CircleCode
                leftIris.irisScaleY = leftEyeDistance;
                rightIris.irisScaleY = rightEyeDistance;

                //assigns position of FaceMesh parent. Lerp is used to interpolate movement between FaceMesh
                //position with respect to the calculated rootPosition per frame.
                faceRoot.localPosition = Vector3.Lerp(faceRoot.localPosition, rootPosition, Time.deltaTime * smoothFactor);
            }

            //Calculating Calibration Center so that the face doesn't appear too
            //far off from Unity's Camera Viewport
            Vector2 center = Vector2.zero;
            int count = 0;
            
            //x1 y1 z1 x2 y2 z2 x3 y3 z3. Index increases by three to consistently
            //access either x, y values. For loop used to calculate center with
            //respect to the facePoints location in the x and y axes.
            for (int i = 0; i + 2 < points.Length; i += 3)
            {   
                if (string.IsNullOrEmpty(points[i]))
                {
                    continue;
                }

                if(float.TryParse(points[i], out float x)
                && float.TryParse(points[i + 1], out float y))
                    {
                        x /= 100f;
                        y = -y /100f;

                        center += new Vector2(x, y);
                        count ++;
                    }
            }

            //Normalizing center based on count per facepoint.
            if (count > 0)
            {
                center /= count;
            }
            
            //another index used to keep track of facePoints to draw in the other side of the face
            //original index increments by 3 to consistently access either x, y, z values
            int index = 0;
            for (int i = 0; index < facePoints.Length && i + 2 < points.Length; i += 3)
            {   
                if (string.IsNullOrEmpty(points[i]))
                {
                    continue;
                }
                
                //initializes the points' location and consistently updates their
                //locations per frame
                if(float.TryParse(points[i], out float x)
                    && float.TryParse(points[i + 1], out float y)
                    && float.TryParse(points[i + 2], out float z))
                    {
                        x /= 100f;
                        y = -y/100f;
                        z /= 100f;

                        z *= depthScale;
                        
                        Vector3 facialPosition = new Vector3(x - center.x, y - center.y, z);
                        facePoints[index].transform.localPosition = Vector3.Lerp(facePoints[index].transform.localPosition, facialPosition, Time.deltaTime * smoothFactor);
                        index++;
                }
            }
        } 
    }
}
