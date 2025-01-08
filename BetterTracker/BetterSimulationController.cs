using UnityEngine;
using System.Collections.Generic;

public class BetterSimulationController : MonoBehaviour
{
    public EyeController eyeController;
    public RaycastRecorder raycastRecorder;
    public EyeTrackingDataLoader dataLoader;
    private List<EyeTrackingData> eyeTrackingData;
    private int currentIndex = 0;

    public void RunSimulation()
    {
        eyeTrackingData = dataLoader.LoadEyeTrackingData();

        for (currentIndex = 0; currentIndex < eyeTrackingData.Count; currentIndex++)
        {
            ProcessData(eyeTrackingData[currentIndex]);
        }

        Debug.Log("Simulation complete.");
    }

    private void ProcessData(EyeTrackingData data)
    {
        // Update eye positions and orientations
        eyeController.UpdateEyeCameras(data);

        // Perform raycasting and record results
        raycastRecorder.RecordGazeData();
    }
}