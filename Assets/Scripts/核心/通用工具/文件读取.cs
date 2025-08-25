using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System;
namespace 工具
{
    ///<summary>
    ///负责读取配置文件并提供解析行
    ///读取StreamingAssets文件夹下传进来的fileName文件
    ///<summary>
    public class 文件读取
    {
        public static string 获取文件内容(string 文件名)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            string url = "file://" + Application.dataPath + "/StreamingAssets/" + 文件名;
#elif UNITY_IPHONE
            string url = "file://" + Application.dataPath + "/Raw/" + 文件名;
#elif UNITY_ANDROID
            string url = "jar:file://" + Application.dataPath + "!/assets/" + 文件名;
#endif
            UnityWebRequest GET请求 = UnityWebRequest.Get(url);
            GET请求.SendWebRequest();
            while (true)
            {
                if (GET请求.downloadHandler.isDone)
                    return GET请求.downloadHandler.text;
            }
        }

        public static void 逐行处理(string 内容, Action<string> 处理方法)
        {
            using (StringReader 文本流 = new StringReader(内容))
            {
                string 行;
                while ((行 = 文本流.ReadLine()) != null) 
                {
                    处理方法(行);
                }
            }
        }
    }
}