using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour {
    
    [SerializeField] Transform shotReferenceTransform;                  // this object's forward direction is used as reference to cast our ray
    [SerializeField] GameObject ballSwinger;                            // the swinging ball we follow
    [SerializeField] Grapple otherGrapple;                              // the other hand's grapple
    [SerializeField] Material normalMaterial;                           // the laser's material when we're not attached to anything
    [SerializeField] Material swingableMaterial;                        // the laser's material when we're pointing at something swingable
    [SerializeField] Material swingingMaterial;                         // the laser's material when we're swinging
    [SerializeField] Material hookInvokedMaterial;                      // the laser's material when hooking is invoked
    [SerializeField] GameObject followMeCameraRig;                      // the Follow Me Camera Rig object
    [SerializeField] GameObject grappler;

    [HideInInspector] public bool isHooked;                             // is THIS grapple currently hooked?
    [HideInInspector] public bool hookInvoked;                          // while jumping to get ready to grapple, this is true

    public GameObject myContainer;              // this hand's cube that blocks us

    Container myContainerScript;                // the script attached to my container
    GameObject invokedHitObject;                // the object that we've been invoked to put our container on
    FollowMeCameraRig followMeCameraRigScript;  // the script attached to Follow Me Camera Rig
    Rigidbody followMeRigidbody;                // the rigidbody on Follow Me Camera Rig
    PlayerManager playerManager;                // the PlayerManager attached to CameraRig
    Transform cameraEyeTransform;               // the transform of the headset
    SteamVR_TrackedObject trackedObj;           // this hand
    LineRenderer lineRenderer;                  // this hand's line renderer
    Rigidbody ballSwingerRigidbody;             // the balls rigidbody
    Vector3 laserEndPosition;                   // where our laser stops
    Vector3 initialInvokedSpotToHook;           // when we first invoked hooking, this is the exact position the ray hit
    Vector3 initialInvokedObjectPosition;       // when we first invoked hooking, this is the position that the object we hit was
    float maxLength = 100f;                     // the max length our laser can travel
    float previousYCoord;                       // where we were in the Y-axis last frame. This is used to find when we're at the peak of our jump
    int layerMask;                              // the layermask of what our ray can hit
    bool laserEnabled = true;

    // Use this for initialization
    void Start ()
    {
        // Assign declared variables
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        lineRenderer = GetComponent<LineRenderer>();
        ballSwingerRigidbody = ballSwinger.GetComponent<Rigidbody>();
        isHooked = false;
        hookInvoked = false;
        playerManager = GameObject.Find("[CameraRig]").GetComponent<PlayerManager>();
        cameraEyeTransform = GameObject.Find("Camera (eye)").transform;
        followMeCameraRigScript = followMeCameraRig.GetComponent<FollowMeCameraRig>();
        followMeRigidbody = followMeCameraRig.GetComponent<Rigidbody>();
        myContainerScript = myContainer.GetComponent<Container>();

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

        // FOR TESTING PURPOSES: PAUSE GAME
        //if (device.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
        //{
        //    print("You clicked the pause button! :O");
        //    Time.timeScale = 0f;
        //}

        // If we're allowed to shoot our laser...
        if (laserEnabled)
        {
            // If we're actually pointing at an object...
            if (Physics.Raycast(ray, out raycastHit, maxLength, layerMask))
            {
                // Set our laser to stop there
                laserEndPosition = raycastHit.point;

                // If the object we're pointing at is swingable...
                if (raycastHit.collider.gameObject.tag == "Swingable" || raycastHit.collider.gameObject.tag == "MovingSwingable")
                {
                    // Change the laser color
                    lineRenderer.material = swingableMaterial;

                    // If we press the trigger...
                    if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                    {
                        // If the player manager says we're not grounded...
                        if (!playerManager.onGround)
                        {
                            // ...if the other grapple has hook invoked...
                            if (otherGrapple.hookInvoked)
                            {
                                // ...that means we just jumped, so hook invoke ourselves so we don't fire until we're at our peak
                                InvokeHooking(raycastHit);
                            }
                            // Otherwise, the other grapple isn't invoked, so...
                            else
                            {
                                // ...shoot the grapple and notify the PlayerManager
                                ShootGrapple(raycastHit.point, raycastHit.transform);
                                playerManager.SwitchToHookedFromMidair();
                            }
                        }
                        // Otherwise, we're on the ground, so...
                        else
                        {
                            // ...jump, and invoke hooking
                            playerManager.Jump();
                            InvokeHooking(raycastHit);
                        }
                    }
                }
            }
            // Otherwise, our ray didn't hit anything swingable, so...
            else
            {
                // ...change the line renderer material accordingly
                lineRenderer.material = normalMaterial;
            }
        }
        
        // If the trigger is RELEASED...
        if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
        {
            // ...if we're hooked...
            if (isHooked)
            {
                // If the other grapple isn't hooked, then we're in freefall, so...
                if (!otherGrapple.isHooked)
                {
                    // Deactivate the ball and container
                    followMeCameraRigScript.DisconnectFromBall();
                    DeactivateBall();
                }

                Unhook();
            }

            // ...if hooking is invoked...
            if (hookInvoked)
            {
                // ...uninvoke it
                hookInvoked = false;
            }
        }

        // If hooking is invoked...
        if (hookInvoked)
        {
            // Change the laser material
            lineRenderer.material = hookInvokedMaterial;
            laserEndPosition = initialInvokedSpotToHook;
            
            // ...if we're falling...
            if (playerManager.transform.position.y < previousYCoord)
            {
                // ...take into account if the object we want to shoot moved, and shoot there
                Vector3 targetObjectsPositionChange = invokedHitObject.transform.position - initialInvokedObjectPosition;
                ShootGrapple(initialInvokedSpotToHook + targetObjectsPositionChange, invokedHitObject.transform);

                // Mark ourselves unhooked
                hookInvoked = false;
            }
            // Otherwise, we're not yet falling, so...
            else
            {
                // ...set our previous Y coordinate for next frame
                previousYCoord = playerManager.transform.position.y;
            }
        }

        // If this grapple is hooked...
        if (isHooked && laserEnabled)
        {
            // Set line renderer's material and end position
            lineRenderer.material = swingingMaterial;
            laserEndPosition = myContainer.transform.position;
        }
    }

    void LateUpdate()
    {
        if (laserEnabled)
        {
            // Set our line renderer to the right position
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, laserEndPosition);
        }
    }



    //////////////////////////////////////////////////////
    /////////////////// CUSTOM METHODS ///////////////////
    //////////////////////////////////////////////////////
    
    /// <summary>
    /// Shoots the grapple, with the container centered at the input Vector3
    /// </summary>
    /// <param name="inputHitLocation"></param>
    private void ShootGrapple(Vector3 inputHitLocation, Transform inputHitObjectTransform)
    {
        GameObject grappleSphere = Instantiate(grappler);
        grappleSphere.GetComponent<GrappleSphere>().Target = inputHitLocation;
        

        //// Flag ourselves hooked
        //isHooked = true;

        //// Activate the ball and put it in position at our head
        //ballSwinger.SetActive(true);
        //ballSwinger.transform.position = cameraEyeTransform.position;
        //ballSwingerRigidbody.velocity = followMeRigidbody.velocity;

        //// Update the position of Follow Me Camera Rig to the ball
        //followMeCameraRigScript.SetPosition();

        //// If we're the first to get hooked, joint connect Follow Me to the ball
        //if (!otherGrapple.isHooked)
        //{
        //    followMeCameraRigScript.ConnectToBall();
        //    followMeRigidbody.velocity = ballSwingerRigidbody.velocity;
        //}

        //// Activate our container, and set it to the appropriate place
        //myContainer.SetActive(true);
        //myContainer.transform.position = inputHitLocation;
        //myContainerScript.SetDistanceFromOrigin();

        //// If we're hooking to a moving object...
        //if (inputHitObjectTransform.tag == "MovingSwingable")
        //{
        //    // ...let my container know
        //    myContainerScript.latchedObjectTransform = inputHitObjectTransform;
        //    myContainerScript.latchedObjPreviousPosition = inputHitObjectTransform.position;
        //}
        
        //// If the other grapple is hooked...
        //if (otherGrapple.isHooked)
        //{
        //    // ...reset their container's length, in case the player moved his head
        //    otherGrapple.myContainer.GetComponent<Container>().SetDistanceFromOrigin();
        //}
    }
    
    /// <summary>
    /// Invokes hooking, and assigns all needed variables involved
    /// </summary>
    /// <param name="inputRaycastHit"></param>
    private void InvokeHooking(RaycastHit inputRaycastHit)
    {
        hookInvoked = true;
        initialInvokedSpotToHook = inputRaycastHit.point;
        invokedHitObject = inputRaycastHit.transform.gameObject;
        initialInvokedObjectPosition = invokedHitObject.transform.position;
        previousYCoord = playerManager.transform.position.y;
    }


    /// <summary>
    /// Just deactivates the ball. Only here so other scripts can do this
    /// </summary>
    public void DeactivateBall()
    {
        ballSwinger.SetActive(false);
    }

    public void Unhook()
    {                
        // Deactivate our container and flag ourselves as not hooked
        myContainer.SetActive(false);
        isHooked = false;
    }
}
