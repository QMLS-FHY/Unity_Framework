using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class 角色控制 : MonoBehaviour
{
    [Tooltip("角色控制器")] public CharacterController 角色控制器;
    [Tooltip("重力加速度")] private float 重力加速度 = 9.8f;
    private float 水平输入;
    private float 垂直输入;

    [Header("相机")]
    [Tooltip("摄像机相机")] public Transform 主相机;
    [Tooltip("摄像机高度变化的平滑值")] public float 插值速度 = 10f;
    [Tooltip("当前摄像机的位置")] private Vector3 相机本地位置;
    [Tooltip("当前摄像机的高度")] private float 当前高度;

    [Header("移动")]
    [Tooltip("角色行走的速度")] public float 行走速度 = 6f;
    [Tooltip("角色奔跑的速度")] public float 奔跑速度 = 9f;
    [Tooltip("角色下蹲的速度")] public float 下蹲速度 = 3f;
    [Tooltip("角色移动的方向")] private Vector3 移动方向;
    [Tooltip("当前速度")] private float 当前速度;
    [Tooltip("是否奔跑")] private bool 是否奔跑;

    [Header("地面检测")]
    [Tooltip("地面检测位置")] public Transform 地面检测位置;
    [Tooltip("地面检测半径")] public float 检测半径 = 0.4f;
    [Tooltip("是否在地面")] private bool 是否在地面;

    [Header("头顶检测")]
    [Tooltip("头顶检测位置")] public Transform 头顶检测位置;
    [Tooltip("盒子半长、半宽、半高")] public Vector3 检测盒半尺寸 = new Vector3(0.4f, 0.5f, 0.4f);
    [Tooltip("判断玩家是否可以站立")] private bool 是否可以站立;

    [Header("斜坡检测")]
    [Tooltip("斜坡射线长度")] public float 斜坡射线长度 = 0.2f;
    [Tooltip("是否在斜坡")] private bool 是否在斜坡;

    [Header("跳跃")]
    [Tooltip("角色跳跃的高度")] public float 跳跃高度 = 2.5f;
    [Tooltip("判断是否在跳跃")] private bool 是否跳跃中;

    [Header("下蹲")]
    [Tooltip("正常站立时玩家高度")] private float 站立高度;
    [Tooltip("下蹲时候的玩家高度")] private float 下蹲高度;
    [Tooltip("判断玩家是否在下蹲")] private bool 是否下蹲;

    [Header("斜坡")]
    [Tooltip("走斜坡时施加的力度")] public float 斜坡力度 = 6.0f;

    void Start()
    {
        站立高度 = 角色控制器.height;
        下蹲高度 = 站立高度 / 2;
        相机本地位置 = 主相机.localPosition;
        当前速度 = 行走速度;
    }

    void Update()
    {
        水平输入 = Input.GetAxis("Horizontal");
        垂直输入 = Input.GetAxis("Vertical");

        //地面检测
        是否在地面 = 是否在地面检测();

        //头顶检测
        是否可以站立 = 是否可以站立检测();

        //斜坡检测
        是否在斜坡 = 是否在斜坡检测();

        设置速度();
        控制奔跑();
        控制下蹲();
        控制移动();
        控制跳跃();
    }

    // 速度设置
    void 设置速度()
    {
        if (是否奔跑)
        {
            当前速度 = 奔跑速度;
        }
        else if (是否下蹲)
        {
            当前速度 = 下蹲速度;
        }
        else
        {
            当前速度 = 行走速度;
        }
    }

    // 控制奔跑
    void 控制奔跑()
    {
        if (Input.GetKey(KeyCode.LeftShift) && !是否下蹲)
        {
            是否奔跑 = true;
        }
        else
        {
            是否奔跑 = false;
        }
    }

    // 控制下蹲
    void 控制下蹲()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            下蹲(true);
        }
        else
        {
            下蹲(false);
        }
    }

    // 控制移动
    void 控制移动()
    {
        if (是否在地面)
        {
            移动方向 = transform.right * 水平输入 + transform.forward * 垂直输入;
            移动方向 = 移动方向.normalized;
        }
    }

    // 控制跳跃
    void 控制跳跃()
    {
        if (Input.GetButtonDown("Jump") && 是否在地面)
        {
            是否跳跃中 = true;
            移动方向.y = 跳跃高度;
        }
        移动方向.y -= 重力加速度 * Time.deltaTime;
        角色控制器.Move(移动方向 * Time.deltaTime * 当前速度);

        控制斜坡();
        是否跳跃中 = false;
    }

    // 控制斜坡
    public void 控制斜坡()
    {
        if (是否在斜坡 && !是否跳跃中)
        {
            移动方向.y = 角色控制器.height / 2 * 斜坡射线长度;
            角色控制器.Move(Vector3.down * 角色控制器.height / 2 * 斜坡力度 * Time.deltaTime);
        }
    }

    // 下蹲/站立切换
    public void 下蹲(bool 新下蹲状态)
    {
        if (!新下蹲状态 && !是否可以站立) return;
        是否下蹲 = 新下蹲状态;
        float 目标高度 = 是否下蹲 ? 下蹲高度 : 站立高度;
        float 高度变化量 = 目标高度 - 角色控制器.height;
        角色控制器.height = 目标高度;
        角色控制器.center += new Vector3(0, 高度变化量 / 2, 0);

        float 目标摄像机高度 = 是否下蹲 ? 相机本地位置.y / 2 + 角色控制器.center.y : 相机本地位置.y;
        当前高度 = Mathf.Lerp(当前高度, 目标摄像机高度, 插值速度 * Time.deltaTime);
        主相机.localPosition = new Vector3(相机本地位置.x, 当前高度, 相机本地位置.z);
    }

    // 是否可以站立
    bool 是否可以站立检测()
    {
        Collider[] 碰撞体数组 = Physics.OverlapBox(头顶检测位置.position, 检测盒半尺寸);
        foreach (Collider 碰撞体 in 碰撞体数组)
        {
            if (碰撞体.gameObject != gameObject && !是否为子物体(碰撞体.transform, transform))
            {
                return false;
            }
        }
        return true;
    }

    // 地面检测
    bool 是否在地面检测()
    {
        Collider[] 碰撞体数组 = Physics.OverlapSphere(地面检测位置.position, 检测半径);
        foreach (Collider 碰撞体 in 碰撞体数组)
        {
            if (碰撞体.gameObject != gameObject && !是否为子物体(碰撞体.transform, transform))
            {
                return true;
            }
        }
        return false;
    }

    // 斜坡检测
    public bool 是否在斜坡检测()
    {
        RaycastHit 碰撞信息;
        if (Physics.Raycast(transform.position + 角色控制器.height / 2 * Vector3.down, Vector3.down, out 碰撞信息, 角色控制器.height / 2 * 斜坡射线长度))
        {
            return 碰撞信息.normal != Vector3.up;
        }
        return false;
    }

    // 子物体关系判断
    bool 是否为子物体(Transform 子物体, Transform 父物体)
    {
        while (子物体 != null)
        {
            if (子物体 == 父物体)
            {
                return true;
            }
            子物体 = 子物体.parent;
        }
        return false;
    }

    // 调试可视化
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(头顶检测位置.position, 检测盒半尺寸 * 2f);
        Gizmos.DrawWireSphere(地面检测位置.position, 检测半径);
        Debug.DrawRay(transform.position + 角色控制器.height / 2 * Vector3.down, Vector3.down * 角色控制器.height / 2 * 斜坡射线长度, Color.blue);
    }
}