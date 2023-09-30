using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;


public class BoundingBoxObject
{
    public float Height { get; set; }
    public float Left { get; set; }
    public float Top { get; set; }
    public float Width { get; set; }
}

public class  GeocodeObject
{
    public string Place_Id { get; set; }
    public string Licence { get; set; }
    public string Osm_Type { get; set; }
    public string Osm_Id { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }
    public string Class {  get; set; }
    public string Type { get; set; }
    public string Place_Rank { get; set; }
    public string Importance { get; set; }
    public string AddressType {  get; set; }
    public string Name { get; set; }
    public string Display_Name {  get; set; }
    public AddressObject Address { get; set; }
    public double[] BoundingBox { get; set; }
}

public class AddressObject
{
    public string House_Number { get; set; }
    public string Road { get; set; }
    public string Quarter { get; set; }
    public string Farm { get; set; }
    public string Municipality { get; set; }
    public string County { get; set; }
    public string ISO3166_2_lvl4 { get; set; }
    public string PostCode { get; set; }
    public string Country { get; set; }
    public string Country_Code { get; set; }
}

/// <summary>逆ジオコーディングクラス</summary>
public class LatLonToAddress : MonoBehaviour
{
    /// <summary>APIのパラメータテンプレートつきURL</summary>
    private const string ApiBaseUrl = "https://nominatim.openstreetmap.org/reverse?format=json&lat={0}&lon={1}";

    /// <summary>住所文字列</summary>
    public string Address { get; private set; }

    /// <summary>経緯度から住所文字列を取得</summary>
    /// <param name="longitude">経度</param>
    /// <param name="latitude">緯度</param>
    /// <returns>遅延評価用戻り値</returns>
    public IEnumerator GetAddrFromLonLat(float longitude, float latitude)
    {
        // URLに経緯度パラメータを埋め込み
        string url = string.Format(ApiBaseUrl, latitude, longitude);
        //ウェブリクエストを生成
        using (var request = UnityEngine.Networking.UnityWebRequest.Get(url))
        {
            //通信待ち
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                //  エラーなので終了
                yield break;
            }

            print(request.downloadHandler.text);

            // 結果JSONのデシリアライズ
            var geocode = JsonConvert.DeserializeObject<GeocodeObject>(request.downloadHandler.text);
            if (geocode.Address == null)
            {
                // 失敗したので終了
                yield break;
            }

            Address = geocode.Address.Country;
        }
    }
}
