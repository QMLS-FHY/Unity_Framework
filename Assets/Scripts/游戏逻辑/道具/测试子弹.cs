using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using 管理器;

public class 测试子弹 : MonoBehaviour, 池对象接口
{
    private Vector3 目标;
    private 对象管理器 对象管理器;
    public void 初始化()
    {
        目标 = transform.TransformPoint(0, 0, 50);
    }

    void Start()
    {
        this.对象管理器 = 对象管理器.实例;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, 目标, Time.deltaTime * 30);
        if (Vector3.Distance(transform.position, 目标) < 0.1)
        {
            this.对象管理器.回收对象(对象:gameObject);
        }
    }
}
