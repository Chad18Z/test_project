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

    SteamVR_TrackedObject trackedObj;
    GameObject newBall;
    GameObject newContainer;
    LineRenderer lineRenderer;
    Vector3 previousControllerPosition;
    Vector3 previousBallPosition;
    Vector3 previousCameraRigPosition;
    Vector3 endPosition;
    float maxLength = 100f;

	// Use this for initialization
	void Start ()
    {
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

        if (Physics.Raycast(ray, out raycastHit, maxLength))
        {
            endPosition = raycastHit.point;

            // If the object we're pointing at is swingable...
            if (raycastHit.collider.gameObject.tag == "Swingable")
            {
                lineRenderer.material = swingableMaterial;

                // If we're airborne and press the trigger...
                if (!playerManager.onGround && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                {
                    newBall = Instantiate(ballSwinger, transform.position, Quaternion.identity);
                    Vector3 myCurrentVelocity = (cameraRigTransform.position - previousCameraRigPosition) / Time.deltaTime;
                    newBall.GetComponent<Rigidbody>().velocity = myCurrentVelocity;
                    previousBallPosition = newBall.transform.position;

                    newContainer = Instantiate(ballContainer, raycastHit.point, Quaternion.identity);
                    Container newContainerScript = newContainer.GetComponent<Container>();
                    newContainerScript.targetTransform = newBall.transform;
                    float newBallRadius = newBall.GetComponent<SphereCollider>().radius * newBall.transform.localScale.x;
                    float distFromContainer = (newContainer.transform.position - newBall.transform.position).magnitude;
                    newContainerScript.SetDistanceFromOrigin(distFromContainer + newBallRadius);

                    playerManager.SwitchToHookedFromMidair();
                }
            }
        }
        else
        {
            lineRenderer.material = normalMaterial;
        }

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
}
