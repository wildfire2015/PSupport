using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PSupport;

public class ConvertUTF8 
{
    //[MenuItem("ConvertText/ConvertUTF8")]
    public static void ConvertTextToUTF8()
    {
        string[] assetpaths = AssetDatabase.GetAllAssetPaths();
        
        foreach (string path in assetpaths)
        {
            if(Path.GetExtension(path) == ".csv")
            {
                StreamReader sr = new StreamReader(Path.GetFullPath(path), Encoding.Default);
                string contex = sr.ReadToEnd();
                sr.Close();
                UTF8Encoding utf8 = new UTF8Encoding(true);
                StreamWriter sw = new StreamWriter(Path.GetFullPath(path), false, utf8);
                sw.Write(contex);
                sw.Flush();
                sw.Close();
                Debug.Log("====convert===" + path + "====successful!");

            }
        }
       
       
    }
}
