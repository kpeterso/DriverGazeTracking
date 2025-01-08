using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class EyeTrackingDataLoader : MonoBehaviour
{
    public string filePath; // Filepath for the eye-tracking data CSV
    private List<EyeTrackingData> eyeDataList;

    public List<EyeTrackingData> LoadEyeTrackingData()
    {
        eyeDataList = LoadEyeTrackingData(filePath);
        return eyeDataList;
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
            string line = lines[i];
            string[] values = line.Split(',');

            // Parse each value from the CSV
            EyeTrackingData data = new()
            {
                timestamp = values[0],
                leftEyePosition = new Vector3(
                    float.Parse(values[1]),
                    float.Parse(values[2]),
                    float.Parse(values[3])
                ),
                leftEyeYaw = float.Parse(values[4]),
                leftEyePitch = float.Parse(values[5]),
                rightEyePosition = new Vector3(
                    float.Parse(values[6]),
                    float.Parse(values[7]),
                    float.Parse(values[8])
                ),
                rightEyeYaw = float.Parse(values[9]),
                rightEyePitch = float.Parse(values[10])
            };

            dataList.Add(data);
        }

        return dataList;
    }
}

public class EyeTrackingData
{
    public Vector3 leftEyePosition;
    public Vector3 rightEyePosition;
    public float leftEyeYaw;
    public float leftEyePitch;
    public float rightEyeYaw;
    public float rightEyePitch;
    public string timestamp;
}
