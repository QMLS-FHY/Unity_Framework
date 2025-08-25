using System.Collections.Generic;
using System.IO;
using UnityEditor;
public class 生成映射文件 : Editor
{
    private static string 文件地址 = "Assets/StreamingAssets/资源映射.txt";
    [MenuItem("自定义/文件/重新生成资源映射文件")]
    public static void 重新生成资源映射文件()
    {
        Dictionary<string, List<string>> 文件拓展名字典 = new Dictionary<string, List<string>>();
        File.Delete(文件地址);
        文件拓展名字典.Add("prefab", new List<string>() { "prefab" });
        文件拓展名字典.Add("audioclip", new List<string>() { "mp3", "mp4" });
        文件拓展名字典.Add("texture", new List<string>() { "png", "jpg", "bmp" });
        if (!Directory.Exists("Assets/StreamingAssets"))
        {
            Directory.CreateDirectory("Assets/StreamingAssets");
        }
        foreach (var 项 in 文件拓展名字典)
        {
            string[] 映射组 = 获取单类资源映射(项.Key, 项.Value);
            File.AppendAllLines(文件地址, 映射组);
        }
        AssetDatabase.Refresh();
    }

    private static string[] 获取单类资源映射(string type, List<string> 后缀组)
    {
        string[] GUID表 = AssetDatabase.FindAssets($"t:{type}", new string[] { "Assets/Resources" });
        for (int i = 0; i < GUID表.Length; i++)
        {
            GUID表[i] = AssetDatabase.GUIDToAssetPath(GUID表[i]);
            string 文件名 = Path.GetFileNameWithoutExtension(GUID表[i]);
            string 文件地址 = GUID表[i].Replace("Assets/Resources/", string.Empty);
            foreach (string 后缀 in 后缀组)
            {
                文件地址 = 文件地址.Replace("." + 后缀, string.Empty);
            }
            GUID表[i] = 文件名 + "=" + 文件地址;
        }
        return GUID表;
    }
}