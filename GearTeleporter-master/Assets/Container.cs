using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour {

    public Transform targetTransform;
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 directionToLook = targetTransform.position - transform.position;
        transform.forward = directionToLook;
	}

    public void SetDistanceFromOrigin(float inputDistance)
    {
        float cubeHalfWidth = transform.GetChild(0).transform.localScale.x / 2f;
        transform.GetChild(0).transform.localPosition = new Vector3(0f, 0f, inputDistance + cubeHalfWidth + 0.02f);
    }
}
