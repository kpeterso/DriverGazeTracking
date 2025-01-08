/*using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class HeadEyeMovementController : MonoBehaviour
{
    public Transform headTransform;
    public Transform leftEyeTransform;
    public Transform rightEyeTransform;
    public float xOffset, yOffset, zOffset;

    private FaceData prevData = new FaceData();
    private FaceData nextData = new FaceData();
    private Quaternion eyeStartRotation;
    private Quaternion eyeEndRotation;
    private Vector3 oldLeftEyePosition;
    private Vector3 newLeftEyeEndPostition;
    private float lerpFactor;
    private float updateInterval = 0.2f; // Interval at which data updates

    private List<string> gazedObjectsList = new List<string>();
    private GazeTracker gazeTracker;
    private float lastDataUpdateTime;

    void OnEnable()
    {
        var faceDataManager = FindObjectOfType<FaceDataManager>();
        faceDataManager.OnFaceDataUpdate += SetFaceData;
    }

    void Start()
    {
        gazeTracker = FindObjectOfType<GazeTracker>();
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
    }

    public void SetFaceData(FaceData newData)
    {
        lastDataUpdateTime = Time.time; // Record the time of this data update

        prevData = nextData;
        nextData = newData;
        oldLeftEyePosition = leftEyeTransform.position;
        newLeftEyeEndPostition = new Vector3(nextData.left_eye_x + xOffset, nextData.left_eye_y + yOffset, nextData.left_eye_z + zOffset);

        eyeEndRotation = Quaternion.Euler(nextData.eyePitch, nextData.eyeYaw, 0);
        eyeStartRotation = leftEyeTransform.rotation;

        // Store the current gazed object
        GameObject currentGazedObject = gazeTracker.GetCurrentGazedObject();
        if (currentGazedObject != null)
        {
            gazedObjectsList.Add(nextData.timestamp + "," + currentGazedObject.name);
        }
    }

    public void ApplyTransformations()
    {
        // Spherical interpolate head rotation
        Quaternion headStartRotation = headTransform.rotation;
        Quaternion headEndRotation = Quaternion.Euler(nextData.facePitch, nextData.faceYaw, nextData.faceRoll);
        headTransform.rotation = Quaternion.Slerp(headStartRotation, headEndRotation, lerpFactor);

        // Linear interpolation of eye positions
        leftEyeTransform.position = Vector3.Lerp(oldLeftEyePosition, newLeftEyeEndPostition, lerpFactor);
        Vector3 rightEyeEndPosition = new Vector3(nextData.right_eye_x, nextData.right_eye_y, nextData.right_eye_z);
        rightEyeTransform.position = Vector3.Lerp(rightEyeTransform.position, rightEyeEndPosition, lerpFactor);

        // Spherical interpolate eye rotations
        leftEyeTransform.rotation = Quaternion.Slerp(eyeStartRotation, eyeEndRotation, lerpFactor);
        rightEyeTransform.rotation = Quaternion.Slerp(eyeStartRotation, eyeEndRotation, lerpFactor);
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
        WriteGazedObjectsToCSV();
    }

    private void WriteGazedObjectsToCSV()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "GazedObjects.csv");

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // Write header
            writer.WriteLine("Timestamp,GazedObject");

            // Write data
            foreach (var gazedObject in gazedObjectsList)
            {
                writer.WriteLine(gazedObject);
            }
        }

        Debug.Log($"Gazed objects data saved to {filePath}");
    }
}*/