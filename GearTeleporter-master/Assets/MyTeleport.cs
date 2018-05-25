using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Teleports
/// </summary>
public class MyTeleport : MonoBehaviour {

    public Bezier bezier;                               // the Bezier script attached to this hand's child named BezierCurve
    public GameObject teleportEndSprite;                // the object to put where the teleport hits
    [SerializeField] Transform cameraRigTransform;      // the Camera Rig object's transform
    [SerializeField] Transform playerEyeTransform;      // the head's transform
    
    SteamVR_TrackedObject trackedObj;                   // steam's special little tracked object script
    bool teleportAllowed;

    // Use this for initialization
    void Start ()
    {
        // Assign declared variables
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        teleportAllowed = true;

        // Set our travel distance shorter and turn off the teleporter sprite
        bezier.ExtensionFactor = -.5f;
        teleportEndSprite.SetActive(false);
	}

    // Update is called once per frame
    void Update()
    {
        // Get this device
        var device = SteamVR_Controller.Input((int)trackedObj.index);

        if (teleportAllowed)
        {
            // If we press down on the touchpad, turn the bezier curve on
            if (device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
            {
                bezier.ToggleDraw(true);
            }
            // If we're mid-holding down the touchpad, keep it on
            else if (device.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
            {
                HandleArcMaking();
            }
            // If we let go of the touchpad button...
            else if (device.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
            {
                // If it actually hit an endpoint, teleport there and turn off the bezier
                if (bezier.endPointDetected) TeleportToPosition(bezier.EndPoint + TeleportOffset());
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
    private void HandleArcMaking()
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
    void TeleportToPosition(Vector3 inputPosition)
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
}
