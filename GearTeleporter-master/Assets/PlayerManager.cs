using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] Grapple leftHandGrapple;
    [SerializeField] Grapple rightHandGrapple;
    [SerializeField] GameObject landingSensor;

    [HideInInspector] public bool onGround;
    [HideInInspector] public Vector3 currentVelocity;

    Transform cameraEyeTransform;
    Rigidbody rigidBody;
    Vector3 previousPosition;
    float verticalJumpForce = 6f;
    float lateralJumpForce = 2f;

    // Use this for initialization
    void Start ()
    {
        previousPosition = transform.position;
        onGround = true;

        rigidBody = GetComponent<Rigidbody>();
        cameraEyeTransform = GameObject.Find("Camera (eye)").transform;

        // DELETE AFTER TESTING
        SwitchToFellOffCliff();
    }

    void FixedUpdate()
    {
        currentVelocity = (transform.position - previousPosition) / Time.deltaTime;

        previousPosition = transform.position;
    }





    public void SwitchToHookedFromMidair()
    {
        rigidBody.isKinematic = true;
        rigidBody.useGravity = false;

        landingSensor.SetActive(true);
        onGround = false;
    }

    public void SwitchToLanded()
    {
        // TODO: Add left hand grapple
        if (rightHandGrapple.isHooked)
        {
            rightHandGrapple.DeactivateBall();
            rightHandGrapple.myContainer.SetActive(false);
        }
        if (leftHandGrapple.isHooked)
        {
            leftHandGrapple.DeactivateBall();
            leftHandGrapple.myContainer.SetActive(false);
        }

        rigidBody.isKinematic = true;
        rigidBody.useGravity = false;

        landingSensor.SetActive(false);
        onGround = true;
    }

    public void SwitchToFellOffCliff()
    {
        rigidBody.isKinematic = false;
        rigidBody.useGravity = true;

        landingSensor.SetActive(true);
        onGround = false;
    }

    public void SwitchToMidairFromHooked(Vector3 inputVelocity)
    {
        rigidBody.isKinematic = false;
        rigidBody.useGravity = true;
        rigidBody.velocity = inputVelocity;

        landingSensor.SetActive(true);
        onGround = false;
    }
    
    public void Jump()
    {
        rigidBody.isKinematic = false;
        rigidBody.useGravity = true;
        Invoke("SetLandingSensorActive", .1f);
        onGround = false;
        
        Vector2 flattenedTargetVector = (new Vector2(cameraEyeTransform.forward.x, cameraEyeTransform.forward.z)).normalized * lateralJumpForce;
        Vector3 jumpVector = new Vector3(flattenedTargetVector.x, verticalJumpForce, flattenedTargetVector.y);
        rigidBody.AddForce(jumpVector, ForceMode.Impulse);
    }



    /// <summary>
    /// This method only exists so we can call "Invoke" on it
    /// </summary>
    private void SetLandingSensorActive()
    {
        landingSensor.SetActive(true);
    }
}
