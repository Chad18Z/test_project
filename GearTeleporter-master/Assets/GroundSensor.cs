using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSensor : MonoBehaviour {

    [SerializeField] Transform cameraEyeTransform;
    [SerializeField] PlayerManager playerManager;
	
	// Update is called once per frame
	void Update ()
    {
        transform.position = new Vector3(cameraEyeTransform.position.x, transform.position.y, cameraEyeTransform.position.z);
	}

    void OnTriggerEnter(Collider other)
    {
        //playerManager.onGround = true;
    }

    void OnTriggerExit(Collider other)
    {
        playerManager.SwitchToFellOffCliff();
    }
}
