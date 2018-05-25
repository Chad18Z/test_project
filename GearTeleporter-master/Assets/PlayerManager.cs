using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Kinda the master of the player. Holds important variables and does high level stuff
/// </summary>
public class PlayerManager : MonoBehaviour
{
    [SerializeField] Grapple leftHandGrapple;                       // the Grapple script attached to our left hand
    [SerializeField] Grapple rightHandGrapple;                      // the Grapple script attached to our right hand
    [SerializeField] GameObject landingSensor;                      // our landing sensor object
    [SerializeField] GameObject followMeCameraRig;                  // the Follow Me, Camera Rig! object

    [HideInInspector] public bool onGround;                         // tells if we're on the ground

    Transform cameraEyeTransform;               // our head's transform
    Vector3 previousPosition;                   // last frame's position
    float verticalJumpForce = 6f;               // the force applied upwards when jumping
    float lateralJumpForce = 2f;                // the force applied laterally when jumping

    // Use this for initialization
    void Start ()
    {
        // Initialize those declared variables
        previousPosition = transform.position;
        onGround = true;
        cameraEyeTransform = GameObject.Find("Camera (eye)").transform;
    }




    //////////////////////////////////////////////////////
    /////////////////// CUSTOM METHODS ///////////////////
    //////////////////////////////////////////////////////

    /// <summary>
    /// Switches from being midair to being hooked
    /// </summary>
    public void SwitchToHookedFromMidair()
    {
        // Turn on Follow Me and the landing sensor, and record that we're not on the ground
        followMeCameraRig.SetActive(true);
        landingSensor.SetActive(true);
        onGround = false;
    }
    
    /// <summary>
    /// Switches to landed
    /// </summary>
    public void SwitchToLanded()
    {
        // For either hand that's hooked, deactivate it's container and the ball
        if (rightHandGrapple.isHooked)
        {
            rightHandGrapple.DeactivateBall();
            rightHandGrapple.myContainer.SetActive(false);
        }
        if (leftHandGrapple.isHooked)
        {
            leftHandGrapple.DeactivateBall();
            leftHandGrapple.myContainer.SetActive(false);
        }

        // Deactivate Follow Me and our landing sensor, and flag ourselves grounded
        followMeCameraRig.SetActive(false);
        landingSensor.SetActive(false);
        onGround = true;
    }

    /// <summary>
    /// Switches to freefall
    /// </summary>
    public void SwitchToFellOffCliff()
    {
        // Activate Follow Me and the landing sensor, and flag ourselves as ungrounded
        followMeCameraRig.SetActive(true);
        landingSensor.SetActive(true);
        onGround = false;
    }
    
    /// <summary>
    /// Goes from being the on the ground to jumping
    /// </summary>
    public void Jump()
    {
        // Activate Follow Me, and invoke the landing sensor to come on in a sec, so it doesn't IMMEDIATELY say we landed
        followMeCameraRig.SetActive(true);
        Invoke("SetLandingSensorActive", .1f);
        onGround = false;
        
        // Do some vector math stuff to figure out what to set our velocity at to jump
        Vector2 flattenedTargetVector = (new Vector2(cameraEyeTransform.forward.x, cameraEyeTransform.forward.z)).normalized * lateralJumpForce;
        Vector3 jumpVector = new Vector3(flattenedTargetVector.x, verticalJumpForce, flattenedTargetVector.y);
        followMeCameraRig.GetComponent<Rigidbody>().velocity = jumpVector;
    }
    
    /// <summary>
    /// This method only exists so we can call "Invoke" on it
    /// </summary>
    private void SetLandingSensorActive()
    {
        landingSensor.SetActive(true);
    }
}
