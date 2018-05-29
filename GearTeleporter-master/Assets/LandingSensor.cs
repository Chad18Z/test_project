using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Senses if we landed, and puts us in the right place
/// </summary>
public class LandingSensor : MonoBehaviour {

    [SerializeField] Transform cameraEyeTransform;              // the Transform of our headset
    [SerializeField] Transform cameraRigTransform;              // the Transform of the Camera Rig object
    [SerializeField] PlayerManager playerManager;               // the PlayerManager script that's attached to the Camera Rig

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Not Landable")) return;

        // Because colliders aren't perfect, we want to make sure we didn't fall slightly through the floor before stopping.
        // This casts a ray straight down from above us and finds the ground, moving the Camera Rig to that exact height

        // Make the ray length as long as we are tall
        float rayLength = cameraEyeTransform.position.y - cameraRigTransform.position.y;

        // Cast the ray straight down, and make a RaycastHit for it
        Ray ray = new Ray(cameraEyeTransform.position, Vector3.down);
        RaycastHit raycastHit;

        // Add all the stuff we want to ignore to our bitmask, then reverse it
        int bitMask = 1 << 8 | 1 << 9 | 1 << 10 | 1 << 11;
        bitMask = ~bitMask;

        // If we hit something, set the Camera Rig's height to be that hit point
        if (Physics.Raycast(ray, out raycastHit, rayLength, bitMask))
        {
            cameraRigTransform.position = new Vector3(cameraRigTransform.position.x, raycastHit.point.y, cameraRigTransform.position.z);
        }

        // Switch us to landed in the player manager
        playerManager.SwitchToLanded();
    }
}
