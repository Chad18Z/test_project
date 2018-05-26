﻿using System.Collections;
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
    [SerializeField] GameObject followMeCameraRig;

    [HideInInspector] public bool isHooked;                             // is this grapple currently hooked?
    [HideInInspector] public bool hookInvoked;

    public GameObject myContainer;              // this hand's cube that blocks us

    FollowMeCameraRig followMeCameraRigScript;
    Rigidbody followMeRigidbody;
    PlayerManager playerManager;                // the PlayerManager attached to CameraRig
    Transform cameraEyeTransform;
    SteamVR_TrackedObject trackedObj;           // this hand
    LineRenderer lineRenderer;                  // this hand's line renderer
    Rigidbody ballSwingerRigidbody;             // the balls rigidbody
    Vector3 laserEndPosition;                   // where our laser stops
    Vector3 invokedSpotToHook;
    float maxLength = 100f;                     // the max length our laser can travel
    float previousYCoord;
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
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
        {
            print("You clicked the pause button! :O");
            Time.timeScale = 0f;
        }

        // If we're allowed to shoot our laser...
        if (laserEnabled)
        {
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
                        // If the player manager says we're not grounded...
                        if (!playerManager.onGround)
                        {
                            // ...if the other grapple has hook invoked...
                            if (otherGrapple.hookInvoked)
                            {
                                // ...hook invoke ourselves
                                hookInvoked = true;
                                invokedSpotToHook = raycastHit.point;
                                previousYCoord = playerManager.transform.position.y;
                            }
                            // Otherwise, the other grapple isn't invoked, so...
                            else
                            {
                                // ...shoot the grapple and notify the PlayerManager
                                ShootGrapple(raycastHit.point);

                                playerManager.SwitchToHookedFromMidair();
                            }
                        }
                        // Otherwise, we're on the ground, so...
                        else
                        {
                            // ...jump, and invoke hooking
                            playerManager.Jump();
                            hookInvoked = true;
                            invokedSpotToHook = raycastHit.point;
                            previousYCoord = playerManager.transform.position.y;
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

                // Deactivate our container and flag ourselves as not hooked
                myContainer.SetActive(false);
                isHooked = false;
            }

            // ...if hooking is invoked...
            if (hookInvoked)
            {
                // ...uninvoke it
                hookInvoked = false;
                lineRenderer.material = hookInvokedMaterial;
            }
        }

        // If hooking is invoked...
        if (hookInvoked)
        {
            // Change the laser material
            lineRenderer.material = hookInvokedMaterial;
            laserEndPosition = invokedSpotToHook;
            
            // ...if we're falling...
            if (playerManager.transform.position.y < previousYCoord)
            {
                // ...shoot the grapple in the invoked spot and disable hookInvoked
                ShootGrapple(invokedSpotToHook);
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
    private void ShootGrapple(Vector3 inputHitLocation)
    {
        // Flag ourselves hooked
        isHooked = true;

        // Activate the ball and put it in position at our head
        ballSwinger.SetActive(true);
        ballSwinger.transform.position = cameraEyeTransform.position;
        ballSwingerRigidbody.velocity = followMeRigidbody.velocity;

        // Update the position of Follow Me, Camera Rig
        followMeCameraRigScript.SetPosition();

        // If we're the first to get hooked, joint connect Follow Me to the ball
        if (!otherGrapple.isHooked)
        {
            followMeCameraRigScript.ConnectToBall();
            followMeRigidbody.velocity = ballSwingerRigidbody.velocity;
        }

        // Activate our container, and set it to the appropriate place
        myContainer.SetActive(true);
        myContainer.transform.position = inputHitLocation;
        myContainer.GetComponent<Container>().SetDistanceFromOrigin();
        
        // If the other grapple is hooked...
        if (otherGrapple.isHooked)
        {
            // ...reset their container's length, in case the player moved his head
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
