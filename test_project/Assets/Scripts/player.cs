using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour {

    Rigidbody rb;

	// Use this for initialization
	void Start () {

        rb = gameObject.GetComponent<Rigidbody>();
	}
	
	// Get input from the user and move the ship accordingly
	void Update () {

        Vector3 tempPosition = rb.transform.position;

        if (Input.GetKey("w"))
        {
            rb.AddForce(Vector3.up, ForceMode.Acceleration);
        }
        if (Input.GetKey("s"))
        {
            rb.AddForce(Vector3.down, ForceMode.Acceleration);
        }
        if (Input.GetKey("a"))
        {
            rb.AddForce(Vector3.left, ForceMode.Acceleration);
        }
        if (Input.GetKey("d"))
        {
            rb.AddForce(Vector3.right, ForceMode.Acceleration);
        }

        rb.transform.position = tempPosition;
	}
}
