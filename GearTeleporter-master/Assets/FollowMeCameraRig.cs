using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When this bad boy is enabled, it will cause the camera rig to follow it
/// </summary>
public class FollowMeCameraRig : MonoBehaviour {
    
    Transform cameraRigTransform;
    Transform cameraEyeTransform;
    Rigidbody rigidBody;

	// Use this for initialization
	void Awake ()
    {
        cameraRigTransform = GameObject.Find("[CameraRig]").transform;
        cameraEyeTransform = GameObject.Find("Camera (eye)").transform;
        rigidBody = GetComponent<Rigidbody>();
    }
	
    // TODO: probably make this LateUpdate (or maybe FixedUpdate?)
	// Update is called once per frame
	void Update ()
    {
        cameraRigTransform.position = transform.position + (cameraRigTransform.position - cameraEyeTransform.position);
	}

    void OnEnable()
    {
        transform.position = cameraEyeTransform.position;
        rigidBody.velocity = Vector3.zero;
        cameraRigTransform.position = transform.position + (cameraRigTransform.position - cameraEyeTransform.position);
    }
}
