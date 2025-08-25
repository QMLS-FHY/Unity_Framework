using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using 框架;
using 工具;

namespace 管理器
{
    public class 资源管理器 : 单例<资源管理器>
    {
        private static Dictionary<string, string> 映射表;
        private static Dictionary<string, object> 资源缓存;

        protected override void 初始化()
        {
            base.初始化();
            string 文件内容 = 文件读取.获取文件内容("资源映射.txt");
            映射表 = new Dictionary<string, string>();
            资源缓存 = new Dictionary<string, object>();
            文件读取.逐行处理(文件内容, 生成映射);
        }

        private void 生成映射(string 行)
        {
            string[] 键值对 = 行.Split('=');
            映射表.Add(键值对[0], 键值对[1]);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        public T 获取<T>(string 资源名) where T : Object
        {
            if (映射表.ContainsKey(资源名))
            {
                string 资源键 = $"{资源名}.{typeof(T)}";
                if (!资源缓存.ContainsKey(资源键))
                {
                    T 资源 = Resources.Load<T>(映射表[资源名]);
                    资源缓存.Add(资源键, 资源);
                }
                return 资源缓存[资源键] as T;
            }
            else return default(T);
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        public void 异步获取<T>(string 资源名, UnityAction<T> 回调委托 = null) where T : Object
        {
            StartCoroutine(异步获取实现<T>(资源名, 回调委托));
        }
        private IEnumerator 异步获取实现<T>(string 资源名, UnityAction<T> 回调委托) where T : Object
        {
            if (映射表.ContainsKey(资源名))
            {
                string 资源键 = $"{资源名}.{typeof(T)}";
                if (!资源缓存.ContainsKey(资源键))
                {
                    ResourceRequest 异步加载 = Resources.LoadAsync<T>(映射表[资源名]);
                    yield return 异步加载;
                    if (!资源缓存.ContainsKey(资源键))
                        资源缓存.Add(资源键, 异步加载.asset as T);
                }
                回调委托?.Invoke(资源缓存[资源键] as T);

            }
            else 回调委托?.Invoke(default(T));
        }
    }
}


