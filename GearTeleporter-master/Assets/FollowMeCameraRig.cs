using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Activated when the player is in freefall or hooked. Makes Camera Rig follow it, and is attached to the ball swinger (if it exists)
/// </summary>
public class FollowMeCameraRig : MonoBehaviour
{
    [SerializeField] GameObject ballSwinger;

    [HideInInspector] public Vector3 currentVelocity;

    Transform cameraRigTransform;
    Transform cameraEyeTransform;
    Rigidbody rigidBody;
    Rigidbody ballSwingerRigidbody;
    CapsuleCollider capsuleCollider;

	// Use this for initialization
	void Awake ()
    {
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
        cameraRigTransform.position = transform.position + (cameraRigTransform.position - cameraEyeTransform.position);
    }

    void OnEnable()
    {
        transform.position = cameraEyeTransform.position;
        cameraRigTransform.position = transform.position + (cameraRigTransform.position - cameraEyeTransform.position);
        InvokeRepeating("SetCollider", 0.000001f, 2f);
    }






    public void ConnectToBall()
    {
        FixedJoint fixedJoint = gameObject.AddComponent<FixedJoint>();
        fixedJoint.connectedBody = ballSwingerRigidbody;
    }

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


    private void SetCollider()
    {
        float currentHeight = transform.position.y - cameraRigTransform.position.y;
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
