using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pull : MonoBehaviour {

    [HideInInspector] public Vector3 previousPosition;
    [HideInInspector] public SteamVR_TrackedObject controller;
    public bool canGrip;

	// Use this for initialization
	void Start ()
    {
        controller = GetComponent<SteamVR_TrackedObject>();
        previousPosition = controller.transform.localPosition;
	}

    void OnTriggerEnter(Collider other)
    {
        canGrip = true;
    }

    void OnTriggerExit(Collider other)
    {
        canGrip = false;
    }
}
