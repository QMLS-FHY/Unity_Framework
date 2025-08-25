using UnityEngine;

namespace UI
{
    /// <summary>
    /// 所有UI界面脚本的通用基类框架
    /// </summary>
    public class UI界面脚本 : UI基类
    {
        /// <summary>
        /// 构造函数，传入界面名和层级类型
        /// </summary>
        public UI界面脚本(string 界面名, UI层级类型 层级)
        {
            this.名称 = 界面名;
            this.层级 = 层级;
        }

        /// <summary>
        /// 初始化UI（可重写，通常用于查找控件、注册事件等）
        /// </summary>
        public override void 初始化()
        {
            base.初始化();
            // 子类可在这里查找控件、注册事件
        }

        /// <summary>
        /// 当界面被激活时的回调（可重写）
        /// </summary>
        public virtual void 当激活()
        {
            // 子类可重写，处理界面显示时的逻辑
        }

        /// <summary>
        /// 当界面被失活时的回调（可重写）
        /// </summary>
        public virtual void 当失活()
        {
            // 子类可重写，处理界面隐藏时的逻辑
        }
    }
} 