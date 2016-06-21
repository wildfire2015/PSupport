using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class TextureTool
{
    [MenuItem("TextureTool/ToPng")]
    public static void ChangeToPng()
    {
        Texture2D tgaTex = Selection.activeObject as Texture2D;
        string path = AssetDatabase.GetAssetPath(tgaTex);
        TextureImporter texIm = TextureImporter.GetAtPath(path) as TextureImporter;
        texIm.textureFormat = TextureImporterFormat.RGBA32;
        texIm.normalmap = false;
        texIm.isReadable = true;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        Texture2D pngTex = new Texture2D(tgaTex.width, tgaTex.height, TextureFormat.RGBA32, false);
        pngTex.SetPixels(tgaTex.GetPixels());
        pngTex.Apply();
       
        FileInfo file = new FileInfo(path);
        string newP = path.Replace(file.Extension, ".png");
        File.WriteAllBytes(newP, pngTex.EncodeToPNG());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
