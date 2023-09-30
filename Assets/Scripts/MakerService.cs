using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class MakerService : MonoBehaviour
{
    private IList<GameObject> _Makers = new List<GameObject>();
    public LatLonToAddress LatLonToAddress;

    public GameObject MakerPrefab; 
    public IReadOnlyCollection<GameObject> Makers => this._Makers.ToArray();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //  カメラに映らないマーカーを非表示にする
        foreach (var maker in this._Makers)
        {
            var cameraLength = (Camera.main.transform.position - maker.transform.position).magnitude;
            //if (cameraLength > 0)
            {
                //maker.SetActive(cameraLength < 14);
            }
        }
    }

    public IEnumerator CreateMaker(Vector3 touchWorldPoint, float radius)
    {
        var maker = Instantiate(this.MakerPrefab, touchWorldPoint, Quaternion.identity);

        var objMaker = maker.GetComponent<ObjectMarker>();
        objMaker.LatLon = CoordinateHelper.WorldPositionToGeo(touchWorldPoint, radius);
#if false
        var pos = Vector3.MoveTowards(touchWorldPoint, Camera.main.transform.position, 0.5f);
        maker.transform.position = pos;

        var textObj = maker.GetComponentInChildren<TMP_Text>();
        textObj.SetText(objMaker.LatLon.ToString());
        yield return StartCoroutine(this.LatLonToAddress.GetAddrFromLonLat(objMaker.LatLon.y, objMaker.LatLon.x));
        textObj.SetText(this.LatLonToAddress.Address);
#else
        var gchild = maker.transform.GetChild(0).transform.GetChild(0);
        gchild.GetComponent<TMP_Text>().SetText(objMaker.LatLon.ToString());

        yield return StartCoroutine(this.LatLonToAddress.GetAddrFromLonLat(objMaker.LatLon.y, objMaker.LatLon.x));
        gchild.GetComponent<TMP_Text>().SetText(this.LatLonToAddress.Address);
#endif

        this._Makers.Add(maker);
    }
}
