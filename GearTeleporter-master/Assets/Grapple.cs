using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour {
    
    [SerializeField] Transform shotReferenceTransform;                  // this object's forward direction is used as reference to cast our ray
    [SerializeField] GameObject ballSwinger;                            // the swinging ball we follow
    [SerializeField] GameObject ballContainer;                          // the cube that blocks the ball   
    [SerializeField] Transform cameraRigTransform;                      // the Transform of the CameraRig
    [SerializeField] PlayerManager playerManager;                       // the PlayerManager attached to CameraRig
    [SerializeField] Grapple otherGrapple;                              // the other hand's grapple
    [SerializeField] Material normalMaterial;                           // the laser's material when we're not attached to anything
    [SerializeField] Material swingableMaterial;                        // the laser's material when we're pointing at something swingable
    [SerializeField] Material swingingMaterial;                         // the laser's material when we're swinging

    [HideInInspector] public Vector3 previousBallPosition;              // the ball's position last frame
    [HideInInspector] public bool isHooked;                             // is this grapple currently hooked?
    [HideInInspector] public bool ballSwingerOwner;                     // do we move relative to this hand?
    [HideInInspector] public bool cameraRigMoveDisabledThisFrame;       // tells if we should not move the camera rig this frame

    public GameObject myContainer;              // this hand's cube that blocks us

    SteamVR_TrackedObject trackedObj;           // this hand
    LineRenderer lineRenderer;                  // this hand's line renderer
    Rigidbody ballSwingerRigidbody;             // the balls rigidbody
    Vector3 previousControllerPosition;         // the position the controller was in last frame
    Vector3 previousCameraRigPosition;          // the position the camera rig was in last frame
    Vector3 laserEndPosition;                   // where our laser stops
    float maxLength = 100f;                     // the max length our laser can travel
    int layerMask;                              // the layermask of what our ray can hit

    // Use this for initialization
    void Start ()
    {
        // Assign declared variables
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        lineRenderer = GetComponent<LineRenderer>();
        ballSwingerRigidbody = ballSwinger.GetComponent<Rigidbody>();
        isHooked = false;
        ballSwingerOwner = false;
        cameraRigMoveDisabledThisFrame = false;

        // Create the layermask. Add everything we don't want to hit to the mask, then reverse the mask
        layerMask = LayerMask.GetMask("BallSwinger", "BallContainer");
        layerMask = ~layerMask;
    }

    // Update is called once per frame
    void Update ()
    {
        // Create the device, Ray, and RaycastHit
        var device = SteamVR_Controller.Input((int)trackedObj.index);
        Ray ray = new Ray(transform.position, shotReferenceTransform.up);
        RaycastHit raycastHit;
        
        // Set this as the laser's end position for starters. It may change later in the script
        laserEndPosition = transform.position + shotReferenceTransform.up * maxLength;

        // If we're actually pointing at an object...
        if (Physics.Raycast(ray, out raycastHit, maxLength, layerMask))
        {
            // Set our laser to stop there
            laserEndPosition = raycastHit.point;

            // If the object we're pointing at is swingable...
            if (raycastHit.collider.gameObject.tag == "Swingable")
            {
                // Change the laser color
                lineRenderer.material = swingableMaterial;

                // If we press the trigger...
                if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                {
                    // If we're airborne...
                    if (!playerManager.onGround)
                    {
                        // ...shoot the grapple at it, flag ourselves hooked, and notify the PlayerManager
                        ShootGrapple(raycastHit.point);
                        isHooked = true;

                        playerManager.SwitchToHookedFromMidair();
                    }
                    // If we're on the ground...
                    else
                    {
                        // ...jump!
                        playerManager.Jump();
                    }
                }
            }
        }
        // Otherwise, our ray didn't hit anything, so...
        else
        {
            // ...change the line renderer material accordingly
            lineRenderer.material = normalMaterial;
        }

        // If this grapple is hooked and the trigger is RELEASED...
        if (isHooked && device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
        {
            // If the other grapple isn't hooked, then we're in freefall, so...
            if (!otherGrapple.isHooked)
            {
                // Deactivate the ball and container, and give the ball's final velocity to the player manager so he can take over
                Vector3 currentVelocity = ballSwingerRigidbody.velocity;
                DeactivateBall();
                myContainer.SetActive(false);
                playerManager.SwitchToMidairFromHooked(currentVelocity);
            }
            // Otherwise, the other grapple IS hooked, so...
            else
            {
                // If this is the ball swinger owner...
                if (ballSwingerOwner)
                {
                    // Declare the other grapple as the righteous owner of the sacred ball :O
                    ballSwingerOwner = false;
                    otherGrapple.ballSwingerOwner = true;

                    // Deactivate our container, give the other hand the ball and reset their container to the right spot
                    myContainer.SetActive(false);
                    Vector3 otherContainerOrigin = otherGrapple.myContainer.transform.position;
                    ballSwinger.transform.position = otherGrapple.transform.position;
                    otherGrapple.myContainer.transform.position = otherContainerOrigin;
                    otherGrapple.myContainer.GetComponent<Container>().SetDistanceFromOrigin();
                    Vector3 ballCurrentVelocity = ballSwingerRigidbody.velocity;
                    otherGrapple.previousBallPosition = ballSwinger.transform.position + -(ballCurrentVelocity * Time.deltaTime);
                }
                // Otherwise, this wasn't the ball swinger owner, so...
                else
                {
                    // Just deactivate our container and double check the ballSwingerOwner bools are correct
                    myContainer.SetActive(false);
                    otherGrapple.ballSwingerOwner = true;
                    ballSwingerOwner = false;
                }
            }

            // Finally, make sure we're marked as not hooked and not the ball owner
            isHooked = false;
            ballSwingerOwner = false;
        }

        // If this grapple is hooked...
        if (isHooked)
        {
            // If we're the righteous ball owner and we're not on that frame where we shouldn't move...
            if (ballSwingerOwner && !cameraRigMoveDisabledThisFrame)
            {
                // ...move the camera rig the appropriate distance, so it appears as if we're moving relative to the ball
                cameraRigTransform.position += (previousControllerPosition - trackedObj.transform.position);
                cameraRigTransform.position += (ballSwinger.transform.position - previousBallPosition);
            }

            // Set line renderer's material and end position
            lineRenderer.material = swingingMaterial;
            laserEndPosition = myContainer.transform.position;
        }

        // Assign all those previous positions that we may need for next frame
        previousCameraRigPosition = cameraRigTransform.position;
        previousControllerPosition = transform.position;
        if (ballSwinger.activeSelf) previousBallPosition = ballSwinger.transform.position;

        // Unflag that the camera move should be disabled so it can only last one frame maximum
        cameraRigMoveDisabledThisFrame = false;
    }

    void LateUpdate()
    {
        // Set our line renderer to the right position
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, laserEndPosition);
    }



    //////////////////////////////////////////////////////
    /////////////////// CUSTOM METHODS ///////////////////
    //////////////////////////////////////////////////////

    /// <summary>
    /// Shoots the grapple, with the container centered at the input Vector3
    /// </summary>
    /// <param name="inputHitLocation"></param>
    private void ShootGrapple(Vector3 inputHitLocation)
    {
        // Declare this hand as the righteous ballSwinger owner
        ballSwingerOwner = true;
        otherGrapple.ballSwingerOwner = false;

        // Activate the ball and put it in position at our hand
        ballSwinger.SetActive(true);
        Vector3 ballPositionBeforeTeleport = ballSwinger.transform.position;
        ballSwinger.transform.position = transform.position;

        // This needs to be added so the camera rig doesn't think we moved a crazy amount
        Vector3 amountBallMoved = ballSwinger.transform.position - ballPositionBeforeTeleport;
        previousBallPosition += amountBallMoved;

        // Make it so the other grapple can't move the camera rig for one frame. This is so we don't get double-moved if grapples are shot in the same frame
        otherGrapple.cameraRigMoveDisabledThisFrame = true;

        // If the other grapple isn't hooked, then we're the first hooked hand, so...
        if (!otherGrapple.isHooked)
        {
            // ...set the ball's velocity to be our velocity, and trick the previousBallPosition into being appropriate
            ballSwingerRigidbody.velocity = playerManager.currentVelocity;
            previousBallPosition = ballSwinger.transform.position + -(playerManager.currentVelocity * Time.deltaTime);
        }

        // Activate our container, and set it to the appropriate place
        myContainer.SetActive(true);
        myContainer.transform.position = inputHitLocation;
        Container myContainerScript = myContainer.GetComponent<Container>();
        myContainerScript.SetDistanceFromOrigin();
        
        // If the other grapple is already hooked...
        if (otherGrapple.isHooked)
        {
            // ...set its container's new distance
            otherGrapple.myContainer.GetComponent<Container>().SetDistanceFromOrigin();
        }
    }
    
    /// <summary>
    /// Just deactivates the ball. Only here so other scripts can do this
    /// </summary>
    public void DeactivateBall()
    {
        ballSwinger.SetActive(false);
    }
}
