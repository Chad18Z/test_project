using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Teleports and jumps
/// </summary>
public class MyTeleport : MonoBehaviour {

    public Bezier bezier;                               // the Bezier script attached to this hand's child named BezierCurve
    public GameObject teleportEndSprite;                // the object to put where the teleport hits
    [SerializeField] Transform cameraRigTransform;      // the Camera Rig object's transform
    [SerializeField] Transform playerEyeTransform;      // the head's transform

    PlayerManager playerManager;
    SteamVR_TrackedObject trackedObj;                   // steam's special little tracked object script
    int framesToTeleport = 5;
    bool teleportAllowed;
    // Jump variables
    Vector2 initialTouchDownLocation;
    Vector2 finalTouchLocation;
    float startTouchTime;
    float maxSwipeTime = .33f;
    float swipeDistance = 0.6f;
    bool swipeTentative = false;

    // Use this for initialization
    void Start ()
    {
        // Assign declared variables
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        teleportAllowed = true;
        playerManager = GameObject.Find("[CameraRig]").GetComponent<PlayerManager>();

        // Set our travel distance shorter and turn off the teleporter sprite
        bezier.ExtensionFactor = -.5f;
        teleportEndSprite.SetActive(false);
	}

    // Update is called once per frame
    void Update()
    {
        // Get this device
        var device = SteamVR_Controller.Input((int)trackedObj.index);

        // If we're allowed to teleport (which means we should be allowed to jump too)...
        if (teleportAllowed)
        {
            // JUMP CHECK
            // Check if the player touched the pad
            if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad))
            {
                // Record the starting data and flag as swipe tentative
                initialTouchDownLocation = device.GetAxis();
                startTouchTime = Time.time;
                swipeTentative = true;
            }

            // If we COULD be releasing touching the touchpad for a jump
            if (swipeTentative)
            {
                // If the player releases touching the touchpad...
                if (device.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad))
                {
                    swipeTentative = false;
                    Vector2 swipeDifference = finalTouchLocation - initialTouchDownLocation;

                    // If we swiped at least a certain distance and swiped upwards and we swiped up more than to the side...
                    if (swipeDifference.magnitude >= swipeDistance && swipeDifference.y > 0f && swipeDifference.y > Mathf.Abs(swipeDifference.x))
                    {
                        // Successful jump! :D
                        playerManager.Jump();
                    }
                }
                // Otherwise...
                else
                {
                    // Update the last place their finger touched, and see if they've missed their time period to swipe
                    finalTouchLocation = device.GetAxis();
                    if (Time.time - startTouchTime > maxSwipeTime) swipeTentative = false;
                }
            }


            // HANDLE TELEPORT
            // If we press down on the touchpad, turn the bezier curve on
            if (device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
            {
                bezier.ToggleDraw(true);
            }
            // If we're mid-holding down the touchpad, keep it on
            else if (device.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
            {
                HandleEndPoint();
            }
            // If we let go of the touchpad button...
            else if (device.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
            {
                // If it actually hit an endpoint, teleport there and turn off the bezier
                if (bezier.endPointDetected) StartCoroutine(TeleportCameraRigToPosition(bezier.EndPoint + TeleportOffset()));
                teleportEndSprite.SetActive(false);
                bezier.ToggleDraw(false);
            }
        }
    }



    //////////////////////////////////////////////////////
    /////////////////// CUSTOM METHODS ///////////////////
    //////////////////////////////////////////////////////

    /// <summary>
    /// Makes the arc
    /// </summary>
    private void HandleEndPoint()
    {
        // If the bezier hit something, activate the teleportEndSprite
        if (bezier.endPointDetected)
        {
            teleportEndSprite.SetActive(true);
            teleportEndSprite.transform.position = bezier.EndPoint;
        }
        // Otherwise, deactivate it
        else
        {
            teleportEndSprite.SetActive(false);
        }
    }

    /// <summary>
    /// Teleports you to the input position
    /// </summary>
    /// <param name="inputPosition"></param>
    void TeleportToPos(Vector3 inputPosition)
    {
        cameraRigTransform.position = inputPosition;
    }

    /// <summary>
    /// Returns the lateral vector distance from the Camera Rig to the player's head
    /// </summary>
    /// <returns></returns>
    Vector3 TeleportOffset()
    {
        Vector3 flatDistanceFromCamToPlayer = new Vector3(cameraRigTransform.position.x - playerEyeTransform.position.x, 0f, cameraRigTransform.position.z - playerEyeTransform.position.z);

        return flatDistanceFromCamToPlayer;
    }

    public void DisableTeleport()
    {
        teleportAllowed = false;
        bezier.ToggleDraw(false);
    }

    public void EnableTeleport()
    {
        teleportAllowed = true;
    }

    IEnumerator TeleportCameraRigToPosition(Vector3 inputPosition)
    {
        var waitForFixedUpdate = new WaitForFixedUpdate();

        Vector3 positionDifference = inputPosition - cameraRigTransform.position;
        Vector3 movementPerFrame = positionDifference / (float)framesToTeleport;

        for (int i = 0; i < framesToTeleport; i++)
        {
            cameraRigTransform.position += movementPerFrame;

            yield return waitForFixedUpdate;
        }
    }
}
