using UnityEngine;

public class CameraController : MonoBehaviour
{
    private static readonly float CameraFieldOfViewMin = 1.0f;

    private Camera _TargetCamera;
    private float _FieldOfViewDefault;

    public Transform TargetSphere; // 拡大縮小の中心となるオブジェクト（例: 地球オブジェクト）
    public float ZoomSpeed = 3f; // 拡大縮小の速度

    void Start()
    {
        this._TargetCamera = this.GetComponent<Camera>();
        this._FieldOfViewDefault = this._TargetCamera.fieldOfView;
    }

    void Update()
    {
        this._Rotate();
        this._Zoom();
    }

    /// <summary>
    /// 指定した球オブジェクトを中心に、ドラッグに追従してカメラを回転させます
    /// </summary>
    private void _Rotate()
    {
        //  平面上のドラッグから、球面上のドラッグ区間（AB）を求める
        InputHepler.GetMouseDragFromTo(this.TargetSphere, this._TargetCamera, out var fromPos, out var toPos);

        //  回転軸axisは、球面上の中心点（O）とABからなる平面の法線
        var vecA = fromPos - this.TargetSphere.position;
        var vecB = toPos - this.TargetSphere.position;
        var axis = Vector3.Cross(vecA, vecB);

        //  球面上の中心点（O）から軸axisに対する回転角度（θ）を求める
        var dragAngle = Vector3.SignedAngle(fromPos, toPos, axis);

        //  回転軸axisに対してθ回転させる
        //this.target.Rotate(axis, dragAngle, Space.World);
        this.transform.RotateAround(this.TargetSphere.position, axis, -1 * dragAngle);
    }

    /// <summary>
    /// マウスカーソル位置を基準に、画面を拡縮します
    /// </summary>
    private void _Zoom()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll == 0)
        {
            return;
        }

        float view = Mathf.Clamp(value: this._TargetCamera.fieldOfView - scroll * ZoomSpeed, min: CameraFieldOfViewMin, max: this._FieldOfViewDefault);
        if (view < CameraFieldOfViewMin || view > this._FieldOfViewDefault)
        {
            //  範囲外になったら調整しない
            return;
        }

        //  拡縮前の衝突点を計算
        var ray = this._TargetCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit))
        {
            //  マウスがターゲットと重なっていない場合、オブジェクトの中心を基準に拡大
            this._TargetCamera.fieldOfView = view;
            return;
        }

        //  マウスがターゲットに重なる場合、カーソル位置を基準に拡大

        //  拡縮後の衝突点を計算
        this._TargetCamera.fieldOfView = view;
        var ray2 = this._TargetCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray2, out var hit2))
        {
            //  拡縮後にカーソルがターゲットと重ならなくなったとき
            return;
        }
        //Debug.DrawRay(ray2.origin, ray2.direction * 100, Color.green, 5, true);

        //  ホイール操作による拡縮前後のカーソル位置からの衝突点（A、B）をそれぞれ求める
        var fromPos = hit.point;
        var toPos = hit2.point;

        //  回転軸axisは、球面上の中心点（O）とABからなる平面の法線
        var vecA = fromPos - this.TargetSphere.position;
        var vecB = toPos - this.TargetSphere.position;
        var axis = Vector3.Cross(vecA, vecB);

        //  球面上の中心点（O）から軸axisに対する回転角度（θ）を求める
        //  拡縮時のカーソルのズレを打ち消す方向
        var dragAngle = Vector3.SignedAngle(toPos, fromPos, axis);

        //  回転軸axisに対してθ回転させる
        this.transform.RotateAround(this.TargetSphere.position, axis, dragAngle);
    }
}
