using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class Teleporter : MonoBehaviour {

    SteamVR_TrackedObject trackedObj;
    TeleportArc myArc;

	// Use this for initialization
	void Start ()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        myArc = GetComponent<TeleportArc>();
	}

    // Update is called once per frame
    void Update()
    {
        RaycastHit myRayHit;

        var device = SteamVR_Controller.Input((int)trackedObj.index);
        if (device.GetTouch(SteamVR_Controller.ButtonMask.Trigger))
        {
            myArc.DrawArc(out myRayHit);
            myArc.Show();
            print(myRayHit.distance);
        }
    }
}
