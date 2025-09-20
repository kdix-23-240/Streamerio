using UnityEngine;

public class StageConnectionPoint : MonoBehaviour
{
    [Header("接続アンカー(必須)")]
    public Transform LeftPoint;
    public Transform RightPoint;

    public Vector3 LeftPosition  => LeftPoint  ? LeftPoint.position  : transform.position;
    public Vector3 RightPosition => RightPoint ? RightPoint.position : transform.position;

    /// <summary>このステージの LeftPoint を指定ワールド座標に合わせる</summary>
    public void AlignLeftTo(Vector3 worldPos)
    {
        Vector3 delta = worldPos - LeftPosition;
        transform.position += delta;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (LeftPoint)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(LeftPosition, 0.3f);
        }
        if (RightPoint)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(RightPosition, 0.3f);
        }
        if (LeftPoint && RightPoint)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(LeftPosition, RightPosition);
        }
    }
#endif
}