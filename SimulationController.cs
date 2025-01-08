using UnityEngine;

public class SimulationController : MonoBehaviour
{
    private readonly float simulationStartTime;
    private bool isPlaying = false;
    private float simulationSpeed = 1.0f;

    void Update()
    {
        float simulationTime = Time.time - simulationStartTime;
        CheckInput();
    }

    private void CheckInput()
    {
        // Toggle play/pause
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePlayPause();
        }

        // Increase simulation speed
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ChangeSimulationSpeed(0.1f); // Increase speed
        }

        // Decrease simulation speed
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ChangeSimulationSpeed(-0.1f); // Decrease speed
        }

        // Exit Simulation
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitSimulation();
        }
    }

    private void TogglePlayPause()
    {
        isPlaying = !isPlaying;
        Time.timeScale = isPlaying ? simulationSpeed : 0;
    }

    private void ChangeSimulationSpeed(float change)
    {
        simulationSpeed = Mathf.Clamp(simulationSpeed + change, 0.1f, 10f);
        if (isPlaying)
        {
            Time.timeScale = simulationSpeed;
        }
    }

    private void ExitSimulation()
    {
        // Code to handle application exit
        Application.Quit();

        // If running in the Unity editor, stop play mode
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // Public methods to control the simulation from other scripts
    public void Play()
    {
        isPlaying = true;
        Time.timeScale = simulationSpeed;
    }

    public void Pause()
    {
        isPlaying = false;
        Time.timeScale = 0;
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }
}