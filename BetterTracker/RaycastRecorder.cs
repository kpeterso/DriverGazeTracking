using UnityEngine;
using System.IO;

public class RaycastRecorder : MonoBehaviour
{
    public EyeController eyeController;
    public string outputFilePath = "EyeTrackingResults.txt";

    public void RecordGazeData()
    {

        if (Physics.Raycast(eyeController.leftEye.transform.position, eyeController.leftEye.transform.forward, out RaycastHit hitLeft))
        {
            LogHitData(hitLeft, "LeftEye");
        }

        /*if (Physics.Raycast(eyeController.rightEyeCamera.transform.position, eyeController.rightEyeCamera.transform.forward, out RaycastHit hitRight))
        {
            LogHitData(hitRight, "RightEye");
        }*/
    }

    private void LogHitData(RaycastHit hit, string eye)
    {
        string logEntry = $"{eye} hit {hit.collider.name} at {hit.point}\n";
        File.AppendAllText(outputFilePath, logEntry);
    }
}