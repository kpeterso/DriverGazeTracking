using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using SFB;
using UnityEngine.ProBuilder; // Standalone File Browser

public class FileHandler : MonoBehaviour
{
    public delegate void OnDataLoaded(List<FaceData> data);
    public event OnDataLoaded onDataLoaded;

    void Start()
    {
        OpenFileBrowserAndReadData();
    }
    
    public void OpenFileBrowserAndReadData()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "csv", false);
        if (paths.Length > 0)
        {
            StartCoroutine(ReadDataFromCSV(paths[0]));
        }
    }

    private IEnumerator ReadDataFromCSV(string filePath)
    {
        List<FaceData> faceDataList = new();
        string[] lines;

        try
        {
            lines = File.ReadAllLines(filePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Error reading CSV file: " + e.Message);
            yield break;
        }

        foreach (string line in lines)
        {
            FaceData data = ParseLineToFaceData(line);
            faceDataList.Add(data);
        }
        Debug.Log("Completed File Read. " + faceDataList.Count);
        onDataLoaded?.Invoke(faceDataList);
    }

    private FaceData ParseLineToFaceData(string line)
    {
        string[] parts = line.Split(',');

        FaceData data = new()
        {
            timestamp = SafeGetString(parts, 0),
            faceYaw = 1 * SafeParseFloat(parts, 1),
            faceRoll = 1 * SafeParseFloat(parts, 2),
            facePitch = -1 * SafeParseFloat(parts, 3),
            eyeYaw = 1 * SafeParseFloat(parts, 4),
            eyePitch = -1 * SafeParseFloat(parts, 5),
            left_eye_z = SafeParseFloat(parts, 6) / 1000f,
            left_eye_x = SafeParseFloat(parts, 7) / 1000f,
            left_eye_y = SafeParseFloat(parts, 8) / 1000f,
            right_eye_z = (SafeParseFloat(parts, 9) / 1000f) - .08f,
            right_eye_x = (SafeParseFloat(parts, 10) / 1000f) - .31f,
            right_eye_y = (SafeParseFloat(parts, 11) / 1000f) - 2.28f,
            speed = SafeParseFloat(parts, 12),
            shift = SafeGetChar(parts, 13),
            lat = SafeParseFloat(parts, 14),
            lon = SafeParseFloat(parts, 15),
            steeringAngle = SafeParseFloat(parts, 16),
            ADMode = SafeGetString(parts, 17),
            ICCMode = SafeGetString(parts, 18),
            brakeStroke = SafeParseFloat(parts, 19),
            accelStroke = SafeParseFloat(parts, 20),
            ICCButton = SafeGetString(parts, 21),
            DriverDoorStatus = SafeGetString(parts, 22),
            PassengerDoorStatus = SafeGetString(parts, 23)
        };

        return data;
    }

    private float SafeParseFloat(string[] parts, int index, float defaultValue = 0f)
    {
        if (parts.Length > index && float.TryParse(parts[index], out float result))
        {
            return result;
        }
        else
        {
            return defaultValue;
        }
    }

    private string SafeGetString(string[] parts, int index, string defaultValue = "")
    {
        if (parts.Length > index)
        {
            return parts[index];
        }
        else
        {
            return defaultValue;
        }
    }

    private char SafeGetChar(string[] parts, int index, char defaultValue = '\0')
    {
        if (parts.Length > index && !string.IsNullOrEmpty(parts[index]))
        {
            return parts[index][0];
        }
        else
        {
            return defaultValue;
        }
    }


    public void ExportGazeTimeToCSV(string filePath, Dictionary<GameObject, float> gazeTime)
    {
        StringBuilder csvBuilder = new();
        csvBuilder.AppendLine("GameObject,TimeSpent");

        foreach (KeyValuePair<GameObject, float> entry in gazeTime)
        {
            csvBuilder.AppendLine($"{entry.Key.name},{entry.Value}");
        }

        try
        {
            File.WriteAllText(filePath, csvBuilder.ToString());
            Debug.Log($"Gaze time data exported to {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError("Error writing to CSV file: " + e.Message);
        }
    }
}