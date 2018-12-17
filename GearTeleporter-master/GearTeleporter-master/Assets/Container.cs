using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour {

    [HideInInspector] public Transform latchedObjectTransform;
    [HideInInspector] public Vector3 latchedObjPreviousPosition;
    public Transform targetTransform;   // the Transform of our target, the ball

    
    float cubeHalfWidth;                // half of the cube's width. Duh
    float ballSwingerRadius;            // radius of the ball swinger. Obviously

    void Start()
    {
        // Assign the correct values, then deactivate ourselves
        cubeHalfWidth = transform.GetChild(0).transform.localScale.x / 2f;
        ballSwingerRadius = targetTransform.GetComponent<SphereCollider>().radius * targetTransform.localScale.x;

        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update ()
    {
        // Always point to the ball
        Vector3 directionToLook = targetTransform.position - transform.position;
        transform.forward = directionToLook;

        // If we're latched to a moving object...
        if (latchedObjectTransform != null)
        {
            // ...move the container along with the moving object
            transform.position += latchedObjectTransform.position - latchedObjPreviousPosition;

            // Store that objects current position for next frame
            latchedObjPreviousPosition = latchedObjectTransform.position;
        }
	}

    void OnDisable()
    {
        latchedObjectTransform = null;
    }



    //////////////////////////////////////////////////////
    /////////////////// CUSTOM METHODS ///////////////////
    //////////////////////////////////////////////////////

    /// <summary>
    /// Sets the block the correct distance from the ball to contain it
    /// </summary>
    public void SetDistanceFromOrigin()
    {
        // Point self to ball
        Vector3 directionToLook = targetTransform.position - transform.position;
        transform.forward = directionToLook;

        // Get the distance we should be, and set that as our distance in the Z-axis
        float distFromTarget = (transform.position - targetTransform.position).magnitude;
        transform.GetChild(0).transform.localPosition = new Vector3(0f, 0f, distFromTarget + ballSwingerRadius + cubeHalfWidth + 0.02f);
    }
}
