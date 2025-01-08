using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LineChart : MonoBehaviour
{
    private Image chartImage; // Assign this in the Inspector
    private Texture2D chartTexture;
    private Queue<float> dataPoints = new Queue<float>();

    private void OnEnable()
    {
        var faceDataManager = FindObjectOfType<FaceDataManager>();
        faceDataManager.OnFaceDataUpdate += HandleNewDataPoint;
    }

    private void OnDisable()
    {
        var faceDataManager = FindObjectOfType<FaceDataManager>();
        if (faceDataManager != null)
        {
            faceDataManager.OnFaceDataUpdate -= HandleNewDataPoint;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        chartImage = GetComponent<Image>();
        chartTexture = new Texture2D((int)chartImage.rectTransform.rect.width, (int)chartImage.rectTransform.rect.height);
        chartImage.sprite = Sprite.Create(chartTexture, new Rect(0.0f, 0.0f, chartTexture.width, chartTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
    }

    // Update is called once per frame
    void Update()
    {
        DrawLineChart();
    }

    void DrawLineChart()
    {
        // Clear the texture
        Color clear = new Color(0, 0, 0, 50);
        chartTexture.SetPixels(0, 0, chartTexture.width, chartTexture.height, Enumerable.Repeat(clear, chartTexture.width * chartTexture.height).ToArray());

        // Draw the line chart
        int i = 0;
        float xSpacing = chartTexture.width / (float)dataPoints.Count;
        Vector2 prevPos = Vector2.zero;
        foreach (float point in dataPoints) {
            Vector2 pos = new Vector2(i * xSpacing, (100-point) * chartTexture.height);
            if (i > 0) {
                DrawLine(prevPos, pos, Color.white);
            }
            prevPos = pos;
            i++;
        }

        // Apply changes to the texture
        chartTexture.Apply();
    }

    private void HandleNewDataPoint(FaceData newDataPoint) {
        // Add new data point to the queue
        dataPoints.Enqueue(newDataPoint.accelStroke);

        // Ensure the queue doesn't grow beyond 30 seconds worth of data
        // Assuming you're updating once per second
        if (dataPoints.Count > 30) {
            dataPoints.Dequeue();
        }

        DrawLineChart();
    }

    void DrawLine(Vector2 p1, Vector2 p2, Color color)
    {
        int x0 = (int)p1.x;
        int y0 = (int)p1.y;
        int x1 = (int)p2.x;
        int y1 = (int)p2.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = (x0 < x1) ? 1 : -1;
        int sy = (y0 < y1) ? 1 : -1;
        int err = dx - dy;

        while (true) {
            chartTexture.SetPixel(x0, y0, color);

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

}
