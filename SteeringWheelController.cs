using UnityEngine;

public class SteeringWheelController : MonoBehaviour
{
    public Transform steeringWheelTransform; // Reference to the steering wheel transform
    private float currentSteeringAngle;
    private float targetSteeringAngle;
    [SerializeField] private float rotationSpeed = 5f; // Speed of rotation, adjust as necessary

    void OnEnable()
    {
        var faceDataManager = FindObjectOfType<FaceDataManager>();
        faceDataManager.OnFaceDataUpdate += SetSteeringAngle;

    }
    void Update()
    {
        // Smoothly rotate the steering wheel towards the target angle
        currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, targetSteeringAngle, Time.deltaTime * rotationSpeed);
        steeringWheelTransform.localRotation = Quaternion.Euler(330, 0, -currentSteeringAngle);
    }

    // Call this method to update the target steering angle
    public void SetSteeringAngle(FaceData newData)
    {
        targetSteeringAngle = newData.steeringAngle;
    }

    void OnDisable()
    {
        var faceDataManager = FindObjectOfType<FaceDataManager>();
        if (faceDataManager != null)
        {
            faceDataManager.OnFaceDataUpdate -= SetSteeringAngle;
        }
    }
}