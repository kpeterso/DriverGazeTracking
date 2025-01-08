using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class GazeTrackerWithHeadEyeMovement : MonoBehaviour
{
    // References
    public Transform headTransform;
    public Transform leftEyeTransform;
    public Transform rightEyeTransform;
    public GameObject gazeDotPrefab; // Prefab for visualizing gaze points
    private Dictionary<GameObject, float> gazeTimeDict = new();


    // Offsets for eye positions
    public float xOffset, yOffset, zOffset;

    // Face data for interpolation
    private FaceData prevData = new();
    private FaceData nextData = new();
    private Quaternion eyeStartRotation;
    private Quaternion eyeEndRotation;
    private Vector3 oldLeftEyePosition;
    private Vector3 newLeftEyeEndPosition;
    private float lerpFactor;
    private readonly float updateInterval = 0.2f; // Interval at which data updates
    private float lastDataUpdateTime;

    // Gaze tracking variables
    private GameObject gazeDot;
    private GameObject currentGazedObject;
    private float nextSampleTime = 0f;
    public float sampleInterval = 0.02f; // Interval between gaze data samples

    // Data recording
    private List<GazeDataPoint> gazeData = new();

    void OnEnable()
    {
        var faceDataManager = FindObjectOfType<FaceDataManager>();
        faceDataManager.OnFaceDataUpdate += SetFaceData;
    }

    void Start()
    {
        // Initialize gazeDot
        if (gazeDotPrefab != null)
        {
            gazeDot = Instantiate(gazeDotPrefab);
            gazeDot.SetActive(false);
        }
        else
        {
            gazeDot = new GameObject("GazeDot");
            gazeDot.SetActive(false);
        }

        nextSampleTime = Time.time;
    }

    void Update()
    {
        if (!nextData.IsEmpty())
        {
            // Calculate lerpFactor based on real elapsed time
            float elapsedTime = Time.time - lastDataUpdateTime;
            lerpFactor = elapsedTime / updateInterval;
            lerpFactor = Mathf.Clamp01(lerpFactor); // Ensure lerpFactor is between 0 and 1
            ApplyTransformations();
        }

        // Track gaze and record data
        TrackGaze();
    }

    public void SetFaceData(FaceData newData)
    {
        lastDataUpdateTime = Time.time; // Record the time of this data update

        prevData = nextData;
        nextData = newData;
        oldLeftEyePosition = leftEyeTransform.position;
        newLeftEyeEndPosition = new Vector3(nextData.left_eye_x + xOffset, nextData.left_eye_y + yOffset, nextData.left_eye_z + zOffset);

        eyeEndRotation = Quaternion.Euler(nextData.eyePitch, nextData.eyeYaw, 0);
        eyeStartRotation = leftEyeTransform.rotation;
    }

    public void ApplyTransformations()
    {
        // Spherical interpolate head rotation
        Quaternion headStartRotation = headTransform.rotation;
        Quaternion headEndRotation = Quaternion.Euler(nextData.facePitch, nextData.faceYaw, nextData.faceRoll);
        headTransform.rotation = Quaternion.Slerp(headStartRotation, headEndRotation, lerpFactor);

        // Linear interpolation of eye positions
        leftEyeTransform.position = Vector3.Lerp(oldLeftEyePosition, newLeftEyeEndPosition, lerpFactor);
        Vector3 rightEyeEndPosition = new Vector3(nextData.right_eye_x, nextData.right_eye_y, nextData.right_eye_z);
        rightEyeTransform.position = Vector3.Lerp(rightEyeTransform.position, rightEyeEndPosition, lerpFactor);

        // Spherical interpolate eye rotations
        leftEyeTransform.rotation = Quaternion.Slerp(eyeStartRotation, eyeEndRotation, lerpFactor);
        rightEyeTransform.rotation = Quaternion.Slerp(eyeStartRotation, eyeEndRotation, lerpFactor);
    }

    private void TrackGaze()
    {
        // Cast a ray from the left eye in its forward direction
        Ray eyeRay = new(leftEyeTransform.position, leftEyeTransform.forward);

        if (Physics.Raycast(eyeRay, out RaycastHit hit))
        {
            GameObject hitObject = hit.collider.gameObject;
            gazeDot.transform.position = hit.point;
            gazeDot.SetActive(true);
            currentGazedObject = hitObject;
            // **Update gazeTimeDict**
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
                    GazeDataPoint dataPoint = new()
                    {
                        timestamp = Time.time,
                        worldPosition = worldPosition,
                        viewportPosition = (Vector2)viewportPosition,
                        gazedObjectName = hitObject.name
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

            // Optional: Record data when not gazing at any object
            if (Time.time >= nextSampleTime)
            {
                GazeDataPoint dataPoint = new()
                {
                    timestamp = Time.time,
                    worldPosition = leftEyeTransform.position + leftEyeTransform.forward * 100f, // Arbitrary far point
                    viewportPosition = Vector2.zero,
                    gazedObjectName = "None"
                };
                gazeData.Add(dataPoint);

                nextSampleTime += sampleInterval;
            }
        }
    }

    public Dictionary<GameObject, float> GetGazeTimeData()
    {
        return gazeTimeDict;
    }

    public void ResetGazeData()
    {
        gazeData.Clear();
        gazeTimeDict.Clear();
        nextSampleTime = Time.time;
    }

    void OnDisable()
    {
        var faceDataManager = FindObjectOfType<FaceDataManager>();
        if (faceDataManager != null)
        {
            faceDataManager.OnFaceDataUpdate -= SetFaceData;
        }
    }

    void OnApplicationQuit()
    {
        SaveGazeData();
    }

    private void SaveGazeData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "GazeData.csv");
        StringBuilder csvContent = new StringBuilder();

        // Write header
        csvContent.AppendLine("Timestamp,ViewportX,ViewportY,WorldX,WorldY,WorldZ,GazedObject");

        // Write data points
        foreach (var dataPoint in gazeData)
        {
            csvContent.AppendLine($"{dataPoint.timestamp}," +
                                $"{dataPoint.viewportPosition.x}," +
                                $"{dataPoint.viewportPosition.y}," +
                                $"{dataPoint.worldPosition.x}," +
                                $"{dataPoint.worldPosition.y}," +
                                $"{dataPoint.worldPosition.z}," +
                                $"{dataPoint.gazedObjectName}");
        }

        File.WriteAllText(filePath, csvContent.ToString());
        Debug.Log($"Gaze data saved to {filePath}");
    }
}

[System.Serializable]
public class GazeDataPoint
{
    public float timestamp;
    public Vector2 viewportPosition;
    public Vector3 worldPosition;
    public string gazedObjectName;
}