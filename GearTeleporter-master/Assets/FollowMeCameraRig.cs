using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When this bad boy is enabled, it will cause the camera rig to follow it
/// </summary>
public class FollowMeCameraRig : MonoBehaviour {
    
    Transform cameraRigTransform;
    Transform cameraEyeTransform;
    Rigidbody rigidbody;

	// Use this for initialization
	void Awake ()
    {
        cameraRigTransform = GameObject.Find("[CameraRig]").transform;
        cameraEyeTransform = GameObject.Find("Camera (eye)").transform;
        rigidbody = GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        cameraRigTransform.position = transform.position + (cameraRigTransform.position - cameraEyeTransform.position);
	}

    void OnEnable()
    {
        transform.position = cameraEyeTransform.position;
        rigidbody.velocity = Vector3.zero;
        cameraRigTransform.position = transform.position + (cameraRigTransform.position - cameraEyeTransform.position);
    }
}
