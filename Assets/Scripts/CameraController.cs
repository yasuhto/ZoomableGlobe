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
        this.Rotate();
        this.Zoom();
    }

    /// <summary>
    /// 指定した球オブジェクトを中心に、ドラッグに追従してカメラを回転させます
    /// </summary>
    private void Rotate()
    {
        //  平面上のドラッグから、球面上のドラッグ区間（AB）を求める
        InputHepler.GetMouseDragFromTo(this.TargetSphere, this._TargetCamera, out var fromPos, out var toPos);
        InputHepler.RotateAroundFromToOnSphere(this.transform, fromPos, toPos, this.TargetSphere.position);
    }

    /// <summary>
    /// マウスカーソル位置を基準に、画面を拡縮します
    /// </summary>
    private void Zoom()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll == 0)
        {
            return;
        }

        float view = Mathf.Clamp(value: this._TargetCamera.fieldOfView - scroll * ZoomSpeed, min: CameraFieldOfViewMin, max: this._FieldOfViewDefault);
        if (view >= CameraFieldOfViewMin && view <= this._FieldOfViewDefault)
        {
            InputHepler.ZoomFromOnSphere(Input.mousePosition, this._TargetCamera, this.TargetSphere.position, view);
        }
    }
}
