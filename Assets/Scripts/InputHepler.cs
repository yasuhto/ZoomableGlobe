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

}
