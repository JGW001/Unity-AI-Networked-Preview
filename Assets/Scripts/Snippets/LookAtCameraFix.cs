using UnityEngine;
public class LookAtCameraFix : MonoBehaviour
{
    // No need to run this on server.
    // This basically makes the object you attach this script to, look at you, it will be used for nametags & speech bubbles etc.
    #if !UNITY_SERVER
    private void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, transform.rotation.z * -1.0f);
    }
    #endif
}
