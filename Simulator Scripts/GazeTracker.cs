/*using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class GazeTracker : MonoBehaviour
{
    public GameObject leftEye, rightEye;
    [SerializeField] private GameObject gazeDotPrefab; // Prefab for visualizing gaze points
    private Dictionary<GameObject, float> gazeTimeDict = new();
    private GameObject currentGazedObject;
    private GameObject gazeDot;

    // New variables for recording gaze data
    private List<GazeDataPoint> gazeData = new List<GazeDataPoint>();
    private float nextSampleTime = 0f;
    public float sampleInterval = 0.02f; // Interval between samples in seconds

    void Start()
    {
        if (gazeDotPrefab != null)
        {
            gazeDot = Instantiate(gazeDotPrefab);
            gazeDot.SetActive(false);
        }
        else
        {
            gazeDot.SetActive(true);
        }
    }

    void Update()
    {
        TrackGaze();
    }

    private void TrackGaze()
    {
        // Cast a ray from the left eye in its forward direction
        Ray eyeRay = new Ray(leftEye.transform.position, leftEye.transform.forward);

        if (Physics.Raycast(eyeRay, out RaycastHit hit))
        {
            GameObject hitObject = hit.collider.gameObject;
            gazeDot.transform.position = hit.point;
            gazeDot.SetActive(true);

            if (currentGazedObject != hitObject)
            {
                currentGazedObject = hitObject;
            }

            if (!gazeTimeDict.ContainsKey(hitObject))
            {
                gazeTimeDict[hitObject] = 0f;
            }

            gazeTimeDict[hitObject] += Time.deltaTime;

            // Record gaze data at specified intervals
            if (Time.time >= nextSampleTime)
            {
                // Record the gaze data point
                Vector3 worldPosition = hit.point;
                Vector3 viewportPosition = Camera.main.WorldToViewportPoint(worldPosition);

                if (viewportPosition.z > 0)
                {
                    GazeDataPoint dataPoint = new GazeDataPoint
                    {
                        worldPosition = worldPosition,
                        viewportPosition = new Vector2(viewportPosition.x, viewportPosition.y),
                        timestamp = Time.time
                    };
                    gazeData.Add(dataPoint);
                }

                nextSampleTime += sampleInterval;
            }
        }
        else
        {
            currentGazedObject = null;
            gazeDot.SetActive(false);
        }
    }

    public GameObject GetCurrentGazedObject()
    {
        return currentGazedObject;
    }

    public Dictionary<GameObject, float> GetGazeTimeData()
    {
        return gazeTimeDict;
    }

    // Call this method to reset gaze data, for example, at the start of a new simulation run
    public void ResetGazeData()
    {
        gazeTimeDict.Clear();
        gazeData.Clear();
        nextSampleTime = Time.time;
    }

    void OnApplicationQuit()
    {
        SaveGazeData();
    }

    private void SaveGazeData()
    {
        // Save the gaze data to a CSV file
        string filePath = Application.persistentDataPath + "/GazeData.csv";
        StringBuilder csvContent = new StringBuilder();

        // Write header
        csvContent.AppendLine("Timestamp,ViewportX,ViewportY,WorldX,WorldY,WorldZ");

        // Write data points
        foreach (var dataPoint in gazeData)
        {
            csvContent.AppendLine($"{dataPoint.timestamp}," +
                                  $"{dataPoint.viewportPosition.x}," +
                                  $"{dataPoint.viewportPosition.y}," +
                                  $"{dataPoint.worldPosition.x}," +
                                  $"{dataPoint.worldPosition.y}," +
                                  $"{dataPoint.worldPosition.z}");
        }

        File.WriteAllText(filePath, csvContent.ToString());
        Debug.Log("Gaze data saved to " + filePath);
    }
}

[System.Serializable]
public class GazeDataPoint
{
    public Vector3 worldPosition;
    public Vector2 viewportPosition;
    public float timestamp;
}*/