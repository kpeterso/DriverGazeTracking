using UnityEngine;
using UnityEngine.UI;

public class ADModeDisplayController : MonoBehaviour
{
    public GameObject ADMode;
    void OnEnable()
    {
        var faceDataManager = FindObjectOfType<FaceDataManager>();
        faceDataManager.OnFaceDataUpdate += SetADModeColor;
    }

    public void SetADModeColor(FaceData newData)
    {
        Color color = newData.ADMode switch
        {
            "No AD" => Color.gray,
            "White" => Color.white,
            "Blue" => Color.blue,
            "Green" => Color.green,
            _ => Color.gray
        };
        ADMode.GetComponent<Image>().color = color;
    }

    void OnDisable()
    {
        var faceDataManager = FindObjectOfType<FaceDataManager>();
        if (faceDataManager != null)
        {
            faceDataManager.OnFaceDataUpdate -= SetADModeColor;
        }
    }
}