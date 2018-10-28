using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleSphere : MonoBehaviour {

    Vector3 target;

    /// <summary>
    /// Public setter for target location
    /// </summary>
    public Vector3 Target
    {
        set { target = value; }
    }



    // Update is called once per frame
    void Update () {
		
        if (target != null && transform.position != target)
        {
            transform.position = Vector3.Lerp(transform.position, target, .05f);
        }
	}
}
