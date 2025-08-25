using UnityEngine;

public class 相机控制 : MonoBehaviour
{
    [Header("跟随与注视目标")]
    [Tooltip("相机跟随的目标物体")] public Transform 跟随目标;
    [Tooltip("相机注视的焦点物体")] public Transform 注视焦点;

    [Header("视角环参数")]
    [Tooltip("上环高度")] public float 上环高度 = 5f;
    [Tooltip("中环高度")] public float 中环高度 = 2f;
    [Tooltip("下环高度")] public float 下环高度 = 0.5f;
    [Tooltip("基础跟随距离")] public float 基础距离 = 5f;
    [Tooltip("垂直角度限制")] public Vector2 垂直角度限制 = new Vector2(-30, 60);

    [Header("输入控制")]
    [Tooltip("水平旋转速度")] public float 水平灵敏度 = 100f;
    [Tooltip("垂直旋转速度")] public float 垂直灵敏度 = 50f;
    [Tooltip("滚轮缩放速度")] public float 滚轮灵敏度 = 2f;
    [Tooltip("输入平滑系数")][Range(0, 1)] public float 输入平滑值 = 0.1f;

    [Header("碰撞检测")]
    [Tooltip("碰撞检测层")] public LayerMask 障碍物层级;
    [Tooltip("碰撞安全距离")] public float 最小碰撞距离 = 0.5f;

    // 运行时变量
    private float 当前水平角度;
    private float 当前垂直角度;
    private float 当前实际距离;
    private Vector3 当前速度参考;

    void Start()
    {
        当前水平角度 = transform.eulerAngles.y;
        当前垂直角度 = transform.eulerAngles.x;
        当前实际距离 = 基础距离;
    }

    void LateUpdate()
    {
        if (跟随目标 == null || 注视焦点 == null) return;

        处理旋转输入();
        处理距离缩放();
        更新相机位置();
        避免碰撞();
    }

    // 处理鼠标/手柄输入
    void 处理旋转输入()
    {
        float 水平输入 = Input.GetAxis("Mouse X") * 水平灵敏度 * Time.deltaTime;
        float 垂直输入 = Input.GetAxis("Mouse Y") * 垂直灵敏度 * Time.deltaTime;

        // 平滑输入
        当前水平角度 = Mathf.SmoothDampAngle(当前水平角度, 当前水平角度 + 水平输入, ref 当前速度参考.x, 输入平滑值);
        当前垂直角度 = Mathf.SmoothDampAngle(当前垂直角度, 当前垂直角度 - 垂直输入, ref 当前速度参考.y, 输入平滑值);
        当前垂直角度 = Mathf.Clamp(当前垂直角度, 垂直角度限制.x, 垂直角度限制.y);
    }

    // 处理滚轮缩放
    void 处理距离缩放()
    {
        float 滚轮输入 = Input.GetAxis("Mouse ScrollWheel");
        当前实际距离 = Mathf.Clamp(当前实际距离 - 滚轮输入 * 滚轮灵敏度, 0.5f, 基础距离 * 2);
    }

    // 动态计算相机位置
    void 更新相机位置()
    {
        // 根据垂直角度插值计算环高度
        float 环高度插值 = Mathf.InverseLerp(垂直角度限制.x, 垂直角度限制.y, 当前垂直角度);
        float 当前环高度 = Mathf.Lerp(下环高度, 上环高度, 环高度插值);

        // 球坐标系计算位置
        Quaternion 旋转角度 = Quaternion.Euler(当前垂直角度, 当前水平角度, 0);
        Vector3 目标偏移 = 旋转角度 * new Vector3(0, 0, -当前实际距离);
        Vector3 期望位置 = 跟随目标.position + 目标偏移 + Vector3.up * 当前环高度;

        transform.position = Vector3.Slerp(transform.position, 期望位置, 输入平滑值 * 10 * Time.deltaTime);
        transform.LookAt(注视焦点.position);
    }

    // 防止穿模
    void 避免碰撞()
    {
        RaycastHit 碰撞信息;
        Vector3 检测方向 = (transform.position - 跟随目标.position).normalized;
        float 检测距离 = Vector3.Distance(跟随目标.position, transform.position);

        if (Physics.SphereCast(跟随目标.position, 0.2f, 检测方向, out 碰撞信息, 检测距离, 障碍物层级))
        {
            当前实际距离 = Mathf.Clamp(碰撞信息.distance - 最小碰撞距离, 0.5f, 基础距离);
        }
    }

    // 调试可视化
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(跟随目标.position + Vector3.up * 上环高度, 0.3f);
        Gizmos.DrawWireSphere(跟随目标.position + Vector3.up * 中环高度, 0.3f);
        Gizmos.DrawWireSphere(跟随目标.position + Vector3.up * 下环高度, 0.3f);
    }
}