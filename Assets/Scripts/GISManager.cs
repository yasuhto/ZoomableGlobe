using UnityEngine;
using TMPro;

public class GISManager : MonoBehaviour
{
    private TextMeshProUGUI _ScreenPositionText;
    private TextMeshProUGUI _WorldPositionText;
    private TextMeshProUGUI _GeoPositionText;
    private TextMeshProUGUI _GridPositionText;

    public enum MapType
    {
        MapType2D,
        MapType3D
    }
    public MapType GISMapType;

    public GameObject ScreenPositionObject;
    public GameObject WorldPositionObject;
    public GameObject GeoPositionObject;
    public GameObject GridPositionObject;

    public RectTransform MapImage;
    public GameObject MapSphere;

    // Start is called before the first frame update
    void Start()
    {
        this._ScreenPositionText = this.ScreenPositionObject.GetComponent<TextMeshProUGUI>();
        this._WorldPositionText = this.WorldPositionObject.GetComponent<TextMeshProUGUI>();
        this._GeoPositionText = this.GeoPositionObject.GetComponent<TextMeshProUGUI>();
        this._GridPositionText = this.GridPositionObject.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        this.UpdateGIS();
    }

    private void UpdateGIS()
    {
        this.UpdatePositionInfo();
    }

    private void UpdatePositionInfo()
    {
        // スクリーン座標の取得  
        var screenPos = Input.mousePosition;
        this._ScreenPositionText.text = screenPos.ToString();

        //  World座標変換
        if (!this.TryCalcWorldPosition(screenPos, this.GISMapType, out var worldPos))
        {
            return;
        }
        this._WorldPositionText.text = worldPos.ToString();

        //  Geo座標変換
        if (!this.TryCalcGeo(worldPos, this.GISMapType, out var geo))
        {
            return;
        }
        this._GeoPositionText.text = ToLatLngFormat(geo);

        //  Grid座標変換
        if (!this.TryCalcGridPos(geo, this.GISMapType, out var gridPos))
        {
            return;
        }
        this._GridPositionText.text = gridPos.ToString();
    }

    public static string ToLatLngFormat(Vector2 geo)
    {
        var lat = geo.x > 0 ? $"{Mathf.Abs(geo.x).ToString("0.000000")}'N" : $"{Mathf.Abs(geo.x).ToString("0.000000")}'S";
        var lng = geo.y > 0 ? $"{Mathf.Abs(geo.y).ToString("0.000000")}'E" : $"{Mathf.Abs(geo.y).ToString("0.000000")}'W";

        return $"{lat} {lng}";
    }

    private bool TryCalcGridPos(Vector2 geo, MapType mapType, out Vector2Int gridPos)
    {
        // NOTE:xy座標系が2D/3Dで逆になる

        gridPos = Vector2Int.zero;

        //  2D
        if (mapType == MapType.MapType2D)
        {
            gridPos.x = (int)(this.MapImage.rect.width * (geo.x + 180) / 360f);
            gridPos.y = (int)(this.MapImage.rect.height * (geo.y - 90) / 180f) * -1;
        }
        else
        {
            gridPos.x = (int)(Screen.height * (geo.x + 90) / 180f);
            gridPos.y = (int)(Screen.width * (geo.y + 180) / 360f);
        }

        return true;
    }

    private bool TryCalcGeo(Vector3 worldPos, MapType mapType, out Vector2 geo)
    {
        geo = Vector2.zero;

        //  2D
        if (mapType == MapType.MapType2D)
        {
            //  Image上のローカル座標とサイズから計算
            geo = new Vector2(worldPos.x / this.MapImage.rect.width * 360, worldPos.y / this.MapImage.rect.height * 180);
            return true;
        }

        //  3D
        geo = GeoFromGlobePosition(worldPos, MapSphere.transform.localScale.x * 0.5f);

        return true;

    }

    private bool TryCalcWorldPosition(Vector3 screenPos, MapType mapType, out Vector3 worldPos)
    {
        worldPos = Vector3.zero;

        //  2D
        if (mapType == MapType.MapType2D)
        {
            //worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            var canvas = this.MapImage.GetComponentInParent<Canvas>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.MapImage, screenPos, canvas.worldCamera, out var localPos);
            worldPos = localPos;
            return true;
        }

        //  3D
        var ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out var hit, Mathf.Abs(this.MapSphere.transform.localScale.magnitude)))
        {
            worldPos = hit.point;
            return true;
        }

        return false;
    }

    public static Vector2 GeoFromGlobePosition(Vector3 point, float radius)
    {
        float latitude = Mathf.Asin(point.y / radius);
        float longitude = Mathf.Atan2(point.z, point.x);
        return new Vector2(latitude * Mathf.Rad2Deg, longitude * Mathf.Rad2Deg);
    }
}
