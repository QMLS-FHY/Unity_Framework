using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using 框架;

namespace 管理器
{
    public class 输入管理器 : 单例<输入管理器>
    {
        public static 输入预设 输入预设实例;

        public static event Action 改绑完成后事件;
        public static event Action 改绑取消后事件;
        public static event Action<InputAction, int> 改绑开始前事件;

        protected override void 初始化()
        {
            base.初始化();
            输入预设实例 = new 输入预设();
        }

        /// <summary>
        /// 开始改键
        /// </summary>
        public static void 改键(string 行为名称, int 绑定下标, TMP_Text 状态文本, bool 排除鼠标)
        {
            InputAction 行为 = 输入预设实例.asset.FindAction(行为名称);
            if (行为 == null || 行为.bindings.Count <= 绑定下标)
            {
                Debug.Log("改键时找不到行为或设备事件！");
                return;
            }

            if (行为.bindings[绑定下标].isComposite)
            {
                var 复合起始下标 = 绑定下标 + 1;
                if (复合起始下标 < 行为.bindings.Count && 行为.bindings[复合起始下标].isComposite)
                    执行改键(行为, 绑定下标, 状态文本, true, 排除鼠标);
            }
            else
                执行改键(行为, 绑定下标, 状态文本, false, 排除鼠标);
        }

        /// <summary>
        /// 进行改键
        /// </summary>
        private static void 执行改键(InputAction 行为, int 绑定下标, TMP_Text 状态文本, bool 复合所有部分, bool 排除鼠标)
        {
            if (行为 == null || 绑定下标 < 0)
                return;

            状态文本.text = $"请按下一个{行为.expectedControlType}";
            行为.Disable();
            var 改绑操作 = 行为.PerformInteractiveRebinding(绑定下标);

            改绑操作.OnComplete(operation =>
            {
                行为.Enable();
                operation.Dispose();

                if (复合所有部分)
                {
                    var 下一个绑定下标 = 绑定下标 + 1;
                    if (下一个绑定下标 < 行为.bindings.Count && 行为.bindings[下一个绑定下标].isComposite)
                        执行改键(行为, 下一个绑定下标, 状态文本, 复合所有部分, 排除鼠标);
                }
                保存行为按键(行为);
                改绑完成后事件?.Invoke();
            });

            改绑操作.OnCancel(operation =>
            {
                行为.Enable();
                operation.Dispose();
                改绑取消后事件?.Invoke();
            });

            改绑操作.WithCancelingThrough("<Keyboard>/escape");
            if (排除鼠标)
                改绑操作.WithControlsExcluding("Mouse");
            改绑开始前事件?.Invoke(行为, 绑定下标);
            改绑操作.Start();
        }

        /// <summary>
        /// 获取绑定键的显示字符串
        /// </summary>
        public static string 获得绑定键(string 行为名称, int 绑定下标)
        {
            if (输入预设实例 == null)
                输入预设实例 = new 输入预设();
            InputAction 行为 = 输入预设实例.asset.FindAction(行为名称);
            return 行为.GetBindingDisplayString(绑定下标);
        }

        /// <summary>
        /// 保存行为按键
        /// </summary>
        private static void 保存行为按键(InputAction 行为)
        {
            for (int i = 0; i < 行为.bindings.Count; i++)
            {
                PlayerPrefs.SetString(行为.actionMap + 行为.name + i, 行为.bindings[i].overridePath);
            }
        }

        /// <summary>
        /// 加载行为按键
        /// </summary>
        public static void 加载行为按键(string 行为名称)
        {
            if (输入预设实例 == null)
                输入预设实例 = new 输入预设();
            InputAction 行为 = 输入预设实例.asset.FindAction(行为名称);
            for (int i = 0; i < 行为.bindings.Count; i++)
            {
                if (!string.IsNullOrEmpty(PlayerPrefs.GetString(行为.actionMap + 行为.name + i)))
                    行为.ApplyBindingOverride(i, PlayerPrefs.GetString(行为.actionMap + 行为.name + i));
            }
        }

        /// <summary>
        /// 重置行为按键
        /// </summary>
        public static void 重置行为按键(string 行为名称, int 绑定下标)
        {
            InputAction 行为 = 输入预设实例.asset.FindAction(行为名称);
            if (行为 == null || 行为.bindings.Count <= 绑定下标)
            {
                Debug.Log("重置时找不到行为或设备事件！");
                return;
            }
            if (行为.bindings[绑定下标].isComposite)
            {
                for (int i = 绑定下标; i < 行为.bindings.Count && 行为.bindings[i].isComposite; i++)
                    行为.RemoveBindingOverride(i);
            }
            else
                行为.RemoveBindingOverride(绑定下标);
            保存行为按键(行为);
        }
    }
}