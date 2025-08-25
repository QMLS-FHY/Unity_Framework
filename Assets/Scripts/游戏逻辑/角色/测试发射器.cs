using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using 管理器;

public class 测试发射器 : MonoBehaviour
{
    public GameObject 发射对象;
    private 对象管理器 对象管理器;
    void Start()
    {
        this.对象管理器= 对象管理器.实例;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 适配对象管理器新接口：类别、ID、销毁、预制件
            GameObject 子弹 = this.对象管理器.获取对象("子弹", null, true, 发射对象);
            if (子弹 != null)
            {
                子弹.transform.position = transform.position;
                子弹.transform.rotation = transform.rotation;
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            this.对象管理器.清除("子弹");
        }
        // 按下加号键创建UI“键位修改UI”
        if (Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.Equals))
        {
            管理器.UI管理器.实例.激活界面("键位修改UI模块");
        }
        // 按下减号键关闭最上层UI（以中层为例）
        if (Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetKeyDown(KeyCode.Minus))
        {
            管理器.UI管理器.实例.关闭最上层UI(UI.UI层级类型.中层);
        }
    }
}
