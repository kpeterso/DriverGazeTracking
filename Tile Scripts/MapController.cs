using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapController : MonoBehaviour
{
    public delegate void TextureUpdate();
    public event TextureUpdate OnTextureUpdate;
    private TileManager tileManager;
    private int minTileX, maxTileX, minTileY, maxTileY, textureWidth, textureHeight;
    private double minLat, maxLat, minLon, maxLon;
    private readonly int zoom = 12;

    void Start()
    {
        tileManager = FindObjectOfType<TileManager>();
        tileManager.OnAllTilesDownloaded += TilesDownloaded;
    }

    private void TilesDownloaded(Dictionary<(int x, int y, int zoom), Texture2D> tileCache)
    {
        Texture2D combinedTexture = StitchTiles(tileCache);
        ScaleQuadToFitTexture(combinedTexture);
        SetImage(combinedTexture);
    }

    Texture2D StitchTiles(Dictionary<(int x, int y, int zoom), Texture2D> tileCache)
    {

        minTileX = tileCache.Keys.Min(k => k.x);
        maxTileX = tileCache.Keys.Max(k => k.x);
        minTileY = tileCache.Keys.Min(k => k.y);
        maxTileY = tileCache.Keys.Max(k => k.y);

        (minLat, minLon) = TileUtilities.TileXYToLatLon(minTileX, minTileY, zoom);
        (maxLat, maxLon) = TileUtilities.TileXYToLatLon(maxTileX + 1, maxTileY + 1, zoom);

        textureWidth = (maxTileX - minTileX + 1) * 256;
        textureHeight = (maxTileY - minTileY + 1) * 256;
        Debug.Log("Tile Dimensions: " + textureWidth + " " + textureHeight);
        Texture2D combinedTexture = new Texture2D(textureWidth, textureHeight);

        foreach (var tileEntry in tileCache)
        {
            int tileX = (tileEntry.Key.x - minTileX) * 256;
            int tileY = (maxTileY - tileEntry.Key.y) * 256; // Invert Y axis
            combinedTexture.SetPixels(tileX, tileY, 256, 256, tileEntry.Value.GetPixels());
        }

        combinedTexture.Apply();
        return combinedTexture;
    }

    private void SetImage(Texture2D image)
    {
        if(image != null)
        {
            Renderer quadRenderer = GetComponent<Renderer>();
            quadRenderer.material.mainTexture = image;
            OnTextureUpdate?.Invoke();
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

    

    public Vector2 LatLonToTextureCoordinates(float latitude, float longitude)
    {
        // Normalize latitude and longitude within the bounds
        double normalizedX = (longitude - minLon) / (maxLon - minLon);
        double normalizedY = (latitude - minLat) / (maxLat - minLat);
        normalizedY = 1-normalizedY;

        return new Vector2((float)normalizedX-.5f, (float)normalizedY-.5f);
    }

    void OnDestroy()
    {
        if (tileManager != null)
        {
            tileManager.OnAllTilesDownloaded -= TilesDownloaded;
        }
    }
}