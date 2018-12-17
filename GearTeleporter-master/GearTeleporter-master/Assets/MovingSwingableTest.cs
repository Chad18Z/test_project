using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSwingableTest : MonoBehaviour {

    [SerializeField] float secondsBeforeReverse = 3f;
    float speed = 10f;

    void Start()
    {
        InvokeRepeating("Reverse", secondsBeforeReverse, secondsBeforeReverse);
    }

    // Update is called once per frame
    void Update ()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
	}

    void Reverse()
    {
        speed *= -1f;
    }
}
