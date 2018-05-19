using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    [HideInInspector] public bool onGround;
    [HideInInspector] public bool hooked;
    [SerializeField] GameObject followMeCameraRig;
    [SerializeField] GameObject landingSensor;

    // Use this for initialization
    void Start ()
    {
        onGround = true;
        followMeCameraRig.SetActive(false);
	}

    public void SwitchToHookedFromMidair()
    {
        followMeCameraRig.SetActive(false);
        landingSensor.SetActive(true);
        onGround = false;
        hooked = true;
    }

    public void SwitchToLanded()
    {
        followMeCameraRig.SetActive(false);
        landingSensor.SetActive(false);
        onGround = true;
        hooked = false;
    }

    public void SwitchToFellOffCliff()
    {
        followMeCameraRig.SetActive(true);
        landingSensor.SetActive(true);
        onGround = false;
    }

    public void SwitchToMidairFromHooked(Vector3 inputVelocity)
    {
        followMeCameraRig.SetActive(true);
        followMeCameraRig.GetComponent<Rigidbody>().velocity = inputVelocity;

        landingSensor.SetActive(true);
        onGround = false;
        hooked = false;
    }
}
