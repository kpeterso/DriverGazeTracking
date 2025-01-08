using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject mapCamera;
    private MapController mapController;
    private FaceDataManager faceDataManager;
    private bool moved = false;
    private float lerpDuration = 0.2f; // Duration for the camera lerp movement

    void OnEnable()
    {
        faceDataManager = FindObjectOfType<FaceDataManager>();
        faceDataManager.OnFaceDataUpdate += MoveCameraToPosition;
    }

    void Start()
    {
        if (!TryGetComponent<MapController>(out mapController))
        {
            Debug.LogError("MapController component not found on the GameObject.");
        }
    }

    public void MoveCameraToPosition(FaceData data)
    {
        // Convert latitude and longitude to a position within the map texture
        Vector2 targetPosition = LatLonToTextureCoordinates(data.lat, data.lon);

        if(!moved)
        {
            mapCamera.transform.localPosition = new Vector3(targetPosition.x, targetPosition.y, mapCamera.transform.localPosition.z);
            moved=true;
        }
        // Start the coroutine to lerp the camera to the target position
        StartCoroutine(LerpCameraPosition(targetPosition));
    }

    private IEnumerator LerpCameraPosition(Vector2 targetPosition)
    {
        float timeElapsed = 0;
        Vector3 startPosition = mapCamera.transform.localPosition;
        Vector3 endPosition = new Vector3(targetPosition.x, targetPosition.y, startPosition.z);

        while (timeElapsed < lerpDuration)
        {
            mapCamera.transform.localPosition = Vector3.Lerp(startPosition, endPosition, timeElapsed / lerpDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the position is set exactly at the end to avoid small discrepancies
        mapCamera.transform.localPosition = endPosition;
    }

    private Vector2 LatLonToTextureCoordinates(float latitude, float longitude)
    {
        // Assuming mapController provides a method to convert lat/lon to texture coordinates
        // This needs to be implemented based on how your map texture correlates with lat/lon
        return mapController.LatLonToTextureCoordinates(latitude, longitude);
    }

    void OnDisable()
    {
        if (faceDataManager != null)
        {
            faceDataManager.OnFaceDataUpdate -= MoveCameraToPosition;
        }
    }
}