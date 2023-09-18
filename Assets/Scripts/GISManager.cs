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
    public Camera TargetCamera;

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
        var screenPos = Input.mousePosition;

        this._ScreenPositionText.text = screenPos.ToString();
        this._WorldPositionText.text = this.TryCalcWorldPosition(screenPos, this.GISMapType, out var worldPos) ? worldPos.ToString() : string.Empty;
        this._GeoPositionText.text = this.TryCalcGeo(worldPos, this.MapSphere.transform.localScale.x * 0.5f, this.GISMapType, out var geo) ? CoordinateHelper.ToLatLngFormat(geo) : string.Empty;
        this._GridPositionText.text = this.TryCalcGridPos(geo, this.GISMapType, out var gridPos) ? gridPos.ToString() : string.Empty;
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
            var uv = CoordinateHelper.TouchPointToTextureUV(this.TargetCamera, Input.mousePosition);
            var texHeight = this.MapSphere.GetComponent<Renderer>().material.mainTexture.height;
            var texWidth = this.MapSphere.GetComponent<Renderer>().material.mainTexture.width;

            gridPos.x = (int)Mathf.Round(uv.x * texHeight);
            gridPos.y = (int)Mathf.Round(uv.y * texWidth);
            //gridPos = CoordinateHelper.GeoToGrid(geo, texHeight, texWidth);
        }

        return true;
    }

    private bool TryCalcGeo(Vector3 worldPos, float radius, MapType mapType, out Vector2 geo)
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
        geo = CoordinateHelper.WorldPositionToGeo(worldPos, radius);

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

        return CoordinateHelper.TryTouchPointToWorld(this.TargetCamera, screenPos, out worldPos);
    }
}
