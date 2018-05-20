using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour {

    [HideInInspector] public bool laserActive;

    [SerializeField] Transform shotReferenceTransform;
    [SerializeField] GameObject ballSwinger;
    [SerializeField] GameObject ballContainer;
    [SerializeField] Transform cameraRigTransform;
    [SerializeField] PlayerManager playerManager;
    [SerializeField] Material normalMaterial;
    [SerializeField] Material swingableMaterial;
    [SerializeField] Material swingingMaterial;

    [HideInInspector] public GameObject newBall;
    [HideInInspector] public GameObject newContainer;
    SteamVR_TrackedObject trackedObj;
    LineRenderer lineRenderer;
    Vector3 previousControllerPosition;
    Vector3 previousBallPosition;
    Vector3 previousCameraRigPosition;
    Vector3 endPosition;
    float maxLength = 100f;

	// Use this for initialization
	void Start ()
    {
        // Assign declared variables
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        lineRenderer = GetComponent<LineRenderer>();

        laserActive = true;
	}
	
	// Update is called once per frame
	void Update ()
    {
        var device = SteamVR_Controller.Input((int)trackedObj.index);
        Ray ray = new Ray(transform.position, shotReferenceTransform.up);
        RaycastHit raycastHit;
        
        endPosition = transform.position + shotReferenceTransform.up * maxLength;

        // If we're actually pointing at an object...
        if (Physics.Raycast(ray, out raycastHit, maxLength))
        {
            // Set our laser to stop there
            endPosition = raycastHit.point;

            // If the object we're pointing at is swingable...
            if (raycastHit.collider.gameObject.tag == "Swingable")
            {
                // Change the laser color
                lineRenderer.material = swingableMaterial;

                // If we press the trigger...
                if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                {
                    // If we're airborne...
                    if (!playerManager.onGround)
                    {
                        ShootGrapple(raycastHit.point);

                        playerManager.SwitchToHookedFromMidair();
                    }
                    // If we're on the ground...
                    else
                    {
                        playerManager.Jump();
                    }
                }
            }
        }
        else
        {
            lineRenderer.material = normalMaterial;
        }

        // If the player is hooked and the trigger is RELEASED...
        if (playerManager.hooked && device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
        {
            Vector3 currentVelocity = Vector3.zero;
            if (newBall) currentVelocity = newBall.GetComponent<Rigidbody>().velocity;
            Destroy(newBall);
            Destroy(newContainer);
            playerManager.SwitchToMidairFromHooked(currentVelocity);
        }

        if (playerManager.hooked)
        {
            lineRenderer.material = swingingMaterial;

            cameraRigTransform.position += (previousControllerPosition - trackedObj.transform.position);
            cameraRigTransform.position += (newBall.transform.position - previousBallPosition);
            endPosition = newContainer.transform.position;
        }

        previousCameraRigPosition = cameraRigTransform.position;
        previousControllerPosition = transform.position;
        if (newBall != null) previousBallPosition = newBall.transform.position;
        
    }

    void LateUpdate()
    {
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, endPosition);
    }



    /// <summary>
    /// Shoots the grapple, with the container centered at the input Vector3
    /// </summary>
    /// <param name="inputHitLocation"></param>
    private void ShootGrapple(Vector3 inputHitLocation)
    {
        newBall = Instantiate(ballSwinger, transform.position, Quaternion.identity);
        print("Ball: " + newBall.transform.position + " Controller: " + transform.position);
        Vector3 myCurrentVelocity = (cameraRigTransform.position - previousCameraRigPosition) / Time.deltaTime;
        newBall.GetComponent<Rigidbody>().velocity = myCurrentVelocity;
        previousBallPosition = newBall.transform.position + -(myCurrentVelocity * Time.deltaTime);

        newContainer = Instantiate(ballContainer, inputHitLocation, Quaternion.identity);
        Container newContainerScript = newContainer.GetComponent<Container>();
        newContainerScript.targetTransform = newBall.transform;
        float newBallRadius = newBall.GetComponent<SphereCollider>().radius * newBall.transform.localScale.x;
        float distFromContainer = (newContainer.transform.position - newBall.transform.position).magnitude;
        newContainerScript.SetDistanceFromOrigin(distFromContainer + newBallRadius);
    }
}
