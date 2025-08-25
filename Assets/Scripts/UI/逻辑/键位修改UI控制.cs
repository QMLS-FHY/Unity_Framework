using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System;
using 管理器;
using UI;

public class 键位修改UI控制 : UI基类
{
    [Header("属性")]
    [Tooltip("行为")][SerializeField]
    private InputActionReference 行为;  
    [Tooltip("绑定下标")][SerializeField][Range(0, 10)]
    private int 绑定下标;
    [Tooltip("排除鼠标")][SerializeField]
    private bool 排除鼠标 = true;
    [Tooltip("绑定显示规则")][SerializeField]
    private InputBinding.DisplayStringOptions 绑定显示规则;

    [Header("信息(仅展示)")]
    [Tooltip("行为名称")][SerializeField]
    private string 行为名称;
    [Tooltip("绑定的下标")][SerializeField]
    private int 绑定的下标;
    [Tooltip("绑定的信息")][SerializeField]
    private InputBinding 绑定信息;

    [Header("UI")]
    [Tooltip("行为名称覆盖文本")][SerializeField]
    private string 展示文本;
    [Tooltip("行为名称文本显示组件")][SerializeField]
    private TMP_Text 行为文字组件;
    [Tooltip("绑定键文本显示组件")][SerializeField]
    private TMP_Text 绑定文本组件;
    [Tooltip("绑定按钮组件")][SerializeField]
    private Button 绑定按钮组件;
    [Tooltip("重置按钮组件")][SerializeField]
    private Button 重置按钮组件;


    private void OnEnable()//脚本启用时
    {
        绑定按钮组件.onClick.AddListener(() => 绑定触发事件());
        重置按钮组件.onClick.AddListener(() => 重置触发事件());

        if (行为 != null)
        {
            输入管理器.加载行为按键(行为名称);
            获取行为信息();
            更新UI();
        }

        输入管理器.改绑完成后事件 += 更新UI;
        输入管理器.改绑取消后事件 += 更新UI;
    }

    private void OnDisable()//脚本停用时
    {
        输入管理器.改绑完成后事件 -= 更新UI;
        输入管理器.改绑取消后事件 -= 更新UI;
    }

    private void OnValidate()//检查器数据更新时
    {
        if (行为 == null)
            return;

        获取行为信息();
        更新UI();
    }
    /// <summary>
    /// 获取行为信息
    /// </summary>
    private void 获取行为信息()
    {
        if (行为.action != null)
            行为名称 = 行为.action.name;

        if (行为.action.bindings.Count > 绑定下标)
        {
            绑定的下标 = 绑定下标;
            绑定信息 = 行为.action.bindings[绑定下标];
        }
    }
    /// <summary>
    /// 更新UI文本
    /// </summary>
    private void 更新UI()
    {
        if (行为文字组件 != null)
            if (展示文本 != "")
            {
                行为文字组件.text = 展示文本;
            }
            else
            {
                行为文字组件.text = 行为名称;
            }
        
        if (绑定文本组件 != null)
        {
            if (Application.isPlaying)
            {
                绑定文本组件.text = 输入管理器.获得绑定键(行为名称, 绑定的下标);
            }
            else
                绑定文本组件.text = 行为.action.GetBindingDisplayString(绑定的下标);
        }
    }
    /// <summary>
    /// 点击改键按钮
    /// </summary>
    private void 绑定触发事件()
    {
        输入管理器.改键(行为名称, 绑定的下标, 绑定文本组件, 排除鼠标);
    }
    /// <summary>
    /// 点击重置按钮
    /// </summary>
    private void 重置触发事件()
    {
        输入管理器.重置行为按键(行为名称, 绑定的下标);
        更新UI();
    }
}
