using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ButtonStateController : MonoBehaviour
{
    public GameObject MainSw, Set, Resume, Cancel, ICCStatus, ShiftStatus, driverDoorStatus, passengerDoorStatus;
    public TextMeshProUGUI shiftText, driverDoorText, passengerDoorText;

    private readonly Dictionary<char, string> shiftMappings = new()
    {
        { 'P', "Park" }, { 'R', "Reverse" }, { 'D', "Drive" }, { 'N', "Neutral" }
    };
    
    void OnEnable()
    {
        var faceDataManager = FindObjectOfType<FaceDataManager>();
        faceDataManager.OnFaceDataUpdate += UpdateButtons;
    }

    public void UpdateButtons(FaceData data)
    {
        SetButtonColor(ICCStatus, data.ICCMode == "ICC On");
        SetButtonColor(MainSw, data.ICCButton == "MainSw ON");
        SetButtonColor(Cancel, data.ICCButton == "CancelButton ON");
        SetButtonColor(Set, data.ICCButton == "SetButton ON");
        SetButtonColor(Resume, data.ICCButton == "ResumeButton ON");

        shiftText.text = shiftMappings.TryGetValue(data.shift, out string shift) ? shift : "Unknown";
        if (shiftText.text == "Park") {
            ShiftStatus.GetComponent<Image>().color = Color.gray;
        }
        else if (shiftText.text == "Reverse") {
            ShiftStatus.GetComponent<Image>().color = Color.blue;
        }
        else if (shiftText.text == "Drive") {
            ShiftStatus.GetComponent<Image>().color = Color.green;
        }
        else{
            ShiftStatus.GetComponent<Image>().color = Color.white;
        }

        if (data.DriverDoorStatus == "Closed"){
            driverDoorText.text = "Driver Door Closed";
            driverDoorStatus.GetComponent<Image>().color = Color.gray;
        }
        else {
            driverDoorText.text = "Driver Door Open";
            driverDoorStatus.GetComponent<Image>().color = Color.green;
        }

        if (data.PassengerDoorStatus == "Closed"){
            passengerDoorText.text = "Passenger Door Closed";
            passengerDoorStatus.GetComponent<Image>().color = Color.gray;
        }
        else {
            passengerDoorText.text = "Passenger Door Open";
            passengerDoorStatus.GetComponent<Image>().color = Color.green;
        }
    }

    private void SetButtonColor(GameObject button, bool isActive)
    {
        button.GetComponent<Image>().color = isActive ? Color.green : Color.gray;
    }

    void OnDisable()
    {
        var faceDataManager = FindObjectOfType<FaceDataManager>();
        if (faceDataManager != null)
        {
            faceDataManager.OnFaceDataUpdate -= UpdateButtons;
        }
    }
}