using UnityEngine;

public static class CoordinateHelper
{
    /// <summary>
    /// タッチした球体上のワールド座標に変換します
    /// </summary>
    public static bool TryTouchPointToWorldPosition(Camera camera, Vector3 touch, out Vector3 worldPos)
    {
        worldPos = Vector3.zero;

        var ray = camera.ScreenPointToRay(touch);
        if (Physics.Raycast(ray, out var hit))
        {
            worldPos = hit.point;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 球体上のワールド座標を緯度経度に変換します
    /// </summary>
    public static Vector2 WorldPositionToGeo(Vector3 point, float radius)
    {
        float latitude = Mathf.Asin(point.y / radius);
        float longitude = Mathf.Atan2(point.z, point.x);
        return new Vector2(latitude * Mathf.Rad2Deg, longitude * Mathf.Rad2Deg);
    }

    /// <summary>
    /// 緯度経度をグリッド座標に変換します
    /// </summary>
    public static Vector2Int GeoToGrid(Vector2 geo, int texHeight, int texWidth)
    {
        var gridPos = Vector2Int.zero;

        gridPos.x = (int)(texHeight * (geo.x + 90) / 180f);
        gridPos.y = (int)(texWidth * (geo.y + 180) / 360f);

        return gridPos;
    }

    /// <summary>
    /// タッチした球体上のテクスチャ座標を返します
    /// </summary>
    public static Vector2 TouchPointToTextureUV(Camera camera, Vector3 touch)
    {
        var ray = camera.ScreenPointToRay(touch);
        return Physics.Raycast(ray, out var hit) ? hit.textureCoord : Vector2.zero;
    }

    /// <summary>
    /// 緯度経度フォーマットに変換
    /// </summary>
    public static string ToLatLngFormat(Vector2 geo)
    {
        var lat = geo.x > 0 ? $"{Mathf.Abs(geo.x).ToString("0.000000")}'N" : $"{Mathf.Abs(geo.x).ToString("0.000000")}'S";
        var lng = geo.y > 0 ? $"{Mathf.Abs(geo.y).ToString("0.000000")}'E" : $"{Mathf.Abs(geo.y).ToString("0.000000")}'W";

        return $"{lat} {lng}";
    }
}
