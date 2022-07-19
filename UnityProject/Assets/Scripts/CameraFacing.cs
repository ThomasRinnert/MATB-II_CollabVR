using UnityEngine;

public class CameraFacing : MonoBehaviour
{
    void Update()
    {
        if(Camera.current == null) {return;}

        // transform.LookAt(transform.position + Camera.current.transform.rotation * Vector3.forward, Camera.current.transform.rotation * Vector3.up);
        
        transform.LookAt(Camera.current.transform);
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, transform.localEulerAngles.z);
    }
}
