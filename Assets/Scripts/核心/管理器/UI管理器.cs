using System;
using System.Collections.Generic;
using UnityEngine;
using 框架;
using UI;

namespace 管理器
{
    public class UI管理器 : 单例<UI管理器>
    {
        private Dictionary<string, List<string>> 激活界面ID表 = new Dictionary<string, List<string>>();
        private Dictionary<UI层级类型, List<string>> 层级UI堆叠 = new Dictionary<UI层级类型, List<string>>();
        public GameObject UI层;
        public GameObject 顶层;
        public GameObject 上层;
        public GameObject 中层;
        public GameObject 下层;
        public Camera UI摄像机;

        protected override void 初始化()
        {
            base.初始化();
            管理器.对象管理器.实例.对象即将销毁 += 同步移除对象记录;
            管理器.对象管理器.实例.对象即将回收 += 同步移除对象记录;
            foreach (UI层级类型 层级 in Enum.GetValues(typeof(UI层级类型)))
            {
                if (!层级UI堆叠.ContainsKey(层级))
                    层级UI堆叠[层级] = new List<string>();
            }
            初始化UI对象();
        }

        /// <summary>
        /// 回收或销毁对象时同步移除ID，保证激活表和堆叠表准确
        /// </summary>
        private void 同步移除对象记录(池对象 池对象项)
        {
            string 类别 = 池对象项.类别;
            string ID = 池对象项.ID;
            if (激活界面ID表.TryGetValue(类别, out var idList))
            {
                idList.Remove(ID);
                if (idList.Count == 0)
                    激活界面ID表.Remove(类别);
            }
            foreach (var 层级列表 in 层级UI堆叠.Values)
            {
                层级列表.Remove(ID);
            }
        }

        private bool 初始化UI对象()
        {
            var eventSystem = GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                var esObj = new GameObject("EventSystem");
                esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                GameObject.DontDestroyOnLoad(esObj);
            }

            var cameraObj = GameObject.Find("UI摄像机");
            if (cameraObj == null)
            {
                cameraObj = new GameObject("UI摄像机");
                cameraObj.AddComponent<Camera>();
                GameObject.DontDestroyOnLoad(cameraObj);
            }
            UI摄像机 = cameraObj.GetComponent<Camera>();
            UI摄像机.clearFlags = CameraClearFlags.Depth;
            UI摄像机.cullingMask = LayerMask.GetMask("UI");
            UI摄像机.orthographic = true;
            UI摄像机.depth = 100;

            UI层 = GameObject.Find("UI层");
            if (UI层 == null)
            {
                UI层 = new GameObject("UI层");
            }
            Canvas canvas = null;
            canvas = UI层.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = UI层.AddComponent<Canvas>();
                UI层.AddComponent<UnityEngine.UI.CanvasScaler>();
                UI层.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                GameObject.DontDestroyOnLoad(UI层);
            }
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = UI摄像机;

            顶层 = GameObject.Find("顶层");
            上层 = GameObject.Find("上层");
            中层 = GameObject.Find("中层");
            下层 = GameObject.Find("下层");
            if (顶层 == null) { 顶层 = new GameObject("顶层", typeof(RectTransform)); 顶层.transform.SetParent(UI层.transform, false); }
            if (上层 == null) { 上层 = new GameObject("上层", typeof(RectTransform)); 上层.transform.SetParent(UI层.transform, false); }
            if (中层 == null) { 中层 = new GameObject("中层", typeof(RectTransform)); 中层.transform.SetParent(UI层.transform, false); }
            if (下层 == null) { 下层 = new GameObject("下层", typeof(RectTransform)); 下层.transform.SetParent(UI层.transform, false); }

            return true;
        }

        // 激活界面，返回实例ID
        public string 激活界面(string 界面名, string 实例ID = null)
        {
            if (!激活界面ID表.TryGetValue(界面名, out var idList))
            {
                idList = new List<string>();
                激活界面ID表[界面名] = idList;
            }

            // 1. 如果传入了实例ID，优先查找并激活指定对象
            if (!string.IsNullOrEmpty(实例ID))
            {
                var obj = 管理器.对象管理器.实例.获取对象(界面名, 实例ID);
                if (obj != null)
                {
                    var 脚本 = obj.GetComponent<UI.UI基类>();
                    bool 销毁 = !脚本.留存;
                    obj = 管理器.对象管理器.实例.获取对象(界面名, 实例ID, 销毁);
                    脚本 = obj.GetComponent<UI.UI基类>();
                    if (脚本.加载方式 == UI.UI加载方式.异步)
                    {
                        管理器.资源管理器.实例.异步获取<GameObject>(界面名, (预制体) =>
                        {
                            if (预制体 != null)
                            {
                                GameObject 新界面对象 = 管理器.对象管理器.实例.获取对象(界面名, 实例ID, 销毁, 预制体);
                                var 新脚本 = 新界面对象.GetComponent<UI.UI基类>();
                                新界面对象.transform.SetParent(获取根节点(新脚本.层级), false);
                                新界面对象.transform.SetAsLastSibling();
                                新脚本.激活 = true;
                                新脚本.名称 = 实例ID;
                                if (!idList.Contains(实例ID))
                                    idList.Add(实例ID);
                                if (!层级UI堆叠.TryGetValue(新脚本.层级, out var 层级列表))
                                {
                                    层级列表 = new List<string>();
                                    层级UI堆叠[新脚本.层级] = 层级列表;
                                }
                                if (!层级列表.Contains(实例ID))
                                    层级列表.Add(实例ID);
                            }
                        });
                        return 实例ID;
                    }
                    if (!obj.activeInHierarchy)
                    {
                        脚本.激活 = true;
                    }
                    if (!idList.Contains(实例ID))
                        idList.Add(实例ID);
                    if (!层级UI堆叠.TryGetValue(脚本.层级, out var 层级列表))
                    {
                        层级列表 = new List<string>();
                        层级UI堆叠[脚本.层级] = 层级列表;
                    }
                    if (!层级列表.Contains(实例ID))
                        层级列表.Add(实例ID);
                    return 实例ID;
                }
            }
            else
            {
                // 2. 没有传ID，优先复用未激活的实例（性能优化）
                for (int i = 0; i < idList.Count; i++)
                {
                    var id = idList[i];
                    var obj = 管理器.对象管理器.实例.获取对象(界面名, id);
                    if (obj != null && !obj.activeInHierarchy)
                    {
                        var 脚本 = obj.GetComponent<UI.UI基类>();
                        bool 销毁 = !脚本.留存;
                        obj = 管理器.对象管理器.实例.获取对象(界面名, id, 销毁);
                        脚本 = obj.GetComponent<UI.UI基类>();
                        if (脚本.加载方式 == UI.UI加载方式.异步)
                        {
                            管理器.资源管理器.实例.异步获取<GameObject>(界面名, (预制体) =>
                            {
                                if (预制体 != null)
                                {
                                    GameObject 新界面对象 = 管理器.对象管理器.实例.获取对象(界面名, id, 销毁, 预制体);
                                    var 新脚本 = 新界面对象.GetComponent<UI.UI基类>();
                                    新界面对象.transform.SetParent(获取根节点(新脚本.层级), false);
                                    新界面对象.transform.SetAsLastSibling();
                                    新脚本.激活 = true;
                                    新脚本.名称 = id;
                                    if (!层级UI堆叠.TryGetValue(新脚本.层级, out var 层级列表))
                                    {
                                        层级列表 = new List<string>();
                                        层级UI堆叠[新脚本.层级] = 层级列表;
                                    }
                                    if (!层级列表.Contains(id))
                                        层级列表.Add(id);
                                }
                            });
                            return id;
                        }
                        obj.SetActive(true);
                        脚本.激活 = true;
                        if (!层级UI堆叠.TryGetValue(脚本.层级, out var 层级列表))
                        {
                            层级列表 = new List<string>();
                            层级UI堆叠[脚本.层级] = 层级列表;
                        }
                        if (!层级列表.Contains(id))
                            层级列表.Add(id);
                        return id;
                    }
                }
                实例ID = System.Guid.NewGuid().ToString();
            }

            // 判断加载方式
            GameObject 界面对象 = null;
            UI.UI基类 对象脚本 = null;
            bool 异步 = false;
            管理器.资源管理器.实例.异步获取<GameObject>(界面名, (预制体) =>
            {
                if (预制体 != null)
                {
                    bool 销毁 = true;
                    var 脚本预览 = 预制体.GetComponent<UI.UI基类>();
                    if (脚本预览 != null) 销毁 = !脚本预览.留存;
                    界面对象 = 管理器.对象管理器.实例.获取对象(界面名, 实例ID, 销毁, 预制体);
                    对象脚本 = 界面对象.GetComponent<UI.UI基类>();
                    if (对象脚本.加载方式 == UI.UI加载方式.异步)
                    {
                        异步 = true;
                        界面对象.transform.SetParent(获取根节点(对象脚本.层级), false);
                        界面对象.transform.SetAsLastSibling();
                        对象脚本.激活 = true;
                        对象脚本.名称 = 实例ID;
                        idList.Add(实例ID);
                        if (!层级UI堆叠.TryGetValue(对象脚本.层级, out var 新层级列表))
                        {
                            新层级列表 = new List<string>();
                            层级UI堆叠[对象脚本.层级] = 新层级列表;
                        }
                        新层级列表.Add(实例ID);
                    }
                }
            });
            if (!异步)
            {
                界面对象 = 管理器.对象管理器.实例.获取对象(界面名, 实例ID);
                对象脚本 = 界面对象.GetComponent<UI.UI基类>();
                bool 销毁 = !对象脚本.留存;
                界面对象 = 管理器.对象管理器.实例.获取对象(界面名, 实例ID, 销毁);
                对象脚本 = 界面对象.GetComponent<UI.UI基类>();
                界面对象.transform.SetParent(获取根节点(对象脚本.层级), false);
                界面对象.transform.SetAsLastSibling();
                对象脚本.激活 = true;
                对象脚本.名称 = 实例ID;
                idList.Add(实例ID);
                if (!层级UI堆叠.TryGetValue(对象脚本.层级, out var 新层级列表))
                {
                    新层级列表 = new List<string>();
                    层级UI堆叠[对象脚本.层级] = 新层级列表;
                }
                新层级列表.Add(实例ID);
            }
            return 实例ID;
        }

        // 获取指定实例的UI对象
        public UI基类 获取界面(string 界面名, string 实例ID)
        {
            var obj = 管理器.对象管理器.实例.获取对象(界面名, 实例ID);
            return obj != null ? obj.GetComponent<UI基类>() : null;
        }

        // 获取同一类型所有实例（只返回池中真实存在的对象）
        public List<UI基类> 获取所有实例(string 界面名)
        {
            var result = new List<UI基类>();
            if (激活界面ID表.TryGetValue(界面名, out var idList))
            {
                foreach (var id in idList)
                {
                    if (管理器.对象管理器.实例.是否存在(类别: 界面名, ID: id))
                    {
                        var obj = 管理器.对象管理器.实例.获取对象(界面名, id);
                        if (obj != null)
                        {
                            var 脚本 = obj.GetComponent<UI基类>();
                            if (脚本 != null)
                                result.Add(脚本);
                        }
                    }
                }
            }
            return result;
        }

        // 关闭指定实例的UI对象，兼容留存属性
        public void 关闭界面(string 界面名, string 实例ID)
        {
            var obj = 管理器.对象管理器.实例.获取对象(界面名, 实例ID);
            if (obj != null)
            {
                var 脚本 = obj.GetComponent<UI.UI基类>();
                脚本.激活 = false;
                // 无论留存与否都回收对象，是否自动清理交由对象池管理
                管理器.对象管理器.实例.回收对象(对象: obj, 延迟: 0);
                // 不再手动移除ID，交由事件自动同步
            }
        }

        // 兼容原有API：获取第一个实例
        public UI基类 获取界面(string 界面名)
        {
            if (激活界面ID表.TryGetValue(界面名, out var idList) && idList.Count > 0)
            {
                foreach (var id in idList)
                {
                    if (管理器.对象管理器.实例.是否存在(类别: 界面名, ID: id))
                    {
                        var obj = 管理器.对象管理器.实例.获取对象(界面名, id);
                        if (obj != null)
                            return obj.GetComponent<UI基类>();
                    }
                }
            }
            return null;
        }

        public void 关闭最上层UI(UI层级类型 层级)
        {
            if (层级UI堆叠.TryGetValue(层级, out var 层级列表) && 层级列表.Count > 0)
            {
                string 最上层ID = 层级列表[层级列表.Count - 1];
                foreach (var 项 in 激活界面ID表)
                {
                    if (项.Value.Contains(最上层ID))
                    {
                        关闭界面(项.Key, 最上层ID);
                        break;
                    }
                }
            }
        }

        public void 关闭所有界面()
        {
            var all = new List<(string 界面名, string id)>();
            foreach (var kv in 激活界面ID表)
            {
                foreach (var id in kv.Value)
                {
                    all.Add((kv.Key, id));
                }
            }
            foreach (var (界面名, id) in all)
            {
                关闭界面(界面名, id);
            }
        }

        public Transform 获取根节点(UI层级类型 层级)
        {
            switch (层级)
            {
                case UI层级类型.顶层:
                    return 顶层.transform;
                case UI层级类型.上层:
                    return 上层.transform;
                case UI层级类型.中层:
                    return 中层.transform;
                case UI层级类型.下层:
                    return 下层.transform;
                default:
                    return 中层.transform;
            }
        }

        // 获取某层级最上层UI实例
        public UI基类 获取层级最上层UI(UI层级类型 层级)
        {
            if (层级UI堆叠.TryGetValue(层级, out var 层级列表) && 层级列表.Count > 0)
            {
                for (int i = 层级列表.Count - 1; i >= 0; i--)
                {
                    var id = 层级列表[i];
                    if (管理器.对象管理器.实例.是否存在(ID: id))
                    {
                        var obj = 管理器.对象管理器.实例.获取对象(null, id);
                        if (obj != null)
                            return obj.GetComponent<UI基类>();
                    }
                }
            }
            return null;
        }
    }
}
