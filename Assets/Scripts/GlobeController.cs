using UnityEngine;

public class GlobeController : MonoBehaviour
{
    private readonly static float DefaultFieldOfView = 60f;
    private readonly static float DoubleClickSpan = 0.3f;

    private int _ClickCount;
    private Vector3 _TargetAxis;
    private float _TargetAngle;
    private float _Direction;
    private Vector3 _TouchWorldPoint;

    private float _TargetCameraView;
    private float _TargetCameraViewDelta;

    private float _TargetCameraUpAngle;
    private float _TargetCameraUpSign;

    public Camera TargetCamera;
    public float FocusSpeed = 0.2f;
    public float ZoomRate = 0.75f;

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
        this._ClickCount++;
        this.Invoke("OnDoubleClick", DoubleClickSpan);
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
            this._TargetAngle -= this.FocusSpeed;

            //  回転軸axisに対してθ回転させる
            var center = this.transform.position;
            var angle = Mathf.Min(this._TargetAngle, this.FocusSpeed);
            var sign = this._Direction;
            this.TargetCamera.transform.RotateAround(center, this._TargetAxis, -1 * sign * angle);

            //  回転に合わせて拡大
            float view = Mathf.Clamp(value: this.TargetCamera.fieldOfView - this._TargetCameraViewDelta, min: this._TargetCameraView, max: this.TargetCamera.fieldOfView);
            this.TargetCamera.fieldOfView = view;

            //  回転で生じるカメラと対象オブジェクトのupのズレを計算
            var axis = this.TargetCamera.transform.forward;
            var from = Vector3.ProjectOnPlane(this.TargetCamera.transform.up, this.TargetCamera.transform.forward);
            var to = Vector3.ProjectOnPlane(this.transform.up, this.TargetCamera.transform.forward);
            var vecA = from - center;
            var vecB = to - center;
            angle = Vector3.SignedAngle(vecA, vecB, axis);
            this._TargetCameraUpAngle = Mathf.Abs(angle);
            this._TargetCameraUpSign = Mathf.Sign(angle);
        }

        //  カメラのupを対象オブジェクトのupに合わせる
        if (this._TargetCameraUpAngle > 0f)
        {
            this._TargetCameraUpAngle -= this.FocusSpeed;

            var axis = this.TargetCamera.transform.forward;
            var angle = this._TargetCameraUpAngle > 0 ?
                this.FocusSpeed : this.FocusSpeed + this._TargetCameraUpAngle;
            this.TargetCamera.transform.Rotate(axis, this._TargetCameraUpSign * angle, Space.World);
            Debug.Log($"{this.TargetCamera.transform.forward} : {this._TargetCameraUpAngle}");
        }
    }

    private void OnDoubleClick()
    {
        if (this._ClickCount != 2)
        {
            this._ClickCount = 0;
            //  ダブルクリックと判定されなかったのでカウントリセットして終了
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
        //  カメラの注視点とダブルクリック地点までの角度を保持

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

        //  拡大
        this._TargetCameraView = this.TargetCamera.fieldOfView * this.ZoomRate;
        this._TargetCameraViewDelta = (this.TargetCamera.fieldOfView - this._TargetCameraView) / (this._TargetAngle / this.FocusSpeed);
    }

    void OnDrawGizmos()
    {
        //Gizmos.DrawSphere(this._TouchWorldPoint, 5f);
    }
}
