using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Kinda the master of the player. Also is in charge of Follow Me and the ball swinger
/// </summary>
public class PlayerManager : MonoBehaviour
{
    [SerializeField] Grapple leftHandGrapple;                       // the Grapple script attached to our left hand
    [SerializeField] Grapple rightHandGrapple;                      // the Grapple script attached to our right hand
    [SerializeField] GameObject landingSensor;                      // our landing sensor object
    [SerializeField] GameObject followMeCameraRig;                  // the Follow Me, Camera Rig! object
    [SerializeField] GameObject ballSwinger;                        // the ball object

    [HideInInspector] public PlayerStatus currentStatus;

    FollowMeCameraRig followMeCameraRigScript;
    Rigidbody ballSwingerRigidbody;
    Rigidbody followMeRigidbody;
    Transform cameraEyeTransform;               // our head's transform
    Vector3 previousPosition;                   // last frame's position
    float verticalJumpForce = 6f;               // the force applied upwards when jumping
    float lateralJumpForce = 3f;                // the force applied laterally when jumping

    // Use this for initialization
    void Start ()
    {
        // Initialize those declared variables
        previousPosition = transform.position;
        cameraEyeTransform = GameObject.Find("Camera (eye)").transform;
        currentStatus = PlayerStatus.Grounded;
        ballSwingerRigidbody = ballSwinger.GetComponent<Rigidbody>();
        followMeRigidbody = followMeCameraRig.GetComponent<Rigidbody>();
        followMeCameraRigScript = followMeCameraRig.GetComponent<FollowMeCameraRig>();
    }




    //////////////////////////////////////////////////////
    /////////////////// CUSTOM METHODS ///////////////////
    //////////////////////////////////////////////////////

    public void Jump()
    {
        // Activate Follow Me, and invoke the landing sensor to come on in a sec, so it doesn't IMMEDIATELY say we landed
        currentStatus = PlayerStatus.Midair;
        followMeCameraRig.SetActive(true);
        Invoke("SetLandingSensorActive", .1f);

        // Do some vector math stuff to figure out what to set our velocity at to jump, then jump!
        Vector3 vectorToJump = cameraEyeTransform.forward;
        vectorToJump.y = 0f;
        vectorToJump = vectorToJump.normalized * lateralJumpForce;
        vectorToJump.y = verticalJumpForce;
        followMeCameraRig.GetComponent<Rigidbody>().velocity = vectorToJump;
    }
    
    /// <summary>
    /// This method only exists so we can call "Invoke" on it
    /// </summary>
    private void SetLandingSensorActive()
    {
        landingSensor.SetActive(true);
    }



    public void SwitchToStatus(PlayerStatus inputStatus)
    {
        if (inputStatus == PlayerStatus.Grounded)
        {
            transform.parent = null;
            ballSwinger.SetActive(false);

            // For either hand that's hooked, deactivate it's container and the ball
            if (rightHandGrapple.isHooked)
            {
                rightHandGrapple.myContainer.SetActive(false);
            }
            if (leftHandGrapple.isHooked)
            {
                leftHandGrapple.myContainer.SetActive(false);
            }

            // Make sure both laser pointers are active on each grapple
            leftHandGrapple.ToggleLaser(true);
            rightHandGrapple.ToggleLaser(true);

            // Deactivate Follow Me and our landing sensor, and flag ourselves grounded
            followMeCameraRig.SetActive(false);
            landingSensor.SetActive(false);

            currentStatus = PlayerStatus.Grounded;
        }

        else if (inputStatus == PlayerStatus.Midair)
        {
            if (currentStatus == PlayerStatus.Grounded)
            {
                // Activate Follow Me and the landing sensor, and flag ourselves as ungrounded
                followMeCameraRig.SetActive(true);
                landingSensor.SetActive(true);
            }
            else if (currentStatus == PlayerStatus.Hooked)
            {
                followMeCameraRigScript.DisconnectFromBall();
                ballSwinger.SetActive(false);
            }

            currentStatus = PlayerStatus.Midair;
        }

        else if (inputStatus == PlayerStatus.Hooked)
        {
            if (currentStatus == PlayerStatus.Grounded)
            {
                Debug.LogError("WHOA! You can't go from grounded status to hooked status. I don't even know how you got here.");
            }
            else
            {
                // If we weren't already hooked (with the other grapple), joint connect the Follow Me capsule to the ball
                if (currentStatus == PlayerStatus.Midair)
                {
                    // Activate the ball and put it in position at our head
                    ballSwinger.SetActive(true);
                    ballSwinger.transform.position = cameraEyeTransform.position;
                    ballSwingerRigidbody.velocity = followMeRigidbody.velocity;

                    // Update the position of Follow Me Camera Rig to the ball
                    followMeCameraRigScript.SetPosition();

                    // Make the Follow Me capsule joint connect to the ball
                    followMeCameraRigScript.ConnectToBall();
                    followMeRigidbody.velocity = ballSwingerRigidbody.velocity;
                }
            }
            
            currentStatus = PlayerStatus.Hooked;
        }

        print("Switched status to " + currentStatus);
    }
}
