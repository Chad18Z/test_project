using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Senses when we fall off a cliff
/// </summary>
public class GroundSensor : MonoBehaviour {

    [SerializeField] Transform cameraEyeTransform;
    [SerializeField] PlayerManager playerManager;
	
	// Update is called once per frame
	void Update ()
    {
        // Follow the headset's lateral position (not vertical)
        transform.position = new Vector3(cameraEyeTransform.position.x, transform.position.y, cameraEyeTransform.position.z);
	}


    void OnTriggerExit(Collider other)
    {
        playerManager.SwitchToFellOffCliff();
    }
}
