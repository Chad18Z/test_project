using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The grapple! Handles rope swinging, and are responsible for their respective containers, but NOT for the ball swinger (that's handled by the PlayerManager script)
/// </summary>
public class Grapple : MonoBehaviour {
    
    [SerializeField] Transform shotReferenceTransform;                  // this object's forward direction is used as reference to cast our ray
    [SerializeField] Grapple otherGrapple;                              // the other hand's grapple
    [SerializeField] Material normalMaterial;                           // the laser's material when we're not attached to anything
    [SerializeField] Material swingableMaterial;                        // the laser's material when we're pointing at something swingable
    [SerializeField] Material swingingMaterial;                         // the laser's material when we're swinging
    [SerializeField] Material hookInvokedMaterial;                      // the laser's material when hooking is invoked
    [SerializeField] GameObject followMeCameraRig;                      // the Follow Me Camera Rig object
    [SerializeField] Rope myRopeScript;                                 // the rope script for this grapple

    [HideInInspector] public bool isHooked;                             // is THIS grapple currently hooked?

    public GameObject myContainer;              // this hand's cube that blocks us

    Container myContainerScript;                // the script attached to my container
    FollowMeCameraRig followMeCameraRigScript;  // the script attached to Follow Me Camera Rig
    Rigidbody followMeRigidbody;                // the rigidbody on Follow Me Camera Rig
    PlayerManager playerManager;                // the PlayerManager attached to CameraRig
    Transform cameraEyeTransform;               // the transform of the headset
    SteamVR_TrackedObject trackedObj;           // this hand
    LineRenderer lineRenderer;                  // this hand's line renderer
    Rigidbody ballSwingerRigidbody;             // the balls rigidbody
    Vector3 laserEndPosition;                   // where our laser stops
    float maxLength = 100f;                     // the max length our laser can travel
    int layerMask;                              // the layermask of what our ray can hit
    bool laserEnabled = true;

    // Use this for initialization
    void Start ()
    {
        // Assign declared variables
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        lineRenderer = GetComponent<LineRenderer>();
        isHooked = false;
        playerManager = GameObject.Find("[CameraRig]").GetComponent<PlayerManager>();
        cameraEyeTransform = GameObject.Find("Camera (eye)").transform;
        followMeCameraRigScript = followMeCameraRig.GetComponent<FollowMeCameraRig>();
        followMeRigidbody = followMeCameraRig.GetComponent<Rigidbody>();
        myContainerScript = myContainer.GetComponent<Container>();

        // Create the layermask. Add everything we don't want to hit to the mask, then reverse the mask
        layerMask = LayerMask.GetMask("BallSwinger", "BallContainer", "FollowMeCameraRig", "LandingGroundSensor");
        layerMask = ~layerMask;
    }

    // Update is called once per frame
    void Update ()
    {
        // Create the device, Ray, and RaycastHit
        var device = SteamVR_Controller.Input((int)trackedObj.index);
        Ray ray = new Ray(transform.position, shotReferenceTransform.up);
        RaycastHit raycastHit;

        // FOR TESTING PURPOSES: PAUSE GAME
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
        {
            print("You clicked the pause button! PlayerStatus is " + playerManager.currentStatus);
            Time.timeScale = 0f;
        }


        // Handles if we're NOT hooked
        if (!isHooked)
        {
            // Set this as the laser's end position for starters. It may change later in the script
            laserEndPosition = transform.position + shotReferenceTransform.up * maxLength;

            // Handles if we're not on the ground
            if (playerManager.currentStatus != PlayerStatus.Grounded)
            {
                // If we're actually pointing at an object...
                if (Physics.Raycast(ray, out raycastHit, maxLength, layerMask))
                {
                    // Ready our laser to stop there
                    laserEndPosition = raycastHit.point;

                    if (raycastHit.collider.gameObject.CompareTag("Swingable") || raycastHit.collider.gameObject.CompareTag("MovingSwingable"))
                    {
                        lineRenderer.material = swingableMaterial;

                        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                        {
                            // ...shoot the grapple
                            ShootGrapple(raycastHit.point, raycastHit.transform);
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
        }
        // Handles if we're hooked
        else
        {
            // If we release the trigger...
            if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                // If the other grapple isn't hooked and we're not on the ground, then we're in freefall, so...
                if (!otherGrapple.isHooked && playerManager.currentStatus != PlayerStatus.Grounded)
                {
                    // Let the player manager know
                    playerManager.SwitchToStatus(PlayerStatus.Midair);
                }

                Unhook();
            }
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
        // Flag ourselves hooked
        isHooked = true;
        ToggleLaser(false);
        playerManager.SwitchToStatus(PlayerStatus.Hooked);

        // Activate our container, and set it to the appropriate place
        myContainer.SetActive(true);
        myContainer.transform.position = inputHitLocation;
        myContainerScript.SetDistanceFromOrigin();
        myContainerScript.LatchToObject(inputHitObjectTransform);

        myRopeScript.AttachToTarget(inputHitLocation);
    }

    /// <summary>
    /// Deactivates our container and flag ourselves as not hooked. Doesn't unhook the other grapple (if it's hooked)
    /// </summary>
    public void Unhook()
    {
        myContainer.SetActive(false);
        isHooked = false;
        myRopeScript.DetachFromTarget();
        ToggleLaser(true);
    }

    public void ToggleLaser(bool inputBool)
    {
        laserEnabled = inputBool;
        lineRenderer.enabled = inputBool;
    }
}
