using UnityEngine;
using UnityEngine.UI;

public class PlaybackSliderController : MonoBehaviour
{
    public Slider playbackSlider;
    private int totalCount;
    private int currentIndex;
    private FaceDataManager faceDataManager;
    private bool firstRun = true;
    void OnEnable()
    {
        faceDataManager = FindObjectOfType<FaceDataManager>();
        faceDataManager.OnFaceDataUpdate += NewFaceData;
    }

    private void NewFaceData(FaceData newData)
    {
        if(firstRun)
        {
            firstRun = false;
            totalCount = faceDataManager.FaceDataListCount;
        }
        currentIndex = faceDataManager.CurrentIndex;
        UpdateSlider();
    }

    private void UpdateSlider()
    {
        playbackSlider.value = (float)currentIndex / totalCount;
    }

    void OnDisable()
    {
        var faceDataManager = FindObjectOfType<FaceDataManager>();
        if (faceDataManager != null)
        {
            faceDataManager.OnFaceDataUpdate -= NewFaceData;
        }
    }
}