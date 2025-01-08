using UnityEngine;
using TMPro;
using System.Collections.Generic;
public class TextFieldController : MonoBehaviour
{
    void OnEnable()
    {
        var faceDataManager = FindObjectOfType<FaceDataManager>();
        faceDataManager.OnFaceDataUpdate += UpdateTextFields;
    }

    public TextMeshProUGUI speedText, shiftText, latText, lonText, timeStampText;
    private readonly Dictionary<char, string> shiftMappings = new()
    {
        { 'P', "Park" }, { 'R', "Reverse" }, { 'D', "Drive" }, { 'N', "Neutral"}
    };

    public void UpdateTextFields(FaceData data)
    {
        speedText.text = $"{data.speed:0.0} km/h";
        shiftText.text = shiftMappings.TryGetValue(data.shift, out string shift) ? shift : "Unknown";
        latText.text = $"{data.lat}";
        lonText.text = $"{data.lon}";
        timeStampText.text = data.timestamp;
    }

    void OnDisable()
    {
        var faceDataManager = FindObjectOfType<FaceDataManager>();
        if (faceDataManager != null)
        {
            faceDataManager.OnFaceDataUpdate -= UpdateTextFields;
        }
    }
}