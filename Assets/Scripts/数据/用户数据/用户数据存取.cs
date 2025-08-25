using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System;

public class 用户数据存取
{
    private void Start()
    {
        增加JSON转换器 增加JSON转换器 = new 增加JSON转换器();
        增加JSON转换器.增加所有转换器();
    }

    public static void 保存用户数据(用户数据 用户数据实例)
    {
        string 用户文件夹地址 = Path.Combine(Application.persistentDataPath, "用户");
        Directory.CreateDirectory(用户文件夹地址);
        string JSON文本 = JsonConvert.SerializeObject(用户数据实例);
        string 文件地址 = Path.Combine(用户文件夹地址, $"{用户数据实例.名称}.json");
        File.WriteAllText(文件地址, JSON文本);
    }

    public static 用户数据 加载用户数据(string 名称)
    {
        string 文件地址 = Path.Combine(Application.persistentDataPath, "用户", $"{名称}.json");
        if (File.Exists(文件地址))
        {
            string JSON文本 = File.ReadAllText(文件地址);
            用户数据 用户数据实例 = JsonConvert.DeserializeObject<用户数据>(JSON文本);
            return 用户数据实例;
        }
        else
        {
            return null;
        }
    }

    private class 增加JSON转换器
    {
        public void 增加所有转换器()
        {
            增加Vector3转换器();
        }

        private void 增加Vector3转换器()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = { new Vector3转换器() }
            };
        }
    }

    private class Vector3转换器 : JsonConverter
    {
        public override void WriteJson(JsonWriter JSON写入器, object 对象, JsonSerializer 序列化器)
        {
            var 向量 = (Vector3)对象;
            var JSON对象 = new JObject
            {
                { "X", 向量.x },
                { "Y", 向量.y },
                { "Z", 向量.z }
            };
            JSON对象.WriteTo(JSON写入器);
        }

        public override object ReadJson(JsonReader JSON读取器, Type 目标类型, object 现有值, JsonSerializer 序列化器)
        {
            var JSON对象 = JObject.Load(JSON读取器);
            var X = (float)JSON对象["X"];
            var Y = (float)JSON对象["Y"];
            var Z = (float)JSON对象["Z"];
            return new Vector3(X, Y, Z);
        }

        public override bool CanConvert(Type 目标类型)
        {
            return 目标类型 == typeof(Vector3);
        }
    }
}