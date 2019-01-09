using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSwinger : MonoBehaviour
{
    [HideInInspector] public bool touchingContainer;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("BallSwinger"))
        {
            touchingContainer = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("BallSwinger"))
        {
            touchingContainer = false;
        }
    }
}
