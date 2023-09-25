using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MakerService : MonoBehaviour
{
    private IList<GameObject> _Makers = new List<GameObject>();

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

        }
    }

    public void CreateMaker(Vector3 touchWorldPoint, float radius)
    {
        var maker = Instantiate(this.MakerPrefab, touchWorldPoint, Quaternion.identity);
        var gchild = maker.transform.GetChild(0).transform.GetChild(0);
        gchild.GetComponent<TMP_Text>().SetText(CoordinateHelper.WorldPositionToGeo(touchWorldPoint, radius).ToString());

        this._Makers.Add(maker);
    }
}
