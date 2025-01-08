using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class TileManager : MonoBehaviour
{
    private const string BaseUrl = "https://tile.openstreetmap.org/";
    private HashSet<(int, int, int)> uniqueTileCoords = new();
    private Dictionary<(int x, int y, int zoom), Texture2D> tileCache = new();
    private int tilesNeededCount;
    private int downloadedTileCount = 0;
    private FileHandler fileHandler;
    public delegate void TileDownloadedDelegate();
    public event TileDownloadedDelegate OnTileDownloaded;
    public delegate void AllTilesDownloadedDelegate(Dictionary<(int x, int y, int zoom), Texture2D> tileCache);
    public event AllTilesDownloadedDelegate OnAllTilesDownloaded;
    private readonly int zoom = 12;

    void Start()
    {
        fileHandler = FindObjectOfType<FileHandler>();
        fileHandler.onDataLoaded += LoadTiles;

        OnTileDownloaded += TileDownloaded;
    }

    public void LoadTiles(List<FaceData> faceDataList)
    {
        List<(float, float)> latLonList = TileUtilities.GetLatLongList(faceDataList);

        // Convert each lat-long pair to tile coordinates and add to the set
        foreach ((float lat, float lon) in latLonList)
        {
            int x = TileUtilities.LonToTileX(lon, zoom);
            int y = TileUtilities.LatToTileY(lat, zoom);
            AddTileAndNeighbors(x, y);
        }
        
        tilesNeededCount = uniqueTileCoords.Count;
        // Start downloading each unique tile
        foreach (var tileCoord in uniqueTileCoords)
        {
            StartCoroutine(LoadTileFromDiskOrDownload(tileCoord.Item1, tileCoord.Item2, tileCoord.Item3));
        }
    }

    private void AddTileAndNeighbors(int x, int y)
    {
        // Add the original tile
        uniqueTileCoords.Add((x, y, zoom));

        // Add neighboring tiles
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue; // Skip the original tile
                uniqueTileCoords.Add((x + dx, y + dy, zoom));
            }
        }
    }

    private IEnumerator LoadTileFromDiskOrDownload(int x, int y, int zoom)
    {
        var tileKey = (x, y, zoom);
        if (tileCache.ContainsKey(tileKey))
        {
            Debug.Log($"Tile in cache! {x}, {y}, {zoom}");
            OnTileDownloaded?.Invoke(); // Call the method when a tile is in cache
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
                if (texture.LoadImage(fileData))
                {
                    tileCache[tileKey] = texture;
                    OnTileDownloaded?.Invoke(); // Call the method when a tile is loaded from disk
                }
                else
                {
                    Debug.LogError("Failed to load texture from disk");
                }
            }
            else
            {
                Debug.Log($"Downloading tile {x},{y}");
                yield return StartCoroutine(DownloadTile(x, y, zoom));
            }
        }
    }

    private IEnumerator DownloadTile(int x, int y, int zoom)
    {
        var tileKey = (x, y, zoom);
        string url = $"{BaseUrl}{zoom}/{x}/{y}.png";
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                tileCache[tileKey] = texture;
                SaveTileToDisk(texture, x, y, zoom);
                OnTileDownloaded?.Invoke(); // Call the method when a tile is downloaded
            }
            else
            {
                Debug.LogError("Error downloading tile: " + uwr.error);
                Debug.LogError(url);
            }
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

    private void TileDownloaded()
    {
        downloadedTileCount++;
        if (downloadedTileCount == tilesNeededCount)
        {
            OnAllTilesDownloaded?.Invoke(tileCache);
        }
    }

    public Dictionary<(int x, int y, int zoom), Texture2D> GetTileCache()
    {
        return tileCache;
    }

    void OnDestroy()
    {
        fileHandler.onDataLoaded -= LoadTiles;
        OnTileDownloaded -= TileDownloaded;
    }
}