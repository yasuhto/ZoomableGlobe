using UnityEngine;

public class GlobeController : MonoBehaviour
{
    private readonly static float DefaultFieldOfView = 60f;
    private readonly static float DoubleClickSpan = 0.3f;

    private bool _EnableAlignmentAxis = false;
    private Vector3 _TouchWorldPoint;
    private Vector3 _CameraRotateAroundAxis;
    private float _CameraRotateAroundAngle;
    private float _CameraRotateAroundSign;
    private float _CameraView;
    private float _CameraViewDelta;
    private float _CameraUpAngle;
    private float _CameraUpSign;
    private int _ClickCount;

    public Camera TargetCamera;
    public float FocusSpeed = 0.2f;
    public float ZoomRate = 0.75f;

    void Start()
    {
        this._CameraView = this.TargetCamera.fieldOfView;
    }

    void Update()
    {
        this.FocusOn(this._TouchWorldPoint);
        this.AlignmentAxis();
    }

    public void OnClickDown()
    {
        this._ClickCount++;
        this.Invoke("OnDoubleClick", DoubleClickSpan);
    }

    public void OnScroll()
    {
        //  スクロール操作で拡縮するため、ダブルクリック時の拡縮設定をリセット
        this._CameraView = DefaultFieldOfView;
    }

    public void OnAlignmentAxisButton()
    {
        //  カメラと対象オブジェクトとのup方向のズレを補正

        var axis = this.TargetCamera.transform.forward;
        var from = Vector3.ProjectOnPlane(this.TargetCamera.transform.up, this.TargetCamera.transform.forward);
        var to = Vector3.ProjectOnPlane(this.transform.up, this.TargetCamera.transform.forward);
        var center = this.transform.position;
        var vecA = from - center;
        var vecB = to - center;
        var angle = Vector3.SignedAngle(vecA, vecB, axis);
        this._CameraUpAngle = Mathf.Abs(angle);
        this._CameraUpSign = Mathf.Sign(angle);

        this._EnableAlignmentAxis = true;
    }

    /// <summary>
    /// 指定したワールド座標に徐々に注目する
    /// </summary>
    private void FocusOn(Vector3 focusPoint)
    {
        if (this._CameraRotateAroundAngle > 0f)
        {
            this._CameraRotateAroundAngle -= this.FocusSpeed;

            //  回転軸axisに対してθ回転させる
            {
                var center = this.transform.position;
                var angle = Mathf.Min(this._CameraRotateAroundAngle, this.FocusSpeed);
                var sign = this._CameraRotateAroundSign;
                this.TargetCamera.transform.RotateAround(center, this._CameraRotateAroundAxis, -1 * sign * angle);
            }

            //  回転に合わせて拡大
            this.TargetCamera.fieldOfView = Mathf.Clamp(value: this.TargetCamera.fieldOfView - this._CameraViewDelta, min: this._CameraView, max: this.TargetCamera.fieldOfView);
        }
    }

    /// <summary>
    /// 地球の極軸をスクリーンに合わせます
    /// </summary>
    private void AlignmentAxis()
    {
        if (!this._EnableAlignmentAxis)
        {
            return;
        }

        //  カメラのupを対象オブジェクトのupに徐々に合わせる

        this._CameraUpAngle -= this.FocusSpeed;

        var axis = this.TargetCamera.transform.forward;
        var angle = this._CameraUpAngle > 0 ?
            this.FocusSpeed : this.FocusSpeed + this._CameraUpAngle;
        this.TargetCamera.transform.Rotate(axis, this._CameraUpSign * angle, Space.World);

        //  補正が終了したらフラグをおろす
        if (this._CameraUpAngle <= 0f)
        {
            this._EnableAlignmentAxis = false;
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

        this._CameraRotateAroundAxis = axis;
        this._CameraRotateAroundSign = Mathf.Sign(angle);
        this._CameraRotateAroundAngle = Mathf.Abs(angle);
        this._TouchWorldPoint = from;

        //  拡大
        this._CameraView = this.TargetCamera.fieldOfView * this.ZoomRate;
        this._CameraViewDelta = (this.TargetCamera.fieldOfView - this._CameraView) / (this._CameraRotateAroundAngle / this.FocusSpeed);
    }

    void OnDrawGizmos()
    {
        //Gizmos.DrawSphere(this._TouchWorldPoint, 5f);
    }
}
