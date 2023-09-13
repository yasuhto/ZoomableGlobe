using UnityEngine;

public static class InputHepler
{
    private static Vector3 StartTouchPosition;
  
    /// <summary>
    /// マウスの左クリックでドラッグした変化量を返す
    /// </summary>
    public static Vector3 GetMouseDragDelta()
    {
        var dragDelta = Vector3.zero;

        //  ドラッグ開始位置を保存
        if (Input.GetMouseButtonDown(0))
        {
            StartTouchPosition = Input.mousePosition;
        }

        //  ドラッグ中は前フレームとの差を加算する
        if (Input.GetMouseButton(0))
        {
            dragDelta = Input.mousePosition - StartTouchPosition;
            StartTouchPosition = Input.mousePosition;
        }

        return dragDelta;
    }

    /// <summary>
    /// マウスの左クリックでドラッグした際にレイキャストでヒットした開始と終了座標を返す
    /// </summary>
    public static void GetMouseDragFromTo(Transform target, Camera camera, out Vector3 from, out Vector3 to)
    {
        from = Vector3.zero;
        to = Vector3.zero;

        //  ドラッグ開始位置を保存
        if (Input.GetMouseButtonDown(0))
        {
            StartTouchPosition = Input.mousePosition;
        }

        //  ドラッグ中は前フレームとの差を加算する
        if (Input.GetMouseButton(0))
        {
            //  カメラ方向から見たドラッグの回転角を計算

            var ray = camera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit))
            {
                return;
            }
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.green, 5, true);

            var ray2 = camera.ScreenPointToRay(StartTouchPosition);
            if (!Physics.Raycast(ray2, out var hit2))
            {
                return;
            }

            from = hit2.point;
            to = hit.point;

            StartTouchPosition = Input.mousePosition;
        }
    }

    /// <summary>
    /// 指定した3点からなる球面と角度を中心にターゲットを回転させます
    /// </summary>
    public static void RotateAroundFromToOnSphere(Transform target, Vector3 from, Vector3 to, Vector3 center)
    {
        //  回転軸axisは、球面上の中心点（O）とABからなる平面の法線
        var vecA = from - center;
        var vecB = to - center;
        var axis = Vector3.Cross(vecA, vecB);

        //  球面上の中心点（O）から軸axisに対する回転角度（θ）を求める
        var angle = Vector3.SignedAngle(from, to, axis);

        //  回転軸axisに対してθ回転させる
        target.RotateAround(center, axis, -1 * angle);
    }

    /// <summary>
    /// 指定した点を中心にターゲットを拡縮させます
    /// </summary>
    public static void ZoomFromOnSphere(Camera camera, Vector3 targetPos, Vector3 sphereCenter, float view)
    {
        //  拡縮前の衝突点を計算
        var ray = camera.ScreenPointToRay(targetPos);
        if (!Physics.Raycast(ray, out var hit))
        {
            //  マウスがターゲットと重なっていない場合、オブジェクトの中心を基準に拡大
            camera.fieldOfView = view;
            return;
        }

        //  マウスがターゲットに重なる場合、カーソル位置を基準に拡大

        //  拡縮後の衝突点を計算
        camera.fieldOfView = view;
        var ray2 = camera.ScreenPointToRay(targetPos);
        if (!Physics.Raycast(ray2, out var hit2))
        {
            //  拡縮後にカーソルがターゲットと重ならなくなったとき
            return;
        }

        //  ホイール操作による拡縮前後のカーソル位置からの衝突点（A、B）をそれぞれ求める
        var fromPos = hit.point;
        var toPos = hit2.point;

        //  回転軸axisは、球面上の中心点（O）とABからなる平面の法線
        var vecA = fromPos - sphereCenter;
        var vecB = toPos - sphereCenter;
        var axis = Vector3.Cross(vecA, vecB);

        //  球面上の中心点（O）から軸axisに対する回転角度（θ）を求める
        //  拡縮時のカーソルのズレを打ち消す方向
        var dragAngle = Vector3.SignedAngle(toPos, fromPos, axis);

        //  回転軸axisに対してθ回転させる
        camera.transform.RotateAround(sphereCenter, axis, dragAngle);
    }

}
