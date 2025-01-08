using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class FaceDataManager : MonoBehaviour
{
    // Delegate and event for face data updates
    public delegate void FaceDataEvent(FaceData faceData);
    public event FaceDataEvent OnFaceDataUpdate;

    // Data management
    private List<FaceData> faceDataList;
    public int FaceDataListCount { get; private set; } = 0;
    public int CurrentIndex { get; private set; } = 0;

    // Simulation control variables
    private bool isPlaying = false;
    private float simulationSpeed = 1.0f;

    // References to other components
    private MapController mapController;
    private FileHandler fileHandler;

    void OnEnable()
    {
        // Initialize references
        fileHandler = FindObjectOfType<FileHandler>();
        fileHandler.onDataLoaded += GetFaceData;

        mapController = FindObjectOfType<MapController>();
        if (mapController != null)
        {
            mapController.OnTextureUpdate += HandleStartSim;
        }
        else
        {
            // Start the simulation immediately if no MapController
            HandleStartSim();
        }
    }

    private void GetFaceData(List<FaceData> dataList)
    {
        faceDataList = dataList;
        FaceDataListCount = faceDataList.Count;

        // Sort faceDataList by timestamp to ensure correct playback order
        faceDataList.Sort((a, b) => a.timestamp.CompareTo(b.timestamp));
    }

    private void HandleStartSim()
    {
        // Start the simulation
        isPlaying = true;
        StartCoroutine(SimulationCoroutine());
    }

    private IEnumerator SimulationCoroutine()
    {
        // Ensure data is loaded
        while (faceDataList == null || faceDataList.Count == 0)
        {
            yield return null;
        }

        float startTime = Time.time;
        float nextUpdateTime = startTime;

        while (isPlaying && CurrentIndex < faceDataList.Count)
        {
            // Calculate the time for the next update
            nextUpdateTime += 0.2f / simulationSpeed;

            // Wait until it's time for the next update
            float timeToWait = nextUpdateTime - Time.time;
            if (timeToWait > 0)
            {
                yield return new WaitForSeconds(timeToWait);
            }

            // Invoke the event to update face data
            OnFaceDataUpdate?.Invoke(faceDataList[CurrentIndex]);
            CurrentIndex++;

            // Check if we've reached the end of the data
            if (CurrentIndex >= faceDataList.Count)
            {
                isPlaying = false;
            }
        }
    }

    void Update()
    {
        // Handle user input for simulation control
        CheckInput();
    }

    private void CheckInput()
    {
        // Toggle play/pause with Space key
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePlayPause();
        }

        // Increase simulation speed with Up Arrow key
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ChangeSimulationSpeed(0.1f);
        }

        // Decrease simulation speed with Down Arrow key
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ChangeSimulationSpeed(-0.1f);
        }

        // Exit simulation with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitSimulation();
        }
    }

    private void TogglePlayPause()
    {
        isPlaying = !isPlaying;
        if (isPlaying)
        {
            // Restart the coroutine
            StartCoroutine(SimulationCoroutine());
        }
        else
        {
            // Stop all coroutines
            StopAllCoroutines();
        }
    }

    private void ChangeSimulationSpeed(float change)
    {
        simulationSpeed = Mathf.Clamp(simulationSpeed + change, 0.1f, 10f);
    }

    private void ExitSimulation()
    {
        // Handle application exit
        Application.Quit();

        // If running in the Unity editor, stop play mode
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void OnDestroy()
    {
        // Clean up event subscriptions
        fileHandler.onDataLoaded -= GetFaceData;
        if (mapController != null)
        {
            mapController.OnTextureUpdate -= HandleStartSim;
        }
    }
}