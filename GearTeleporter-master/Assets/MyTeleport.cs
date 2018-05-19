using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyTeleport : MonoBehaviour {

    public Bezier bezier;
    public GameObject teleportEndSprite;
    [SerializeField] Transform cameraRigTransform;
    [SerializeField] Transform playerEyeTransform;

    Grapple grapple;
    SteamVR_TrackedObject trackedObj;

	// Use this for initialization
	void Start ()
    {
        grapple = GetComponent<Grapple>();
        trackedObj = GetComponent<SteamVR_TrackedObject>();

        bezier.ExtensionFactor = -.5f;
        teleportEndSprite.SetActive(false);
	}

    // Update is called once per frame
    void Update()
    {
        var device = SteamVR_Controller.Input((int)trackedObj.index);
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
        {
            //grapple.laserActive = false;
            bezier.ToggleDraw(true);
        }
        else if (device.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
        {
            HandleArcMaking();
        }
        else if (device.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
        {
            //grapple.laserActive = true;

            if (bezier.endPointDetected) TeleportToPosition(bezier.EndPoint + TeleportOffset());
            teleportEndSprite.SetActive(false);
            bezier.ToggleDraw(false);
        }
    }

    private void HandleArcMaking()
    {
        if (bezier.endPointDetected)
        {
            teleportEndSprite.SetActive(true);
            teleportEndSprite.transform.position = bezier.EndPoint;
        }
        else
        {
            teleportEndSprite.SetActive(false);
        }
    }

    void TeleportToPosition(Vector3 inputPosition)
    {
        cameraRigTransform.position = inputPosition;
    }

    Vector3 TeleportOffset()
    {
        Vector3 flatDistanceFromCamToPlayer = new Vector3(cameraRigTransform.position.x - playerEyeTransform.position.x, 0f, cameraRigTransform.position.z - playerEyeTransform.position.z);

        return flatDistanceFromCamToPlayer;
    }
}
