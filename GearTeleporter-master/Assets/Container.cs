using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour {

    float cubeHalfWidth;
    float ballSwingerRadius;
    public Transform targetTransform;

    void Start()
    {
        cubeHalfWidth = transform.GetChild(0).transform.localScale.x / 2f;
        ballSwingerRadius = targetTransform.GetComponent<SphereCollider>().radius * targetTransform.localScale.x;

        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update ()
    {
        Vector3 directionToLook = targetTransform.position - transform.position;
        transform.forward = directionToLook;
	}

    public void SetDistanceFromOrigin()
    {
        Vector3 directionToLook = targetTransform.position - transform.position;
        transform.forward = directionToLook;

        float distFromTarget = (transform.position - targetTransform.position).magnitude;
        transform.GetChild(0).transform.localPosition = new Vector3(0f, 0f, distFromTarget + ballSwingerRadius + cubeHalfWidth + 0.02f);
    }
}
