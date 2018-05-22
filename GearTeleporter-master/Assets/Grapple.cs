using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour {
    
    [SerializeField] Transform shotReferenceTransform;
    [SerializeField] GameObject ballSwinger;
    [SerializeField] GameObject ballContainer;
    [SerializeField] Transform cameraRigTransform;
    [SerializeField] PlayerManager playerManager;
    [SerializeField] Grapple otherGrapple;
    [SerializeField] Material normalMaterial;
    [SerializeField] Material swingableMaterial;
    [SerializeField] Material swingingMaterial;
    
    [HideInInspector] public bool isHooked;
    [HideInInspector] public bool ballSwingerOwner;
    [HideInInspector] public Vector3 previousBallPosition;
    [HideInInspector] public bool cameraRigMoveDisabledThisFrame;

    public GameObject myContainer;

    SteamVR_TrackedObject trackedObj;
    LineRenderer lineRenderer;
    Vector3 previousControllerPosition;
    Vector3 previousCameraRigPosition;
    Vector3 laserEndPosition;
    Vector3 ensuredVelocity;
    float maxLength = 100f;
    int layerMask;

    // Use this for initialization
    void Start ()
    {
        // Assign declared variables
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        lineRenderer = GetComponent<LineRenderer>();
        isHooked = false;
        ballSwingerOwner = false;
        cameraRigMoveDisabledThisFrame = false;

        layerMask = LayerMask.GetMask("BallSwinger", "BallContainer");
        layerMask = ~layerMask;
    }

    // Update is called once per frame
    void Update ()
    {
        var device = SteamVR_Controller.Input((int)trackedObj.index);
        Ray ray = new Ray(transform.position, shotReferenceTransform.up);
        RaycastHit raycastHit;
        
        laserEndPosition = transform.position + shotReferenceTransform.up * maxLength;

        // If we're actually pointing at an object...
        if (Physics.Raycast(ray, out raycastHit, maxLength, layerMask))
        {
            // Set our laser to stop there
            laserEndPosition = raycastHit.point;

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
                        print("I fired at " + Time.time);
                        ShootGrapple(raycastHit.point);
                        isHooked = true;

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

        // If this grapple is hooked and the trigger is RELEASED...
        if (isHooked && device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
        {
            // If the other grapple isn't hooked...
            if (!otherGrapple.isHooked)
            {
                Vector3 currentVelocity = ballSwinger.GetComponent<Rigidbody>().velocity;
                DeactivateBall();
                myContainer.SetActive(false);
                playerManager.SwitchToMidairFromHooked(currentVelocity);
            }
            // Otherwise, the other grapple IS hooked, so...
            else
            {
                // If this is the ball swinger owner...
                if (ballSwingerOwner)
                {
                    // Declare the other grapple as the righteous owner of the sacred ball :O
                    ballSwingerOwner = false;
                    otherGrapple.ballSwingerOwner = true;

                    // Deactivate our container, and reset the other guy's
                    myContainer.SetActive(false);
                    Vector3 otherContainerOrigin = otherGrapple.myContainer.transform.position;
                    ballSwinger.transform.position = otherGrapple.transform.position;
                    otherGrapple.myContainer.transform.position = otherContainerOrigin;
                    otherGrapple.myContainer.GetComponent<Container>().SetDistanceFromOrigin();
                    Vector3 ballCurrentVelocity = ballSwinger.GetComponent<Rigidbody>().velocity;
                    otherGrapple.previousBallPosition = ballSwinger.transform.position + -(ballCurrentVelocity * Time.deltaTime);
                }
                // Otherwise, this wasn't the ball swinger owner, so...
                else
                {
                    myContainer.SetActive(false);
                    otherGrapple.ballSwingerOwner = true;
                    ballSwingerOwner = false;
                }
            }

            isHooked = false;
            ballSwingerOwner = false;
        }

        // If this grapple is hooked...
        if (isHooked)
        {
            // If we're the righteous ball owner and we're not on that frame where we shouldn't move...
            if (ballSwingerOwner && !cameraRigMoveDisabledThisFrame)
            {
                cameraRigTransform.position += (previousControllerPosition - trackedObj.transform.position);
                cameraRigTransform.position += (ballSwinger.transform.position - previousBallPosition);
            }

            // Set line renderer's material and end position
            lineRenderer.material = swingingMaterial;
            laserEndPosition = myContainer.transform.position;
        }

        previousCameraRigPosition = cameraRigTransform.position;
        previousControllerPosition = transform.position;
        if (ballSwinger.activeSelf) previousBallPosition = ballSwinger.transform.position;

        cameraRigMoveDisabledThisFrame = false;
    }

    void LateUpdate()
    {
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, laserEndPosition);
    }


    // TODO: fix my current velocity. What if we weren't just midair?
    /// <summary>
    /// Shoots the grapple, with the container centered at the input Vector3
    /// </summary>
    /// <param name="inputHitLocation"></param>
    private void ShootGrapple(Vector3 inputHitLocation)
    {
        // Become the righteous ballSwinger owner
        ballSwingerOwner = true;
        otherGrapple.ballSwingerOwner = false;

        ballSwinger.SetActive(true);
        Vector3 ballPositionBeforeTeleport = ballSwinger.transform.position;
        ballSwinger.transform.position = transform.position;
        Vector3 amountBallMoved = ballSwinger.transform.position - ballPositionBeforeTeleport;
        previousBallPosition += amountBallMoved;

        otherGrapple.cameraRigMoveDisabledThisFrame = true;

        // If the other grapple isn't hooked...
        if (!otherGrapple.isHooked)
        {
            ballSwinger.GetComponent<Rigidbody>().velocity = playerManager.currentVelocity;
            previousBallPosition = ballSwinger.transform.position + -(playerManager.currentVelocity * Time.deltaTime);
        }

        myContainer.SetActive(true);
        myContainer.transform.position = inputHitLocation;
        Container myContainerScript = myContainer.GetComponent<Container>();
        myContainerScript.SetDistanceFromOrigin();
        
        // If the other grapple is already hooked...
        if (otherGrapple.isHooked)
        {
            // ...set its container's new distance
            otherGrapple.myContainer.GetComponent<Container>().SetDistanceFromOrigin();
        }
    }



    public void DeactivateBall()
    {
        ballSwinger.SetActive(false);
    }
}
