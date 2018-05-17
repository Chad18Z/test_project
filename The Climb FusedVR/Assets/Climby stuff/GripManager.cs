using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GripManager : MonoBehaviour {

    public Rigidbody body;
    public Pull left;
    public Pull right;

    // Use this for initialization
    void Start () {
		
	}
    
    // Update is called once per frame
    void FixedUpdate()
    {
        var ldevice = SteamVR_Controller.Input((int)left.controller.index);
        var rdevice = SteamVR_Controller.Input((int)right.controller.index);

        bool isGripped = left.canGrip || right.canGrip;

        if (isGripped)
        {
            if (left.canGrip && ldevice.GetPress(SteamVR_Controller.ButtonMask.Grip))
            {
                body.useGravity = false;
                body.isKinematic = true;
                body.transform.position += (left.previousPosition - left.transform.localPosition);
            }
            else if (left.canGrip && ldevice.GetPressUp(SteamVR_Controller.ButtonMask.Grip))
            {
                body.useGravity = true;
                body.isKinematic = false;
                body.velocity = (left.previousPosition - left.transform.localPosition) / Time.deltaTime;
            }

            if (right.canGrip && rdevice.GetPress(SteamVR_Controller.ButtonMask.Grip))
            {
                body.useGravity = false;
                body.isKinematic = true;
                body.transform.position += (right.previousPosition - right.transform.localPosition);
            }
            else if (right.canGrip && rdevice.GetPressUp(SteamVR_Controller.ButtonMask.Grip))
            {
                body.useGravity = true;
                body.isKinematic = false;
                body.velocity = (right.previousPosition - right.transform.localPosition) / Time.deltaTime;
            }
        }
        else
        {
            body.useGravity = true;
            body.isKinematic = false;
        }

        left.previousPosition = left.transform.localPosition;
        right.previousPosition = right.transform.localPosition;
    }
}
