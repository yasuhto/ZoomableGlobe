using UnityEngine;

public class GlobeController : MonoBehaviour
{
    private readonly static float DefaultFieldOfView = 60f;

    private int _ClickCount;
    private Vector3 _TargetAxis;
    private float _TargetAngle;
    private float _Direction;
    private Vector3 _TouchWorldPoint;

    private float _TargetCameraView;

    public Camera TargetCamera;
    public float RotateSpeed;

    // Start is called before the first frame update
    void Start()
    {
        this._TargetCameraView = this.TargetCamera.fieldOfView;
    }

    // Update is called once per frame
    void Update()
    {
        if (this._TargetAngle > 0f)
        {
            this._TargetAngle -= this.RotateSpeed;

            //  回転軸axisに対してθ回転させる
            var center = this.transform.position;
            var angle = Mathf.Min(this._TargetAngle, this.RotateSpeed);
            var sign = this._Direction;
            this.TargetCamera.transform.RotateAround(center, this._TargetAxis, -1 * sign * angle);

            //  ↑の回転により、カメラ自身のupと対象オブジェクトとupにズレが生じるので、打ち消す方向に回転
            //  TODO:回転が振動する原因が不明。z軸が０に収束してないのも問題

            //var axis = this.TargetCamera.transform.forward;
            //var from = this.TargetCamera.transform.up;
            //var cameraAngle = Vector3.SignedAngle(from, this.transform.up, axis);
            //this.TargetCamera.transform.Rotate(axis, cameraAngle, Space.World);
            //Debug.Log($"{from} : {axis} : {cameraAngle}");

            this.TargetCamera.transform.LookAt(this.transform, this.transform.up);




        }

        if (this.TargetCamera.fieldOfView > this._TargetCameraView)
        {
            float view = Mathf.Clamp(value: this.TargetCamera.fieldOfView - this.RotateSpeed, min: 1f, max: this.TargetCamera.fieldOfView);
            if (view >= 1f && view <= this.TargetCamera.fieldOfView)
            {
                var screenPos = this.TargetCamera.WorldToScreenPoint(this._TouchWorldPoint);
                InputHepler.ZoomFromOnSphere(this.TargetCamera, screenPos, this.transform.position, view);
                
                this.TargetCamera.transform.LookAt(this.transform, this.transform.up);
            }
        }

    }

    public void OnClickDown()
    {
        _ClickCount++;
        this.Invoke("OnDoubleClick", 0.3f);
    }

    public void OnScroll()
    {
        //  スクロール操作で拡縮するため、ダブルクリック時の拡縮設定をリセット
        this._TargetCameraView = DefaultFieldOfView;
    }

    private void OnDoubleClick()
    {
        //ダブルクリックされているか
        if (this._ClickCount != 2)
        {
            this._ClickCount = 0; 
            return;
        }
        this._ClickCount = 0;

        //  ダブルクリック地点がカメラの中心に収まるように回転する

        if (!Physics.Raycast(this.TargetCamera.transform.position, this.TargetCamera.transform.forward, out var hit))
        {
            return;
        }

        var ray2 = this.TargetCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray2, out var hit2))
        {
            return;
        }

        var from = hit2.point;
        var to = hit.point;
        var center = this.transform.position;

        //  カメラの注視点とダブルクリック地点までの角度を保持

        //  回転軸axisは、球面上の中心点（O）とABからなる平面の法線
        var vecA = from - center;
        var vecB = to - center;
        var axis = Vector3.Cross(vecA, vecB);

        //  球面上の中心点（O）から軸axisに対する回転角度（θ）を求める
        var angle = Vector3.SignedAngle(from, to, axis);

        this._TargetAxis = axis;
        this._Direction = Mathf.Sign(angle);
        this._TargetAngle = Mathf.Abs(angle);
        this._TouchWorldPoint = from;

        //  現状の倍まで拡大する
        this._TargetCameraView = this.TargetCamera.fieldOfView * 0.75f;
    }
}
