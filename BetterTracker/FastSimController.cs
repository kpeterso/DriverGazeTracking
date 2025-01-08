using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class FastSimController : MonoBehaviour
{
    // Public fields to be set in the Unity Inspector
    public GameObject leftEye;       // Left eye GameObject
    public Vector3 eyeOffset;        // Offset for the eye position
    public string dataFilePath;      // Filepath for the eye-tracking data CSV
    public string outputFilePath = "EyeTrackingResults.txt"; // Output file path

    private List<EyeTrackingData> eyeTrackingData; // List to hold the loaded eye-tracking data
    private int currentIndex = 0;   // Current index in the data list

    // Struct to hold eye-tracking data for each timestamp
    public struct EyeTrackingData
    {
        public string timestamp;
        public float faceYaw;
        public float faceRoll;
        public float facePitch;
        public float eyeYaw;
        public float eyePitch;
        public Vector3 leftEyePosition;
        public Vector3 rightEyePosition;
        public float speed;
        public char shift;
        public float lat;
        public float lon;
        public float steeringAngle;
        public string ADMode;
        public string ICCMode;
        public float brakeStroke;
        public float accelStroke;
        public string ICCButton;
        public string DriverDoorStatus;
        public string PassengerDoorStatus;
    }

    // Method to start the simulation
    public void RunSimulation()
    {
        // Load eye-tracking data from CSV
        eyeTrackingData = LoadEyeTrackingData(dataFilePath);

        // Process each data point in the list
        for (currentIndex = 0; currentIndex < eyeTrackingData.Count; currentIndex++)
        {
            ProcessData(eyeTrackingData[currentIndex]);
        }

        Debug.Log("Simulation complete.");
    }

    // Method to load eye-tracking data from a CSV file
    private List<EyeTrackingData> LoadEyeTrackingData(string path)
    {
        List<EyeTrackingData> dataList = new List<EyeTrackingData>();

        // Read all lines from the CSV file
        string[] lines = File.ReadAllLines(path);

        // Skip the first line if it contains headers
        for (int i = 1; i < lines.Length; i++)
        {
            EyeTrackingData data = ParseLineToEyeTrackingData(lines[i]);
            dataList.Add(data);
        }

        return dataList;
    }

    // Method to parse a line of CSV into EyeTrackingData
    private EyeTrackingData ParseLineToEyeTrackingData(string line)
    {
        string[] parts = line.Split(',');

        EyeTrackingData data = new EyeTrackingData
        {
            timestamp = parts[0],
            faceYaw = 1 * float.Parse(parts[1]),
            faceRoll = 1 * float.Parse(parts[2]),
            facePitch = -1 * float.Parse(parts[3]),
            eyeYaw = 1 * float.Parse(parts[4]),
            eyePitch = -1 * float.Parse(parts[5]),
            leftEyePosition = new Vector3(
                float.Parse(parts[7]) / 1000f,
                float.Parse(parts[8]) / 1000f,
                float.Parse(parts[6]) / 1000f
            ),
            rightEyePosition = new Vector3(
                (float.Parse(parts[10]) / 1000f) - 0.31f,
                (float.Parse(parts[11]) / 1000f) - 2.28f,
                (float.Parse(parts[9]) / 1000f) - 0.08f
            ),
            speed = float.Parse(parts[12]),
            shift = parts[13][0],
            lat = float.Parse(parts[14]),
            lon = float.Parse(parts[15]),
            steeringAngle = float.Parse(parts[16]),
            ADMode = parts[17],
            ICCMode = parts[18],
            brakeStroke = float.Parse(parts[19]),
            accelStroke = float.Parse(parts[20]),
            ICCButton = parts[21],
            DriverDoorStatus = parts[22],
            PassengerDoorStatus = parts[23]
        };

        return data;
    }

    // Method to process each data point
    private void ProcessData(EyeTrackingData data)
    {
        // Update eye position and orientation
        UpdateEyePosition(data);

        // Perform raycasting and record results
        RecordGazeData();
    }

    // Method to update the left eye's position and rotation
    private void UpdateEyePosition(EyeTrackingData data)
    {
        leftEye.transform.SetPositionAndRotation(
            data.leftEyePosition + eyeOffset,
            Quaternion.Euler(data.eyePitch, data.eyeYaw, 0)
        );
    }

    // Method to perform raycasting and log results
    private void RecordGazeData()
    {
        // Raycast from the left eye's position in its forward direction
        if (Physics.Raycast(leftEye.transform.position, leftEye.transform.forward, out RaycastHit hitLeft))
        {
            LogHitData(hitLeft);
        }
    }

    // Method to log the raycast hit data to a file in CSV format
    private void LogHitData(RaycastHit hit)
    {   
        Vector3 gazeIntersection = hit.point;
        Vector3 viewportPosition = Camera.main.WorldToViewportPoint(gazeIntersection);
        Vector2 viewportPosition2D = (Vector2)viewportPosition;
        string logEntry = $"{eyeTrackingData[currentIndex].timestamp},{hit.collider.name},{gazeIntersection.x},{gazeIntersection.y},{gazeIntersection.z},{viewportPosition2D.x},{viewportPosition2D.y}\n";
        File.AppendAllText(outputFilePath, logEntry);
    }

}