using UnityEngine;

public static class InputHepler
{
    public static float maxVerticalAngle = 85f;  // 最大仰角

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
            //Debug.DrawRay(ray.origin, ray.direction * 100, Color.green, 5, true);

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

#if false
        // マウスのドラックになるべく同期しつつ、カメラが上下逆さまにならないよう、up方向を維持するように回転
        RotateAroundOnSphereWithUp(target, center, axis, angle, vecA);
#else
        // マウスのドラッグに同期して自由に回転する
        target.RotateAround(center, axis, -angle);
#endif
    }

    /// <summary>
    /// 指定した3点からなる球面と角度を中心にターゲットを回転させ、up方向は維持します
    /// </summary>
    private static void RotateAroundOnSphereWithUp(Transform target, Vector3 center, Vector3 axis, float angle, Vector3 vecA)
    {
        // 小さい動きでaxisが無効な場合はスキップ
        if (axis.sqrMagnitude < 1e-6f) return;

        // --- 極では極軸を中心に回転させるので、向きを調整

        // from が カメラから見て極の「手前側 or 奥側」にあるか判定
        var camForward = Vector3.Cross(Vector3.up, target.right).normalized;
        var side = Vector3.Dot(vecA, camForward);
        if (side < 0f)
        {
            // 奥側にある場合は回転方向を反転
            angle *= -1f;
        }

        // --- 上下反転防止処理 ---

        // 回転を仮適用（後で取り消せるように）
        // FIXME：「仮にでも実行」された回転が1フレームでも反映されてしまう可能性がある。Quaternion で試算する
        target.RotateAround(center, axis, -angle);
        // カメラの「上」が下を向いていないかをチェック（85度以上傾いたら反転とみなす）
        if (Vector3.Dot(target.up, Vector3.up) < Mathf.Cos(maxVerticalAngle * Mathf.Deg2Rad))
        {
            // 反転しそうなら回転を打ち消す
            target.RotateAround(center, axis, angle);
        }

        // カメラを中心方向に向け、上方向をワールドYにする
        target.LookAt(center, Vector3.up);
    }


    /// <summary>
    /// 指定した点を中心にターゲットを拡縮させます
    /// </summary>
    public static void ZoomFromOnSphere(Vector3 targetPos, Camera camera, Vector3 sphereCenter, float view)
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

        // カメラを中心方向に向け、上方向をワールドYにする
        // TODO:カメラの回転後のカーソルのずれを打ち消す
        //camera.transform.LookAt(sphereCenter, Vector3.up);
    }
}
