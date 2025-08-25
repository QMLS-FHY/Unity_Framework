using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using 管理器;
//跳跃需用到刚体组件
public class 输入控制 : MonoBehaviour
{
    [Header("模式")]
    [Tooltip("T=第一视角；F=第三视角")]
    public bool 视角模式;
    [Header("对象绑定")]
    public GameObject 操控对象;
    public GameObject 视角对象;
    [Header("预设值")]
    public float 视角灵敏度;
    public float 移动速度;
    public float 跳跃力度;
    public float 抬头限制;
    public float 低头限制;
    public float 视角距离;
    public float 后抬角度;
    [Header("展示")]
    public Vector2 移动输入;
    public Vector2 视角输入;
    public Vector3 当前视角;
    public Vector3 正面方向;

    //私有变量
    private bool 当前视角模式;
    private 输入预设 输入绑定;
    private InputAction 移动行为;
    private InputAction 视角行为;
    private void Awake()
    {
        当前视角模式 = 视角模式;
        输入绑定 = 输入管理器.输入预设实例;
    }
    private void OnEnable()
    {
        //绑定开启值行为
        移动行为 = 输入绑定.main.move;
        视角行为 = 输入绑定.main.look;
        移动行为.Enable();
        视角行为.Enable();
        //绑定开启按钮行为
        输入绑定.main.jump.performed += 跳跃触发;
        输入绑定.main.jump.Enable();
    }

    private void OnDisable()
    {
        //关闭值行为
        移动行为.Disable();
        视角行为.Disable();
        //关闭按钮行为
        输入绑定.main.jump.Disable();
    }
    private void Start()
    {
        if (当前视角模式)//第一视角
        {
            视角对象.transform.position = 操控对象.transform.position;
            视角对象.transform.rotation = 操控对象.transform.rotation;
        }
        else//第三视角
        {
            视角对象.transform.position = 操控对象.transform.position - 操控对象.transform.forward * 视角距离;
            视角对象.transform.RotateAround(操控对象.transform.position, Vector3.right, 后抬角度);
            视角对象.transform.LookAt(操控对象.transform);
        }
    }
    private void Update()
    {
        //获取值
        移动输入 = 移动行为.ReadValue<Vector2>();
        视角输入 = 视角行为.ReadValue<Vector2>();
        //执行
        位置移动(移动输入);
        视角移动(视角输入);
        //视角切换
        if(视角模式 != 当前视角模式)
        {
            当前视角模式 = 视角模式;
            if (视角模式)//切换视角模式
            {
                视角对象.transform.position = 操控对象.transform.position;
            }
            else//切换第三视角
            {
                视角对象.transform.Translate(Vector3.back * 视角距离);
            }
        }
        //同步Y轴位置
        if(当前视角模式)视角对象.transform.position = new Vector3(视角对象.transform.position.x, 操控对象.transform.position.y, 视角对象.transform.position.z);
    }

    private void 视角移动(Vector2 视角输入)
    {
        if (视角模式)
        {
            当前视角 = 视角对象.transform.eulerAngles;
            当前视角.y += 视角输入.x * 视角灵敏度 * Time.deltaTime;
            当前视角.x -= 视角输入.y * 视角灵敏度 * Time.deltaTime;
            当前视角.x = 当前视角.x % 360;
            if (当前视角.x < 0) 当前视角.x += 360;
            当前视角.x = (当前视角.x >= 0 && 当前视角.x <= 低头限制) || (当前视角.x >= 360 - 抬头限制 && 当前视角.x <= 360)? 当前视角.x: (当前视角.x < 180 ? 低头限制 : 360 - 抬头限制);
            当前视角.z = 0f;
            视角对象.transform.eulerAngles = 当前视角;
            正面方向.y = 当前视角.y;
            操控对象.transform.eulerAngles = 正面方向;
            
        }
        else
        {
            视角对象.transform.RotateAround(操控对象.transform.position, 操控对象.transform.up, 视角输入.x * 视角灵敏度 * Time.deltaTime);
            当前视角.y = 视角对象.transform.eulerAngles.y;
            当前视角.x = 视角对象.transform.eulerAngles.x - 视角输入.y * 视角灵敏度 * Time.deltaTime;
            当前视角.x = 当前视角.x % 360;
            if (当前视角.x < 0) 当前视角.x += 360;
            当前视角.x = (当前视角.x >= 0 && 当前视角.x <= 低头限制) || (当前视角.x >= 360 - 抬头限制 && 当前视角.x <= 360) ? 当前视角.x : (当前视角.x < 180 ? 低头限制 : 360 - 抬头限制);
            视角对象.transform.RotateAround(操控对象.transform.position, 视角对象.transform.right, 当前视角.x - 视角对象.transform.eulerAngles.x);
            正面方向.y = 当前视角.y;
            操控对象.transform.eulerAngles = 正面方向;
        }
    }

    private void 位置移动(Vector2 移动输入)
    {
        var 移动正面方向 = Quaternion.Euler(0, 视角对象.transform.eulerAngles.y, 0) * new Vector3(移动输入.x, 0, 移动输入.y);
        操控对象.transform.position += 移动正面方向 * 移动速度 * Time.deltaTime;
        视角对象.transform.position += 移动正面方向 * 移动速度 * Time.deltaTime;
    }

    private void 跳跃触发(InputAction.CallbackContext obj)
    {
        gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * 跳跃力度, ForceMode.Impulse);
    }
}
