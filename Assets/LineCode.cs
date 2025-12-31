using UnityEngine;

public class LineCode : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public Transform[] contourPoints;
    public bool closedLoop = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
    }

    // Update is called once per frame
    void Update()
    {
        if (contourPoints == null || contourPoints.Length == 0)
        {
            return;
        }

        lineRenderer.positionCount = contourPoints.Length + (closedLoop ? 1 : 0);
        for(int i = 0; i < contourPoints.Length; i++)
        {
            if (contourPoints[i] != null)
            {
                lineRenderer.SetPosition(i, contourPoints[i].localPosition);
            } 
        }

        if (closedLoop && contourPoints[0] != null)
        {
            lineRenderer.SetPosition(contourPoints.Length, contourPoints[0].localPosition);
        }
    }
}
