using UnityEngine;

public class EyeController : MonoBehaviour
{
    public Vector3 eyeOffset;
    public GameObject leftEye;
    //public Camera rightEyeCamera;

    public void UpdateEyeCameras(EyeTrackingData data)
    {
        // Update left eye camera
        leftEye.transform.SetPositionAndRotation((data.leftEyePosition+eyeOffset), Quaternion.Euler(data.leftEyePitch, data.leftEyeYaw, 0));

        // Update right eye camera
        //rightEyeCamera.transform.position = data.rightEyePosition;
        //rightEyeCamera.transform.rotation = Quaternion.Euler(data.rightEyePitch, data.rightEyeYaw, 0);
    }
}