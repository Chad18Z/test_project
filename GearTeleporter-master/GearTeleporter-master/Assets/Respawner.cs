using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawner : MonoBehaviour {

    GameObject cameraRig;
    Vector3 startLocation;

	// Use this for initialization
	void Start ()
    {
        cameraRig = GameObject.Find("[CameraRig]");
        startLocation = cameraRig.transform.position;
	}

    private void OnTriggerEnter(Collider other)
    {
        cameraRig.transform.position = startLocation;
        cameraRig.GetComponent<PlayerManager>().SwitchToStatus(PlayerStatus.Grounded);
    }
}
