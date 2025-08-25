using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using 框架;
using System;

public interface 池对象接口
{
    void 初始化();
}

public class 池对象
{
    public string ID;
    public string 类别;
    public GameObject 对象;
    public float 最后回收时间;
    public 池对象(GameObject 对象, string 类别, string id = null, bool 销毁 = true)
    {
        this.对象 = 对象;
        this.类别 = 类别;
        ID = string.IsNullOrEmpty(id) ? System.Guid.NewGuid().ToString() : id;
        最后回收时间 = 销毁 ? Time.time : 0f;
    }
}

namespace 管理器
{
    /// <summary>
    /// 对象管理器，支持对象池多实例、ID管理、事件通知等。新增：对象回收通知事件、对象存在性检查方法。
    /// </summary>
    public class 对象管理器 : 单例<对象管理器>
    {
        private Dictionary<string, List<池对象>> 对象池字典;
        private Dictionary<GameObject, 池对象> 对象查找表;
        private Dictionary<string, 池对象> ID查找表;
        public float 最大保留时间 = 30f;
        public float 自动清理间隔 = 5f;
        private Coroutine 自动清理协程;
        public event Action<池对象> 对象即将销毁;
        public event Action<池对象> 对象即将回收;

        protected override void 初始化()
        {
            base.初始化();
            对象池字典 = new Dictionary<string, List<池对象>>();
            对象查找表 = new Dictionary<GameObject, 池对象>();
            ID查找表 = new Dictionary<string, 池对象>();
            if (自动清理协程 == null)
                自动清理协程 = StartCoroutine(自动清理超时对象());
        }

        /// <summary>
        /// 获取对象（不指定销毁参数时，复用对象保持原状态，新对象默认自动清理）
        /// </summary>
        public GameObject 获取对象(string 类别, string ID = null, GameObject 预制件 = null)
        {
            return 获取对象(类别, ID, null, 预制件);
        }

        /// <summary>
        /// 获取对象：可通过ID精确获取，也可复用未激活对象，无则新建。可指定是否自动清理。
        /// 销毁为null时，复用对象保持原状态，新对象默认自动清理。
        /// </summary>
        public GameObject 获取对象(string 类别, string ID, bool? 销毁, GameObject 预制件 = null)
        {
            if (!对象池字典.ContainsKey(类别))
                对象池字典[类别] = new List<池对象>();
            var 池对象列表 = 对象池字典[类别];

            // 1. 传入ID，直接查找并初始化
            if (!string.IsNullOrEmpty(ID))
            {
                var 指定对象 = 池对象列表.Find(池对象项 => 池对象项.ID == ID);
                if (指定对象 != null)
                {
                    // 只有明确传入销毁参数时才修改最后回收时间，否则保持原状态
                    if (销毁 != null)
                        指定对象.最后回收时间 = 销毁.Value ? Time.time : 0f;
                    初始化对象(指定对象);
                    return 指定对象.对象;
                }
                // 没有找到指定ID，直接新建一个ID为传入值的对象
                if (预制件 == null)
                {
                    预制件 = 资源管理器.实例.获取<GameObject>(类别);
                    if (预制件 == null)
                    {
                        Debug.LogWarning($"获取对象失败：类别[{类别}]没有可用对象，且资源管理器也找不到对应预制体！");
                        return null;
                    }
                }
                // 新建对象，销毁为null时默认自动清理
                var 新ID池对象 = 创建对象(类别, 预制件, ID, 销毁 ?? true);
                初始化对象(新ID池对象);
                return 新ID池对象.对象;
            }
            // 2. 没有ID，查找未激活对象
            var 未激活对象 = 池对象列表.Find(池对象项 => !池对象项.对象.activeInHierarchy);
            if (未激活对象 != null)
            {
                // 只有明确传入销毁参数时才修改最后回收时间，否则保持原状态
                if (销毁 != null)
                    未激活对象.最后回收时间 = 销毁.Value ? Time.time : 0f;
                初始化对象(未激活对象);
                return 未激活对象.对象;
            }
            // 3. 没有未激活对象，新建
            if (预制件 == null)
            {
                预制件 = 资源管理器.实例.获取<GameObject>(类别);
                if (预制件 == null)
                {
                    Debug.LogWarning($"获取对象失败：类别[{类别}]没有可用对象，且资源管理器也找不到对应预制体！");
                    return null;
                }
            }
            // 新建对象，销毁为null时默认自动清理
            var 新池对象 = 创建对象(类别, 预制件, ID, 销毁 ?? true);
            初始化对象(新池对象);
            return 新池对象.对象;
        }

        /// <summary>
        /// 创建对象（实例化并注册到池）
        /// </summary>
        private 池对象 创建对象(string 类别, GameObject 预制件, string ID, bool 销毁)
        {
            var 新对象 = Instantiate(预制件);
            var 新池对象 = new 池对象(新对象, 类别, ID, 销毁);
            对象池字典[类别].Add(新池对象);
            对象查找表[新对象] = 新池对象;
            ID查找表[新池对象.ID] = 新池对象;
            return 新池对象;
        }

        /// <summary>
        /// 初始化对象（只激活并调用池对象接口）
        /// </summary>
        private void 初始化对象(池对象 信息)
        {
            信息.对象.SetActive(true);
            foreach (var 池对象接口 in 信息.对象.GetComponents<池对象接口>())
                池对象接口.初始化();
        }

        /// <summary>
        /// 检查对象是否存在（可通过类别、ID、对象）
        /// </summary>
        public bool 是否存在(string 类别 = null, string ID = null, GameObject 对象 = null)
        {
            // 通过类别、ID、对象三种方式判断对象是否在池中
            if (!string.IsNullOrEmpty(类别))
            {
                if (对象池字典.TryGetValue(类别, out var 池对象列表))
                {
                    if (!string.IsNullOrEmpty(ID))
                        return 池对象列表.Exists(池对象项 => 池对象项.ID == ID);
                    if (对象 != null)
                        return 池对象列表.Exists(池对象项 => 池对象项.对象 == 对象);
                    return 池对象列表.Count > 0;
                }
                return false;
            }
            if (!string.IsNullOrEmpty(ID))
                return ID查找表.ContainsKey(ID);
            if (对象 != null)
                return 对象查找表.ContainsKey(对象);
            return false;
        }

        /// <summary>
        /// 回收对象（可通过类别、对象或ID）
        /// </summary>
        public void 回收对象(string 类别 = null, GameObject 对象 = null, string ID = null, float 延迟 = 0f)
        {
            if (!string.IsNullOrEmpty(类别))
            {
                if (对象池字典.TryGetValue(类别, out var 池对象列表))
                {
                    if (!string.IsNullOrEmpty(ID))
                    {
                        var 指定对象 = 池对象列表.Find(池对象项 => 池对象项.ID == ID);
                        if (指定对象 != null)
                            StartCoroutine(延迟回收(指定对象.对象, 延迟));
                    }
                    else if(对象 != null)
                    {
                        var 指定对象 = 池对象列表.Find(池对象项 => 池对象项.对象 == 对象);
                        if (指定对象 != null)
                            StartCoroutine(延迟回收(指定对象.对象, 延迟));
                    }
                    else
                    {
                        foreach (var 池对象项 in 池对象列表)
                            StartCoroutine(延迟回收(池对象项.对象, 延迟));
                    }
                }
            }
            else if (对象 != null)
            {
                if (对象查找表.TryGetValue(对象, out var 池对象信息))
                    StartCoroutine(延迟回收(池对象信息.对象, 延迟));
            }
            else if(!string.IsNullOrEmpty(ID))
            {
                if (ID查找表.TryGetValue(ID, out var 池对象信息))
                    StartCoroutine(延迟回收(池对象信息.对象, 延迟));
            }
        }

        /// <summary>
        /// 延迟回收--协程（回收时会触发对象即将回收事件）
        /// </summary>
        private IEnumerator 延迟回收(GameObject 对象, float 延迟)
        {
            yield return new WaitForSeconds(延迟);
            对象.SetActive(false);
            if (对象查找表.TryGetValue(对象, out 池对象 信息))
            {
                对象即将回收?.Invoke(信息);
                if(信息.最后回收时间 != 0f)
                    信息.最后回收时间 = Time.time;
            }
        }

        /// <summary>
        /// 销毁对象（可通过类别、对象或ID）
        /// </summary>
        public void 销毁对象(string 类别 = null, GameObject 对象 = null, string ID = null)
        {
            if (!string.IsNullOrEmpty(类别))
            {
                if (对象池字典.TryGetValue(类别, out var 池对象列表))
                {
                    if (!string.IsNullOrEmpty(ID))
                    {
                        if (ID查找表.TryGetValue(ID, out var 池对象项))
                        {
                            销毁池对象(池对象项);
                        }
                    }
                    else if (对象 != null)
                    {
                        if (对象查找表.TryGetValue(对象, out var 池对象项))
                        {
                            销毁池对象(池对象项);
                        }
                    }
                    else
                    {
                        foreach (var 池对象项 in new List<池对象>(池对象列表))
                        {
                            销毁池对象(池对象项);
                        }
                    }
                }
            }
            else if (对象 != null)
            {
                if (对象查找表.TryGetValue(对象, out var 池对象信息))
                {
                    销毁池对象(池对象信息);
                }
            }
            else if (!string.IsNullOrEmpty(ID))
            {
                if (ID查找表.TryGetValue(ID, out var 池对象信息))
                {
                    销毁池对象(池对象信息);
                }
            }
        }

        /// <summary>
        /// 清除指定类别的对象
        /// </summary>
        public void 清除(string 类别)
        {
            if (对象池字典.ContainsKey(类别))
            {
                foreach (池对象 池对象项 in new List<池对象>(对象池字典[类别]))
                {
                    销毁池对象(池对象项);
                }
                对象池字典.Remove(类别);
            }
        }

        /// <summary>
        /// 清除对象池中所有内容
        /// </summary>
        public void 清除所有()
        {
            foreach (var 类别 in new List<string>(对象池字典.Keys))
            {
                清除(类别);
            }
        }

        /// <summary>
        /// 销毁单个池对象（统一销毁流程）
        /// </summary>
        private void 销毁池对象(池对象 池对象项)
        {
            if (池对象项 == null) return;
            对象即将销毁?.Invoke(池对象项);
            GameObject.Destroy(池对象项.对象);
            对象查找表.Remove(池对象项.对象);
            ID查找表.Remove(池对象项.ID);
            if (对象池字典.TryGetValue(池对象项.类别, out var 池对象列表))
                池对象列表.Remove(池对象项);
        }

        /// <summary>
        /// 自动清理超时未被使用的对象
        /// </summary>
        private IEnumerator 自动清理超时对象()
        {
            while (true)
            {
                float 当前时间 = Time.time;
                foreach (var 类别_池对象列表 in 对象池字典)
                {
                    var 池对象列表 = 类别_池对象列表.Value;
                    for (int 索引 = 池对象列表.Count - 1; 索引 >= 0; 索引--)
                    {
                        var 池对象项 = 池对象列表[索引];
                        if (!池对象项.对象.activeInHierarchy && 池对象项.最后回收时间 > 0 && 当前时间 - 池对象项.最后回收时间 > 最大保留时间)
                        {
                            销毁池对象(池对象项);
                        }
                    }
                }
                yield return new WaitForSeconds(自动清理间隔);
            }
        }
    }
}