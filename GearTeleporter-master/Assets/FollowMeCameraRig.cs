﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Activated when the player is in freefall or hooked. Makes Camera Rig follow it, and is attached to the ball swinger (if it exists)
/// </summary>
public class FollowMeCameraRig : MonoBehaviour
{
    [SerializeField] GameObject ballSwinger;                            // the ball swinger
    [SerializeField] MyTeleport leftMyTeleportScript;
    [SerializeField] MyTeleport rightMyTeleportScript;

    Transform cameraRigTransform;                       // the transform of our Camera Rig
    Transform cameraEyeTransform;                       // the transform of our headset
    Rigidbody rigidBody;                                // our rigidbody
    Rigidbody ballSwingerRigidbody;                     // the ball swinger's rigidbody
    CapsuleCollider capsuleCollider;                    // our capsule collider component

	// Use this for initialization
	void Awake ()
    {
        // Set all the declared variables and deactivate ourselves
        cameraRigTransform = GameObject.Find("[CameraRig]").transform;
        cameraEyeTransform = GameObject.Find("Camera (eye)").transform;
        rigidBody = GetComponent<Rigidbody>();
        ballSwingerRigidbody = ballSwinger.GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Make the camera rig be where we are, PLUS the offset of where the headset is vs the origin of Camera Rig
        cameraRigTransform.position = transform.position + (cameraRigTransform.position - cameraEyeTransform.position);
    }

    void OnEnable()
    {
        // Teleport to the headset position, then offset the camera rig like in update
        transform.position = cameraEyeTransform.position;
        cameraRigTransform.position = transform.position + (cameraRigTransform.position - cameraEyeTransform.position);

        // Update the capsule collider height, and keep that happening ever couple seconds, just to make sure it stays right
        InvokeRepeating("SetCollider", 0.000001f, 2f);

        // Whenever this puppy is on, we're not in a position where we should be allowed to teleport
        leftMyTeleportScript.DisableTeleport();
        rightMyTeleportScript.DisableTeleport();
    }

    void OnDisable()
    {
        // Stop repeating setting the collider for next time it's activated
        CancelInvoke();

        // When turned off, we should be allowed to teleport
        leftMyTeleportScript.EnableTeleport();
        rightMyTeleportScript.EnableTeleport();
    }


    //////////////////////////////////////////////////////
    /////////////////// CUSTOM METHODS ///////////////////
    //////////////////////////////////////////////////////

    /// <summary>
    /// Add a fixed joint component that attaches to the ball swinger
    /// </summary>
    public void ConnectToBall()
    {
        FixedJoint fixedJoint = gameObject.AddComponent<FixedJoint>();
        fixedJoint.connectedBody = ballSwingerRigidbody;
    }

    /// <summary>
    /// Delete the fixed joint from the ball
    /// </summary>
    public void DisconnectFromBall()
    {
        Destroy(GetComponent<FixedJoint>());
    }

    /// <summary>
    /// Sets Follow Me's position to be right on the ball
    /// </summary>
    public void SetPosition()
    {
        transform.position = ballSwinger.transform.position;
    }

    /// <summary>
    /// Sets the height of the collider according to how high the players headset is above the ground
    /// </summary>
    private void SetCollider()
    {
        float currentHeight = transform.position.y - cameraRigTransform.position.y;
        currentHeight -= .2f;
        capsuleCollider.height = currentHeight;

        if (currentHeight > 2f * capsuleCollider.radius)
        {
            capsuleCollider.center = new Vector3(capsuleCollider.center.x, -currentHeight / 2, capsuleCollider.center.z);
        }
        else
        {
            capsuleCollider.center = new Vector3(capsuleCollider.center.x, currentHeight / 2, capsuleCollider.center.z);
        }
    }
}
