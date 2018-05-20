using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    [HideInInspector] public bool onGround;
    [HideInInspector] public bool hooked;

    // [SerializeField] Grapple leftHandGrapple;
    [SerializeField] Grapple rightHandGrapple;
    [SerializeField] GameObject landingSensor;

    Transform cameraEyeTransform;
    Rigidbody rigidbody;
    float verticalJumpForce = 6f;
    float lateralJumpForce = 2f;

    // Use this for initialization
    void Start ()
    {
        onGround = true;
        rigidbody = GetComponent<Rigidbody>();
        cameraEyeTransform = GameObject.Find("Camera (eye)").transform;
	}

    public void SwitchToHookedFromMidair()
    {
        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;

        landingSensor.SetActive(true);
        onGround = false;
        hooked = true;
    }

    public void SwitchToLanded()
    {
        // TODO: Add left hand grapple
        if (hooked)
        {
            Destroy(rightHandGrapple.newBall);
            Destroy(rightHandGrapple.newContainer);
        }

        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;

        landingSensor.SetActive(false);
        onGround = true;
        hooked = false;
    }

    public void SwitchToFellOffCliff()
    {
        rigidbody.isKinematic = false;
        rigidbody.useGravity = true;

        landingSensor.SetActive(true);
        onGround = false;
    }

    public void SwitchToMidairFromHooked(Vector3 inputVelocity)
    {
        rigidbody.isKinematic = false;
        rigidbody.useGravity = true;
        rigidbody.velocity = inputVelocity;

        landingSensor.SetActive(true);
        onGround = false;
        hooked = false;
    }
    
    public void Jump()
    {
        rigidbody.isKinematic = false;
        rigidbody.useGravity = true;
        Invoke("SetLandingSensorActive", .1f);
        onGround = false;
        
        Vector2 flattenedTargetVector = (new Vector2(cameraEyeTransform.forward.x, cameraEyeTransform.forward.z)).normalized * lateralJumpForce;
        Vector3 jumpVector = new Vector3(flattenedTargetVector.x, verticalJumpForce, flattenedTargetVector.y);
        rigidbody.AddForce(jumpVector, ForceMode.Impulse);
    }



    /// <summary>
    /// This method only exists so we can call "Invoke" on it
    /// </summary>
    private void SetLandingSensorActive()
    {
        landingSensor.SetActive(true);
    }
}
