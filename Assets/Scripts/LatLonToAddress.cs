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

/// <summary>�t�W�I�R�[�f�B���O�N���X</summary>
public class LatLonToAddress : MonoBehaviour
{
    /// <summary>API�̃p�����[�^�e���v���[�g��URL</summary>
    private const string ApiBaseUrl = "https://nominatim.openstreetmap.org/reverse?format=json&lat={0}&lon={1}";

    /// <summary>�Z��������</summary>
    public string Address { get; private set; }

    /// <summary>�o�ܓx����Z����������擾</summary>
    /// <param name="longitude">�o�x</param>
    /// <param name="latitude">�ܓx</param>
    /// <returns>�x���]���p�߂�l</returns>
    public IEnumerator GetAddrFromLonLat(float longitude, float latitude)
    {
        // URL�Ɍo�ܓx�p�����[�^�𖄂ߍ���
        string url = string.Format(ApiBaseUrl, latitude, longitude);
        //�E�F�u���N�G�X�g�𐶐�
        using (var request = UnityEngine.Networking.UnityWebRequest.Get(url))
        {
            //�ʐM�҂�
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                //  �G���[�Ȃ̂ŏI��
                yield break;
            }

            print(request.downloadHandler.text);

            // ����JSON�̃f�V���A���C�Y
            var geocode = JsonConvert.DeserializeObject<GeocodeObject>(request.downloadHandler.text);
            if (geocode.Address == null)
            {
                // ���s�����̂ŏI��
                yield break;
            }

            Address = geocode.Address.Country;
        }
    }
}
