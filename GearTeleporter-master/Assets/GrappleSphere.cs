using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleSphere : MonoBehaviour {

    Vector3 target;
    Rigidbody rb;

    /// <summary>
    /// Public setter for target location
    /// </summary>
    public Vector3 Target
    {
        set { target = value; }
    }

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }


    // Update is called once per frame
    void Update () {
		
        if (target != null && transform.position != target)
        {
            transform.position = Vector3.Lerp(transform.position, target, .1f);
        }
	}
}
