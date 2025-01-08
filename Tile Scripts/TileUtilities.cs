using System;
using System.Collections.Generic;

public static class TileUtilities
{
    public static int LatToTileY(double lat, int zoom)
    {
        return (int)Math.Floor((1 - Math.Log(Math.Tan(lat * Math.PI / 180) + 1 / Math.Cos(lat * Math.PI / 180)) / Math.PI) / 2 * (1 << zoom));
    }

    public static int LonToTileX(double lon, int zoom)
    {
        return (int)Math.Floor((lon + 180.0) / 360.0 * (1 << zoom));
    }

    public static (double, double) TileXYToLatLon(int x, int y, int zoom)
    {
        double n = Math.Pow(2, zoom);
        double lonDeg = x / n * 360.0 - 180.0;
        double latRad = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * y / n)));
        double latDeg = latRad * 180.0 / Math.PI;
        return (latDeg, lonDeg);
    }

    public static List<(float, float)> GetLatLongList(List<FaceData> faceDataList)
    {
        var latLongList = new List<(float, float)>();
        foreach (var faceData in faceDataList)
        {
            latLongList.Add((faceData.lat, faceData.lon));
        }
        return latLongList;
    }
}