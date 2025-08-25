using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace 框架
{
    public abstract class 单例<T> : MonoBehaviour where T : 单例<T>
    {
        private static T 实例对象;
        private static readonly object 锁 = new object();
        protected 单例() { }

        public static T 实例
        {
            get
            {
                if (实例对象 != null) return 实例对象;
                lock (锁)
                {
                    实例对象 = FindObjectOfType<T>();
                    if (实例对象 == null)
                    {
                        Debug.Log($"[单例] {typeof(T).Name} 新创建完成");
                        new GameObject(typeof(T) + "-单例").AddComponent<T>();
                    }
                    return 实例对象;
                }
            }
        }

        private void Awake()
        {
            if (实例对象 == null)
            {
                Debug.Log($"[单例] {typeof(T).Name} 新赋值完成");
                实例对象 = this as T;
                DontDestroyOnLoad(gameObject);
                初始化();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        protected virtual void 初始化()
        {
            // 初始化逻辑
            Debug.Log($"[单例] {typeof(T).Name} 初始化完成");
        }

        private void OnDestroy()
        {
            if (实例对象 == this)
            {
                实例对象 = null;
            }
        }
    }
}
