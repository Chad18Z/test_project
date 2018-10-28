using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleSphere : MonoBehaviour {

    Vector3 target; // destination
    LineRenderer line; // this will be the "rope"
    SteamVR_TrackedObject controller;
    GrappleSphereStates grappleState;

    const float TESTDISTANCE = .1f; // must be within this distance of the target object before we're "Hooked"
    const float LERPVALUE = .1f; // the speed at which we Lerp from the controller to the object

    private void Start()
    {
        line = gameObject.GetComponent<LineRenderer>();

        grappleState = GrappleSphereStates.IDLE;
    }
    /// <summary>
    /// Give me a destination! Ensure that "controller" is set prior to target
    /// </summary>
    public Vector3 Target
    {
        set {
                if (controller != null) // ensure that controller has been set before creating a target
                {
                target = value;
                }
            }
    }

    /// <summary>
    /// Give me a reference to the controller which instantiated me. This MUST be set before "target"
    /// </summary>
    public SteamVR_TrackedObject Controller
    {
        set { controller = value; }
    }

    // Update is called once per frame
    void Update () {
        
        switch (grappleState)
        {
            case GrappleSphereStates.IDLE:
                Idle();
                break;

            case GrappleSphereStates.FIRED:
                Fired();
                break;

            case GrappleSphereStates.HOOKED:
                Hooked();
                break;

            case GrappleSphereStates.RETURNING:
                Returning();
                break;

            default:
                Debug.Log("No state selected");
                break;
        }

	}

    /// <summary>
    /// GrappleSphere's behavior whenever it is not doing anything
    /// </summary>
    void Idle()
    {
        if (target != null)
        {
            grappleState = GrappleSphereStates.FIRED;
        }
    }

    /// <summary>
    /// GrappleSphere's behavior whenever it is flying towards the target object
    /// </summary>
    void Fired()
    {
        var device = SteamVR_Controller.Input((int)controller.index); // get device ID
        if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger)) // check for trigger release
        {
            // if trigger is released, then we enter the "Returning" state
            grappleState = GrappleSphereStates.RETURNING;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, target, LERPVALUE);
            line.SetPosition(0, controller.transform.position);
            line.SetPosition(1, transform.position);
        }

        // when grappleSphere reaches its intended destination, destroy it
        if (Vector3.Distance(transform.position, target) < TESTDISTANCE)
        {
            transform.position = target;
            grappleState = GrappleSphereStates.HOOKED;
        }
    }

    /// <summary>
    /// GrappleSphere's behavior whenever it is hooked onto the target object
    /// </summary>
    void Hooked()
    {
        var device = SteamVR_Controller.Input((int)controller.index); // get device ID
        if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger)) // check for trigger release
        {
            // if trigger is released, then we enter the "Returning" state
            grappleState = GrappleSphereStates.RETURNING;
        }
        else
        {
            line.SetPosition(0, controller.transform.position);
            line.SetPosition(1, transform.position);
        }
    }

    /// <summary>
    /// GrappleSphere's behavior whenever it is returning to the controller from which it was instantiated
    /// </summary>
    void Returning()
    {
        transform.position = Vector3.Lerp(transform.position, controller.transform.position, LERPVALUE);
        line.SetPosition(0, transform.position);
        line.SetPosition(1, controller.transform.position);

        // when grappleSphere reaches its intended destination, destroy it
        if (Vector3.Distance(transform.position, controller.transform.position) < TESTDISTANCE)
        {
            Destroy(gameObject);
        }
    }
}
