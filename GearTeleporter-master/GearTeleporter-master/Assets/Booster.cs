using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Booster : MonoBehaviour {

    [SerializeField] Rigidbody followMeRigidbody;
    [SerializeField] Grapple leftGrapple;
    [SerializeField] Grapple rightGrapple;
    [SerializeField] float boostPower = 30f;

    PlayerManager playerManager;
    FollowMeCameraRig followMeScript;
    Vector3 forwardDirection;

	// Use this for initialization
	void Start ()
    {
        // Assign the declared variables
        followMeScript = followMeRigidbody.gameObject.GetComponent<FollowMeCameraRig>();
        forwardDirection = transform.forward.normalized;
        playerManager = GameObject.Find("[CameraRig]").GetComponent<PlayerManager>();
	}

    private void OnTriggerEnter(Collider other)
    {
        // If Follow Me Camera Rig hits me...
        if (other.gameObject.CompareTag("FollowMe"))
        {
            // Ungrapple
            leftGrapple.Unhook();
            rightGrapple.Unhook();
            followMeScript.DisconnectFromBall();
            playerManager.SwitchToStatus(PlayerStatus.Midair);

            // Send that bitch fuckin' FLYING
            followMeRigidbody.velocity = forwardDirection * boostPower;
        }
    }
}
