using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleSphere : MonoBehaviour {

    Vector3 target; // destination
    LineRenderer line; // this will be the "rope"
    SteamVR_TrackedObject controller;

    private void Start()
    {
        line = gameObject.GetComponent<LineRenderer>();
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
		
        // don't begin LERPing until we have a target
        if (target != null)
        {
            transform.position = Vector3.Lerp(transform.position, target, .05f);
            line.SetPosition(0, controller.transform.position);
            line.SetPosition(1, transform.position);
        }

        // when grappleSphere reaches its intended destination, destroy it
        if (transform.position == target)
        {
            Destroy(gameObject);
        }
	}
}
