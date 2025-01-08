using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class GazeDisplayController : MonoBehaviour
{
    public TextMeshProUGUI firstGazeText, secondGazeText, thirdGazeText, shifterGazeText;

    public GameObject shifterObject;

    [SerializeField] private GazeTrackerWithHeadEyeMovement gazeTracker; // Reference to the GazeTracker script
    [SerializeField] private float updateInterval = 1f; // Time interval for updates

    void Start()
    {
        StartCoroutine(UpdateGazeTextRoutine());
    }

    private IEnumerator UpdateGazeTextRoutine()
    {
        while (true)
        {
            UpdateGazeUI(gazeTracker.GetGazeTimeData());
            yield return new WaitForSeconds(updateInterval);
        }
    }

    public void UpdateGazeUI(Dictionary<GameObject, float> gazeData)
    {
        var sortedGaze = gazeData.OrderByDescending(kvp => kvp.Value).Take(3).ToList();
        UpdateGazeText(firstGazeText, sortedGaze, 0);
        UpdateGazeText(secondGazeText, sortedGaze, 1);
        UpdateGazeText(thirdGazeText, sortedGaze, 2);
        //shifterGazeText.text=gazeData[shifterObject].ToString();
    }

    private void UpdateGazeText(TextMeshProUGUI textElement, List<KeyValuePair<GameObject, float>> gazeList, int index)
    {
        textElement.text = gazeList.Count > index ? FormatGazeTimeEntry(gazeList[index]) : "N/A";
    }

    private string FormatGazeTimeEntry(KeyValuePair<GameObject, float> entry)
    {
        float minutes = entry.Value / 60;
        float seconds = entry.Value % 60;
        return $"{entry.Key.name}: {minutes:F0}m {seconds:F2}s";
    }
}