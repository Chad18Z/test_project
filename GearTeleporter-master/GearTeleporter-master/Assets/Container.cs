using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    public Transform targetTransform;   // the Transform of our target, the ball

    GameObject myAnchor;
    float cubeHalfWidth;                // half of the cube's width. Duh
    float ballSwingerRadius;            // radius of the ball swinger. Obviously

    void Start()
    {
        myAnchor = transform.parent.gameObject;

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
	}

    void OnDisable()
    {
        if (myAnchor.GetComponent<FixedJoint>()) Destroy(GetComponent<FixedJoint>());
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

    /// <summary>
    /// Joint attaches the container to the object, ensuring we'll follow it if it moves
    /// </summary>
    /// <param name="inputObjectTransform"></param>
    public void LatchToObject(Transform inputObjectTransform)
    {
        FixedJoint fixedJoint = myAnchor.AddComponent<FixedJoint>();
        fixedJoint.connectedBody = inputObjectTransform.GetComponent<Rigidbody>();
    }
}
