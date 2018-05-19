using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingSensor : MonoBehaviour {

    [SerializeField] Transform cameraEyeTransform;
    [SerializeField] Transform cameraRigTransform;
    [SerializeField] PlayerManager playerManager;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    void OnTriggerEnter(Collider other)
    {
        float rayLength = cameraEyeTransform.position.y - cameraRigTransform.position.y;

        Ray ray = new Ray(cameraEyeTransform.position, Vector3.down);
        RaycastHit raycastHit;
        int bitMask = 1 << 8 | 1 << 9;
        bitMask = ~bitMask;

        if (Physics.Raycast(ray, out raycastHit, rayLength, bitMask))
        {
            print("my ray hit " + raycastHit.collider.gameObject.name);
            cameraRigTransform.position = new Vector3(cameraRigTransform.position.x, raycastHit.point.y, cameraRigTransform.position.z);
        }

        playerManager.SwitchToLanded();
    }
}
