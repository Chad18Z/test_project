using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawner : MonoBehaviour {

    [SerializeField] GameObject player;
    Vector3 spawnPoint;

	// Use this for initialization
	void Start ()
    {
        spawnPoint = player.transform.position;
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        other.transform.position = spawnPoint;
    }
}
