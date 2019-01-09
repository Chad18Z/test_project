using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// TODO: Latch to moving objects
// TODO: Make grapple disable line renderer
// TODO: Make ball swinger the determinant of how bent/wiggly the line is
public class Rope : MonoBehaviour
{
    [SerializeField] GameObject ropePoint;             // the stand-in point that is a reference for each line renderer point
    [SerializeField] int numberOfPoints;               // number of points per full wave. THIS MUST BE DIVISIBLE BY 4
    [SerializeField] bool showSpheres;                 // show the spheres?
    [SerializeField] bool showLine;                    // draw the line?
    [SerializeField] Transform ballSwingerTransform;   // the transform of the ball swinger

    Transform[] pointArray;                     // the array of points

    Transform controllerTransform;              // the controller's transform
    Vector3 currentTargetLocation;              // the location of the current we're attached to
    Transform ropeContainerTransform;           // the transform of the rope container
    LineRenderer lineRenderer;                  // this object's line renderer
    float twoPi;                                // just two times pi, the distance of the unmodified, fully stretched rope
    float maxControllerToTargetDist;                    // the distance from the player to the target in the instant the grapple is fired
    float maxBallToTargetDist;
    bool hooked = false;                        // are we hooked to an object?

    // Use this for initialization
    void Start()
    {
        controllerTransform = transform.parent.parent;

        // Spawn all the points in the rope and position them in a straight, (mostly) even line
        // If the input number of points is divisible by 4...
        if (numberOfPoints % 4 == 0)
        {
            // Add one to the number of points to account for the starting point
            numberOfPoints++;

            // Calculate the usual distance between each point
            float distBetweenPoints = ((Mathf.PI * 2f) / (numberOfPoints - 1));

            // Spawn all the points in the line
            for (int i = 0; i < numberOfPoints; i++)
            {
                // Generate a new point gameobject that's our child
                GameObject newSphere = Instantiate(ropePoint, transform);

                // Set it's z distance
                float currentZ = distBetweenPoints * i;
                newSphere.transform.localPosition = new Vector3(0f, 0f, currentZ);
            }

            // Next, add the bonus points near the peaks for extra smoothness
            float halfDistBetweenPoints = distBetweenPoints / 2f;

            GameObject peakSmootherPoint1A = Instantiate(ropePoint, transform);
            peakSmootherPoint1A.name = "peakSmootherPoint1A";
            GameObject peakSmootherPoint1B = Instantiate(ropePoint, transform);
            peakSmootherPoint1B.name = "peakSmootherPoint1B";
            GameObject peakSmootherPoint2A = Instantiate(ropePoint, transform);
            peakSmootherPoint2A.name = "peakSmootherPoint2A";
            GameObject peakSmootherPoint2B = Instantiate(ropePoint, transform);
            peakSmootherPoint2B.name = "peakSmootherPoint2B";

            peakSmootherPoint1A.transform.position = new Vector3(0f, 0f, Mathf.PI * 0.5f - halfDistBetweenPoints);
            peakSmootherPoint1B.transform.position = new Vector3(0f, 0f, Mathf.PI * 0.5f + halfDistBetweenPoints);
            peakSmootherPoint2A.transform.position = new Vector3(0f, 0f, Mathf.PI * 1.5f - halfDistBetweenPoints);
            peakSmootherPoint2B.transform.position = new Vector3(0f, 0f, Mathf.PI * 1.5f + halfDistBetweenPoints);

            // Set them to the correct child index so the line renderer draws em in the right order
            peakSmootherPoint1A.transform.SetSiblingIndex((numberOfPoints - 1) / 4);
            peakSmootherPoint1B.transform.SetSiblingIndex(((numberOfPoints - 1) / 4) + 2);
            peakSmootherPoint2A.transform.SetSiblingIndex(((numberOfPoints - 1) / 4) * 3 + 2);
            peakSmootherPoint2B.transform.SetSiblingIndex((((numberOfPoints - 1) / 4) * 3) + 4);
        }
        // Otherwise the dumb designer input a number of points that's not divisible by 4, so tell them they're stupid
        else
        {
            Debug.LogError("Hey stupid, the number of points on the rope has to be divisible by 4.");
        }

        // Set the declared variables
        ropeContainerTransform = transform.parent;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = transform.childCount;
        pointArray = new Transform[transform.childCount];
        twoPi = 2f * Mathf.PI;

        // Assign the point array to have all my children
        for (int i = 0; i < pointArray.Length; i++)
        {
            pointArray[i] = transform.GetChild(i);
            if (!showSpheres) pointArray[i].GetComponent<MeshRenderer>().enabled = false;
        }

        // Finally, turn off the line renderer so it doesn't show
        lineRenderer.enabled = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (hooked)
        {
            // Always point rope container at the target
            ropeContainerTransform.LookAt(currentTargetLocation);

            // As the player changes distance, adjust the z scale accordingly
            float currentControllerDistOverMaxDist = Vector3.Distance(controllerTransform.position, currentTargetLocation) / maxControllerToTargetDist;
            transform.localScale = new Vector3(1f, 1f, currentControllerDistOverMaxDist);

            float currentBallDistOverMaxDist = Vector3.Distance(ballSwingerTransform.position, currentTargetLocation) / maxBallToTargetDist;

            // Set the amps
            if (currentBallDistOverMaxDist >= 1f)
            {
                SetAllPointsToAmplitude(0);
                transform.localScale = Vector3.one;
            }
            else if (transform.localScale.z > 0 && transform.localScale.z <= 1f)
            {
                float amps = Mathf.Sqrt(2.4674011f - 2.4674011f * (Mathf.Pow(currentBallDistOverMaxDist, 2f)));
                SetAllPointsToAmplitude(amps);
            }

            if (showLine) UpdateLineRenderer();
        }
    }






    // =================================================
    //                CUSTOM METHODS
    // =================================================
    
    // Set all the points in the array to these amps
    private void SetAllPointsToAmplitude(float inputAmplitude)
    {
        for (int i = 0; i < pointArray.Length; i++)
        {
            float newYValue = Mathf.Sin(pointArray[i].localPosition.z) * inputAmplitude;
            pointArray[i].localPosition = new Vector3(pointArray[i].localPosition.x, newYValue, pointArray[i].localPosition.z);
        }
    }

    // Make the line renderer draw it
    private void UpdateLineRenderer()
    {
        for (int i = 0; i < pointArray.Length; i++)
        {
            lineRenderer.SetPosition(i, pointArray[i].position);
        }
    }



    public void AttachToTarget(Vector3 inputTargetLocation)
    {
        lineRenderer.enabled = true;

        // Set the max distance of the rope and adjust the rope container accordingly
        maxBallToTargetDist = Vector3.Distance(ballSwingerTransform.position, inputTargetLocation);
        maxControllerToTargetDist = Vector3.Distance(controllerTransform.position, inputTargetLocation);
        float ropeContainerScale = maxControllerToTargetDist / twoPi;
        ropeContainerTransform.localScale = Vector3.one * ropeContainerScale;
        currentTargetLocation = inputTargetLocation;
        hooked = true;
    }

    public void DetachFromTarget()
    {
        hooked = false;
        lineRenderer.enabled = false;
    }
}
