using UnityEngine;
using System;
using Unity.VisualScripting;

namespace UI
{
    public enum UI层级类型 { 顶层, 上层, 中层, 下层 }
    public enum UI加载方式 { 同步, 异步 }

    public abstract class UI基类 : MonoBehaviour
    {
        public string 名称;
        public GameObject 对象;
        public bool 留存 = false;
        private bool _激活;
        public bool 激活
        {
            get => _激活;
            set
            {
                _激活 = value;
                gameObject.SetActive(value);
            }
        }
        public UI层级类型 层级 = UI层级类型.中层;
        public UI加载方式 加载方式 = UI加载方式.同步;
        public virtual void 初始化()
        {
            对象 = this.gameObject;
        }

        protected void 设置父节点(Transform 父节点)
        {
            对象.transform.SetParent(父节点);
        }
    }
}