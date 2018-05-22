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

    [HideInInspector] public bool onGround;                         // tells if we're on the ground
    [HideInInspector] public Vector3 currentVelocity;               // tells our current velocity (DOESN'T set it)

    Transform cameraEyeTransform;               // our head's transform
    Rigidbody rigidBody;                        // our rigidbody
    Vector3 previousPosition;                   // last frame's position
    float verticalJumpForce = 6f;               // the force applied upwards when jumping
    float lateralJumpForce = 2f;                // the force applied laterally when jumping

    // Use this for initialization
    void Start ()
    {
        // Initialize those declared variables
        previousPosition = transform.position;
        onGround = true;
        rigidBody = GetComponent<Rigidbody>();
        cameraEyeTransform = GameObject.Find("Camera (eye)").transform;
    }

    void FixedUpdate()
    {
        // Keep our current velocity current!
        currentVelocity = (transform.position - previousPosition) / Time.deltaTime;

        // Set our previous position
        previousPosition = transform.position;
    }




    //////////////////////////////////////////////////////
    /////////////////// CUSTOM METHODS ///////////////////
    //////////////////////////////////////////////////////

    /// <summary>
    /// Switches from being midair to being hooked
    /// </summary>
    public void SwitchToHookedFromMidair()
    {
        rigidBody.isKinematic = true;
        rigidBody.useGravity = false;

        landingSensor.SetActive(true);
        onGround = false;
    }
    
    // TODO: Make sure this works when double-hooked
    /// <summary>
    /// Switches to landed
    /// </summary>
    public void SwitchToLanded()
    {
        // TODO: Add left hand grapple
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

        rigidBody.isKinematic = true;
        rigidBody.useGravity = false;

        landingSensor.SetActive(false);
        onGround = true;
    }

    /// <summary>
    /// Switches to freefall
    /// </summary>
    public void SwitchToFellOffCliff()
    {
        rigidBody.isKinematic = false;
        rigidBody.useGravity = true;

        landingSensor.SetActive(true);
        onGround = false;
    }

    /// <summary>
    /// Switches to freefall FROM being hooked, taking in the last velocity we were
    /// </summary>
    /// <param name="inputVelocity"></param>
    public void SwitchToMidairFromHooked(Vector3 inputVelocity)
    {
        rigidBody.isKinematic = false;
        rigidBody.useGravity = true;
        rigidBody.velocity = inputVelocity;

        landingSensor.SetActive(true);
        onGround = false;
    }
    
    /// <summary>
    /// Goes from being the on the ground to jumping
    /// </summary>
    public void Jump()
    {
        rigidBody.isKinematic = false;
        rigidBody.useGravity = true;
        Invoke("SetLandingSensorActive", .1f);
        onGround = false;
        
        Vector2 flattenedTargetVector = (new Vector2(cameraEyeTransform.forward.x, cameraEyeTransform.forward.z)).normalized * lateralJumpForce;
        Vector3 jumpVector = new Vector3(flattenedTargetVector.x, verticalJumpForce, flattenedTargetVector.y);
        rigidBody.AddForce(jumpVector, ForceMode.Impulse);
    }
    
    /// <summary>
    /// This method only exists so we can call "Invoke" on it
    /// </summary>
    private void SetLandingSensorActive()
    {
        landingSensor.SetActive(true);
    }
}
