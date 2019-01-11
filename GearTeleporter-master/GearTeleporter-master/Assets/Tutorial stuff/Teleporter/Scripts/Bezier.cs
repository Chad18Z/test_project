using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Probably don't mess with this class too much. It's a lot of math and I only somewhat understand it
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class Bezier : MonoBehaviour
{
    [HideInInspector] public bool endPointDetected;
    [HideInInspector] public bool endPointSurfaceValid;
    [SerializeField] Material validTeleportMaterial;
    [SerializeField] Material invalidTeleportMaterial;

    public Vector3 EndPoint
    {
        get { return endpoint; }
    }

    // controls how much farther you can travel, NOT how long the bezier will continue. (Keep between -1f and 1f)
    public float ExtensionFactor
    {
        set { extensionFactor = value; }
    }

    AnimateLine animateLine;
    private Vector3 endpoint;
    private float extensionFactor;
    private Vector3[] controlPoints;
    private LineRenderer lineRenderer;
    float maxTeleportSurfaceAngle = 45f;
    private float extendStep;
    private int SEGMENT_COUNT = 20;                     // this times the segmentCountMultiplier is how many segments there will be
    int segmentCountMultiplier = 5;                     // how much further the bezier will extend
    bool currentlyActive = false;

    void Awake()
    {
        animateLine = GetComponent<AnimateLine>();
        controlPoints = new Vector3[3];
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        extendStep = 5f;
        extensionFactor = 0f;
    }

    void Update()
    {
        UpdateControlPoints();
        HandleExtension();
        if (currentlyActive)
        {
            DrawCurve();
        }
    }

    public void ToggleDraw(bool inputBool)
    {
        if (inputBool == true)
        {
            StartCoroutine(DelayedTurnOnDraw());
        }
        else
        {
            lineRenderer.enabled = false;
        }

        currentlyActive = inputBool;
    }

    void HandleExtension()
    {
        if (extensionFactor == 0f)
            return;

        float finalExtension = extendStep + Time.deltaTime * extensionFactor * 2f;
        extendStep = Mathf.Clamp(finalExtension, 2.5f, 7.5f);
    }

    // The first control is the remote. The second is a forward projection. The third is a forward and downward projection.
    void UpdateControlPoints()
    {
        controlPoints[0] = gameObject.transform.position; // Get Controller Position
        controlPoints[1] = controlPoints[0] + (gameObject.transform.forward * extendStep * 2f / 5f);
        controlPoints[2] = controlPoints[1] + (gameObject.transform.forward * extendStep * 3f / 5f) + Vector3.up * -1f;
    }


    // Draw the bezier curve.
    void DrawCurve()
    {
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, controlPoints[0]);

        Vector3 prevPosition = controlPoints[0];
        Vector3 nextPosition = prevPosition;

        float tIncrement = 1 / (float)SEGMENT_COUNT;
        float t = 0f;

        for (int i = 1; i <= SEGMENT_COUNT * segmentCountMultiplier; i++)
        {
            t += tIncrement;
            lineRenderer.positionCount = i + 1;

            // For the last point, project out the curve two more meters.
            if (i == SEGMENT_COUNT * segmentCountMultiplier)
            {
                Vector3 endDirection = Vector3.Normalize(prevPosition - lineRenderer.GetPosition(i - 2));
                nextPosition = prevPosition + endDirection * 2f;
            }
            else
            {
                nextPosition = CalculateBezierPoint(t, controlPoints[0], controlPoints[1], controlPoints[2]);
            }

            if (CheckColliderIntersection(prevPosition, nextPosition))
            { // If the segment intersects a surface, draw the point and return.
                lineRenderer.SetPosition(i, endpoint);
                endPointDetected = true;
                return;
            }
            else
            { // If the point does not intersect, continue to draw the curve.
                lineRenderer.SetPosition(i, nextPosition);
                endPointDetected = false;
                prevPosition = nextPosition;
            }

            // If we got here, it means the curve never hit anything, so make it red
            lineRenderer.material = invalidTeleportMaterial;
            animateLine.currentMaterial = invalidTeleportMaterial;
        }
    }

    // Check if the line between start and end intersect a collider.
    bool CheckColliderIntersection(Vector3 start, Vector3 end)
    {
        Ray r = new Ray(start, end - start);
        RaycastHit hit;
        if (Physics.Raycast(r, out hit, Vector3.Distance(start, end)))
        {
            if (Vector3.Angle(hit.normal, Vector3.up) <= maxTeleportSurfaceAngle)
            {
                animateLine.currentMaterial = validTeleportMaterial;
                lineRenderer.material = validTeleportMaterial;
                endPointSurfaceValid = true;
            }
            else
            {
                animateLine.currentMaterial = invalidTeleportMaterial;
                lineRenderer.material = invalidTeleportMaterial;
                endPointSurfaceValid = false;
            }

            endpoint = hit.point;
            return true;
        }

        return false;
    }

    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return Mathf.Pow((1f - t), 2) * p0 + 2f * (1f - t) * t * p1 + Mathf.Pow(t, 2) * p2;
    }

    IEnumerator DelayedTurnOnDraw()
    {
        yield return null;

        lineRenderer.enabled = true;
    }
}
