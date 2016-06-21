using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class StripeTextureAlpha
{
    public static string pathOut = "Assets/ArtRes/AlphaETC1";
    /// <summary>
    /// 从图片中剔除Alpha通道
    /// </summary>
    /// <param name="tex"></param>
    public static void StripAlpha(Texture2D tex,string assetPath)
    {
        if (!assetPath.Contains("#A")) return;   /*不带alpha*/
        if (assetPath.Contains("Assets/ArtRes/ui")) return;     /*如果为UI*/
        string path = assetPath;
        
        FileInfo info = new FileInfo(path);
        string fileName = info.Name.Substring(0, info.Name.Length - info.Extension.Length);

        if (!Directory.Exists(pathOut))
            Directory.CreateDirectory(pathOut);
        
        Texture2D alphaTex = new Texture2D(tex.width, tex.height, TextureFormat.RGB24, false);
        Texture2D rgbTex = new Texture2D(tex.width, tex.height, TextureFormat.RGB24, false);

        Color[] pixs = tex.GetPixels();
        rgbTex.SetPixels(pixs);
        for (int i = 0; i < pixs.Length; i++)
        {
            pixs[i].r = pixs[i].g = pixs[i].b = pixs[i].a;
        }
        alphaTex.SetPixels(pixs);
        alphaTex.Apply();
        rgbTex.Apply();

        string alphaOutPath = GetAlphaPath(fileName);
        string rgbOutPath = GetRgbPath(fileName);
        File.WriteAllBytes(alphaOutPath, alphaTex.EncodeToPNG());
        File.WriteAllBytes(rgbOutPath, rgbTex.EncodeToPNG());
        AssetDatabase.SaveAssets();
        // 这里不能够refresh
        // 如果refresh了会出现递归的调用
        // 并且会导致Unity内部crash
        //AssetDatabase.Refresh();
    }
    /// <summary>
    /// 获取Alpha图片的位置
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string GetAlphaPath(string fileName)
    {
        string tmp = pathOut + "/" + fileName + "_Alpha.png";
        return tmp.Replace("#A","");
        
    }
    /// <summary>
    /// 获取RGB图片的位置
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string GetRgbPath(string fileName)
    {
        string tmp = pathOut + "/" + fileName + "_Rgb.png";
        return tmp.Replace("#A", "");
    }
}
