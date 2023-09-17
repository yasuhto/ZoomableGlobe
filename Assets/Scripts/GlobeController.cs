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
    private float _TargetCameraViewDelta;

    public Camera TargetCamera;
    public float RotateSpeed;

    void Start()
    {
        this._TargetCameraView = this.TargetCamera.fieldOfView;
    }

    void Update()
    {
        this.FocusOn(this._TouchWorldPoint);
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

    /// <summary>
    /// 指定したワールド座標に徐々に注目する
    /// </summary>
    private void FocusOn(Vector3 focusPoint)
    {
        if (this._TargetAngle > 0f)
        {
            this._TargetAngle -= this.RotateSpeed;

            var preUp = this.TargetCamera.transform.up;

            //  回転軸axisに対してθ回転させる
            var center = this.transform.position;
            var angle = Mathf.Min(this._TargetAngle, this.RotateSpeed);
            var sign = this._Direction;
            this.TargetCamera.transform.RotateAround(center, this._TargetAxis, -1 * sign * angle);

            //  回転に合わせて拡大
            float view = Mathf.Clamp(value: this.TargetCamera.fieldOfView - this._TargetCameraViewDelta, min: this._TargetCameraView, max: this.TargetCamera.fieldOfView);
            this.TargetCamera.fieldOfView = view;

            //  ↑の回転により、カメラ自身のupと対象オブジェクトとupにズレが生じるので、打ち消す方向に回転
            //  TODO:回転が振動する原因が不明。z軸が０に収束してないのも問題

            //var axis = this.TargetCamera.transform.forward;
            //var from = preUp;
            //var cameraAngle = Vector3.SignedAngle(from, this.TargetCamera.transform.up, axis);
            //this.TargetCamera.transform.Rotate(axis, cameraAngle, Space.World);
            //Debug.Log($"{this.TargetCamera.transform.up} : {from} : {cameraAngle}");

            //this.TargetCamera.transform.LookAt(this.transform, this.transform.up);
        }
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

        this.SetFocusPoint();
    }

    /// <summary>
    /// フォーカス位置を設定する
    /// </summary>
    private void SetFocusPoint()
    {
        //  ダブルクリック地点がカメラの中心に収まるように回転させる

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
        this._TargetCameraViewDelta = (this.TargetCamera.fieldOfView - this._TargetCameraView) / (this._TargetAngle / this.RotateSpeed);
    }
}
