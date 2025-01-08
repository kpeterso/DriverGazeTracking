using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class ImageToQuad : MonoBehaviour
{
    private const string BaseUrl = "https://tile.openstreetmap.org/";
    public delegate Texture2D TilesDownloaded();
    public event TilesDownloaded OnAllTilesDownloaded;
    public GameObject mapCamera;
    private int tilesNeededCount = 0;
    private int downloadedTileCount = 0;
    private HashSet<(int, int, int)> uniqueTileCoords = new();
    private int minTileX, maxTileX, minTileY, maxTileY, textureWidth, textureHeight;
    private double minLat, maxLat, minLon, maxLon;
    private readonly int zoom = 12;
    private Dictionary<(int x, int y, int zoom), Texture2D> tileCache = new();

    public void InitiateTileDisplay(List<FaceData> faceDataList)
    {
        //This function takes a list of GPS coordinates, converts them to tile numbers, retrieves the tiles,
        //combines them into a single texture, then applies that texture to the quad
        List<(float, float)> latLonList = GetLatLongList(faceDataList);

        LoadTiles(latLonList, zoom);
    }

    public void LoadTiles(List<(float, float)> latLonList, int zoom)
    {
        // Convert each lat-long pair to tile coordinates and add to the set
        foreach ((float lat, float lon) in latLonList)
        {
            int x = TileUtilities.LonToTileX(lon, zoom);
            int y = TileUtilities.LatToTileY(lat, zoom);
            AddTileAndNeighbors(x, y, zoom, uniqueTileCoords);
        }

        // Update the count of tiles needed
        tilesNeededCount = uniqueTileCoords.Count;
        downloadedTileCount = 0; // Reset the counter
        OnAllTilesDownloaded += StitchTiles;

        // Start downloading each unique tile
        foreach (var tileCoord in uniqueTileCoords)
        {
            StartCoroutine(LoadTileFromDiskOrDownload(tileCoord.Item1, tileCoord.Item2, tileCoord.Item3));
        }
    }

    private void AddTileAndNeighbors(int x, int y, int zoom, HashSet<(int, int, int)> tileCoordsSet)
    {
        // Add the original tile
        tileCoordsSet.Add((x, y, zoom));

        // Add neighboring tiles
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue; // Skip the original tile
                tileCoordsSet.Add((x + dx, y + dy, zoom));
            }
        }
    }


    private IEnumerator LoadTileFromDiskOrDownload(int x, int y, int zoom)
    {
        var tileKey = (x, y, zoom);
        if (tileCache.ContainsKey(tileKey))
        {
            Debug.Log($"Tile in cache! {x}, {y}, {zoom}");
            TileDownloaded(); // Call the method when a tile is downloaded
        }
        else
        {
            string directoryPath = Path.Combine(Application.persistentDataPath, $"Tiles/{zoom}");
            string filePath = Path.Combine(directoryPath, $"{x}_{y}_{zoom}.png");
            Debug.Log(filePath);
            if (File.Exists(filePath))
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2);
                // Use LoadImage to update the texture with data
                if (texture.LoadImage(fileData))
                {
                    tileCache[(x, y, zoom)] = texture;
                    TileDownloaded();
                    // Optionally update your UI with the loaded texture
                }
                else
                {
                    Debug.LogError("Failed to load texture from disk");
                }
            }
            else
            {
                Debug.Log($"Downloading tile {x},{y}");
                // Start the download coroutine and wait for it to finish
                yield return StartCoroutine(DownloadTile(x, y, zoom));
            }
        }
    }

    private IEnumerator DownloadTile(int x, int y, int zoom)
    {
        var tileKey = (x, y, zoom);
        if (!tileCache.ContainsKey(tileKey))
        {
            string url = $"{BaseUrl}{zoom}/{x}/{y}.png";
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                    tileCache[tileKey] = texture;
                    SaveTileToDisk(texture, x, y, zoom);
                    Debug.Log($"Tile downloaded! {x}, {y}, {zoom}");
                    TileDownloaded(); // Call the method when a tile is downloaded
                }
                else
                {
                    Debug.LogError("Error downloading tile: " + uwr.error);
                }
            }
        }
    }

    // Tile Loading Methods

    private void TileDownloaded()
    {
        downloadedTileCount++;
        if (downloadedTileCount == tilesNeededCount)
        {
            Texture2D texture = OnAllTilesDownloaded?.Invoke();
            (minLat, minLon) = TileUtilities.TileXYToLatLon(minTileX, minTileY, zoom);
            (maxLat, maxLon) = TileUtilities.TileXYToLatLon(maxTileX + 1, maxTileY + 1, zoom);
            SetImage(texture);
        }
    }

    private void SaveTileToDisk(Texture2D texture, int x, int y, int zoom)
    {
        byte[] bytes = texture.EncodeToPNG();
        string directoryPath = Path.Combine(Application.persistentDataPath, $"Tiles/{zoom}");
        Directory.CreateDirectory(directoryPath); // Ensure directory exists
        string filePath = Path.Combine(directoryPath, $"{x}_{y}_{zoom}.png");
        File.WriteAllBytes(filePath, bytes);
    }

    private void SaveStitchedTileToDisk(Texture2D texture, int x, int y, int size, int zoom)
    {
        byte[] bytes = texture.EncodeToPNG();
        string directoryPath = Path.Combine(Application.persistentDataPath, $"StitchedTiles/{zoom}");
        Directory.CreateDirectory(directoryPath); // Ensure directory exists
        string filePath = Path.Combine(directoryPath, $"{x}_{y}_{size}_{zoom}.png");
        File.WriteAllBytes(filePath, bytes);
    }

    Texture2D StitchTiles()
    {
        if (tileCache.Count == 0)
            return null;

        minTileX = tileCache.Keys.Min(k => k.x);
        maxTileX = tileCache.Keys.Max(k => k.x);
        minTileY = tileCache.Keys.Min(k => k.y);
        maxTileY = tileCache.Keys.Max(k => k.y);

        textureWidth = (maxTileX - minTileX + 1) * 256;
        textureHeight = (maxTileY - minTileY + 1) * 256;
        Texture2D combinedTexture = new(textureWidth, textureHeight);

        foreach (var tileEntry in tileCache)
        {
            int tileX = (tileEntry.Key.x - minTileX) * 256;
            int tileY = (maxTileY - tileEntry.Key.y) * 256; // Invert Y axis
            combinedTexture.SetPixels(tileX, tileY, 256, 256, tileEntry.Value.GetPixels());
        }

        combinedTexture.Apply();

        OnAllTilesDownloaded -= StitchTiles; // Unsubscribe from the event

        return combinedTexture;
    }

    private void SetImage(Texture2D image)
    {
        if(image != null)
        {
            ScaleQuadToFitTexture(image);
            Renderer quadRenderer = GetComponent<Renderer>();
            quadRenderer.material.mainTexture = image;
        }
        else
        {
            Debug.LogError("Image is null!");
        }
    }

    void ScaleQuadToFitTexture(Texture2D texture)
    {
        float textureAspectRatio = (float)texture.width / texture.height;
        float quadHeight = 50f; // Set this to the desired size
        float quadWidth = quadHeight * textureAspectRatio;

        transform.localScale = new Vector3(quadWidth, quadHeight, 1f);
    }

    Vector2 LatLonToTextureCoordinates(float latitude, float longitude)
    {
        // Normalize latitude and longitude within the bounds
        double normalizedX = (longitude - minLon) / (maxLon - minLon);
        double normalizedY = (latitude - minLat) / (maxLat - minLat);
        normalizedY = 1-normalizedY;
        // Convert normalized coordinates to texture coordinates
        //float x = (float)(normalizedX * transform.localScale.x);
        //float y = (float)((1 - normalizedY) * transform.localScale.y); // Invert Y axis

        return new Vector2((float)normalizedX-.5f, (float)normalizedY-.5f);
    }

    public void MoveCameraToPosition(float lat, float lon)
    {
        Vector2 targetPosition = LatLonToTextureCoordinates(lat, lon);
        if (mapCamera.transform.localPosition.x > .5)
        {
            mapCamera.transform.localPosition = new Vector3(targetPosition.x, targetPosition.y, mapCamera.transform.localPosition.z);
        }
        // Start the coroutine to move the camera
        StartCoroutine(LerpCameraPosition(targetPosition));
    }

    private IEnumerator LerpCameraPosition(Vector2 targetPosition)
    {
        float lerpDuration = 0.2f;
        float timeElapsed = 0;
        Vector3 startPosition = mapCamera.transform.localPosition;
        Vector3 endPosition = new(targetPosition.x, targetPosition.y, startPosition.z);

        while (timeElapsed < lerpDuration)
        {
            mapCamera.transform.localPosition = Vector3.Lerp(startPosition, endPosition, timeElapsed / lerpDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the position is set exactly at the end to avoid small discrepancies
        mapCamera.transform.localPosition = endPosition;
    }

    public List<(float, float)> GetLatLongList(List<FaceData> faceDataList)
    {
        var latLongList = new List<(float, float)>();
        foreach (var faceData in faceDataList)
        {
            latLongList.Add((faceData.lat, faceData.lon));
        }
        return latLongList;
    }
}