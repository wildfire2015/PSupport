using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEditor;
using System.IO;
public class TextureReplaceTools:EditorWindow
{
    //[MenuItem("Helper/TextureTools/GetReName")]
    //public static void GetTextureRename()
    //{
        
    //}

    [MenuItem("Helper/TextureTools/ToPng")]
    public static void ChangeToPng()
    {
        Texture2D tex = Selection.activeObject as Texture2D;
        string path = AssetDatabase.GetAssetPath(tex);
        FileInfo info = new FileInfo(path);
        TextureImporter texIm = TextureImporter.GetAtPath(path) as TextureImporter;
        texIm.SetPlatformTextureSettings(BuildTarget.StandaloneWindows.ToString(), 1024, TextureImporterFormat.RGBA32);
        texIm.SetPlatformTextureSettings(BuildTarget.iOS.ToString(), 1024, TextureImporterFormat.RGBA32);
        texIm.SetPlatformTextureSettings(BuildTarget.Android.ToString(), 1024, TextureImporterFormat.RGBA32);
        bool isNomal = texIm.normalmap;
        bool createFromHeight = texIm.convertToNormalmap;
        texIm.isReadable = true;
        texIm.normalmap = false;
        
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Texture2D pngTex = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
        pngTex.SetPixels(tex.GetPixels());
        pngTex.Apply();

        string pngPath = path.Replace(info.Extension, ".png");
        File.WriteAllBytes(pngPath, pngTex.EncodeToPNG());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        texIm = TextureImporter.GetAtPath(pngPath) as TextureImporter;
        texIm.SetPlatformTextureSettings(BuildTarget.StandaloneWindows.ToString(), 1024, TextureImporterFormat.DXT5);
        texIm.SetPlatformTextureSettings(BuildTarget.StandaloneWindows.ToString(), 1024, TextureImporterFormat.PVRTC_RGBA4);
        texIm.SetPlatformTextureSettings(BuildTarget.StandaloneWindows.ToString(), 1024, TextureImporterFormat.ETC2_RGBA8);
        texIm.normalmap = isNomal;
        texIm.convertToNormalmap = createFromHeight;
        AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


    [MenuItem("Helper/TextureTools/全工程转Png")]
    public static void GetMaterialFromTexture()
    {
        TextureReplaceTools toolWindow = EditorWindow.GetWindow(typeof(TextureReplaceTools)) as TextureReplaceTools;
    }

    private Texture2D _mTex = null;
    private Texture2D _mTexPng = null;
    private Vector2 scrollV = Vector2.zero;
    private List<Material> _mlMats = new List<Material>();
    private List<Material> _mlProjMats = new List<Material>();
    private List<TextureStruct> _mlTexs = new List<TextureStruct>();
    private List<TextureStruct> _mlNotMoveTexs = new List<TextureStruct>();
    private List<TextureStruct> _mlMoveTexs = new List<TextureStruct>();
    private string _mStrNotMoveTexs = null;
    private string _mReStorePath = "Assets/WrongNameTexture";

    private List<Material> _mlAllMats = new List<Material>();
    string pngPath = null;
    FileInfo info = null;
    void OnGUI()
    {
        if (GUILayout.Button(("替换选中目录的.tga.tif.jpg等图片")))
        {
            var ob = Selection.assetGUIDs;
            string path = AssetDatabase.GUIDToAssetPath(ob[0]);
            ReplaceToPngTex(path);
        }
        if (GUILayout.Button("替换所有的.tga.tif.jpg图片"))
        {
            ReplaceToPngTex();
        }
        EditorGUILayout.LabelField("未处理的图片", "");
        EditorGUILayout.LabelField("",_mStrNotMoveTexs);
        // 显示修改过的mat
        scrollV = GUILayout.BeginScrollView(scrollV);
        for (int i = 0; i < _mlAllMats.Count; i++)
        {
            EditorGUILayout.ObjectField(_mlAllMats[i], typeof(Material));
        }
        GUILayout.EndScrollView();
    }

    private void ReplaceToPngTex(string pathFilter = "Assets")
    {
        GetProjectMaterial();
        _mStrNotMoveTexs = "";
        _mlTexs = GetWrongExtensionFile(pathFilter);
        // 检查是否存在错误的贴图格式
        if (_mlTexs == null || _mlTexs.Count == 0) return;
        for (int i = 0; i < _mlTexs.Count; i++)
        {
            EditorUtility.DisplayProgressBar("图片替换", _mlTexs[i].mPath, i * 1.0f / _mlTexs.Count);
            if (_mlTexs[i].mPath.Contains("WrongNameTexture")) continue;
            // 1.查看是否有关联的Material
            _mlMats.Clear();
            _mTex = AssetDatabase.LoadAssetAtPath(_mlTexs[i].mPath, typeof(Texture2D)) as Texture2D;
            _mlMats = getMats(_mTex);
            if (_mlMats == null || _mlMats.Count == 0)
            {
                _mlNotMoveTexs.Add(_mlTexs[i]);
                _mStrNotMoveTexs += _mlTexs[i].mShortName + "\n";
                continue;
            }
            _mlMoveTexs.Add(_mlTexs[i]);
            for (int matIdx = 0; matIdx < _mlMats.Count; matIdx++)
            {
                if (_mlAllMats.Contains(_mlMats[matIdx])) continue;
                _mlAllMats.Add(_mlMats[matIdx]);
            }

            // 2.重新生成一张.png图片
            info = new FileInfo(_mlTexs[i].mPath);
            pngPath = _mlTexs[i].mPath.Substring(0, _mlTexs[i].mPath.Length - info.Extension.Length) + ".png";
            ReGeneratePng(_mlTexs[i].mPath, pngPath);

            // 3.重新挂载对应的图片
            _mTexPng = AssetDatabase.LoadAssetAtPath(pngPath, typeof(Texture2D)) as Texture2D;
            for (int matIdx = 0; matIdx < _mlMats.Count; matIdx++)
            {
                var tmpMat = _mlMats[matIdx];
                var count = ShaderUtil.GetPropertyCount(tmpMat.shader);
                for (int proIdx = 0; proIdx < count; proIdx++)
                {
                    if (ShaderUtil.GetPropertyType(tmpMat.shader, proIdx) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        var proName = ShaderUtil.GetPropertyName(tmpMat.shader, proIdx);
                        var tmpTex = tmpMat.GetTexture(proName) as Texture2D;
                        if (null == tmpTex) continue;
                        if (tmpTex == _mTex)
                            tmpMat.SetTexture(proName, _mTexPng);
                    }
                }
            }
        }
        EditorUtility.ClearProgressBar();
        //RemoveAllChangeTexture(_mlTexs, _mlNotMoveTexs);
        RemoveAllChangeTexture(_mlMoveTexs);
    }



    private class TextureStruct
    {
        public string mShortName;
        public string mPath;
    }

    private List<Material> getMats(Texture2D tex)
    {
        List<Material> lMat = new List<Material>();
        Material mat = null;
        int count = 0;
        string proName = null;
        for (int i = 0; i < _mlProjMats.Count; i++)
        {
            mat = _mlProjMats[i];
            count = ShaderUtil.GetPropertyCount(mat.shader);
            for (int proIdx = 0; proIdx < count; proIdx++)
            {
                if (ShaderUtil.GetPropertyType(mat.shader, proIdx) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    proName = ShaderUtil.GetPropertyName(mat.shader, proIdx);
                    var tmpTex = mat.GetTexture(proName) as Texture2D;
                    if (null == tmpTex) continue;
                    if (tmpTex == tex)
                        lMat.Add(mat);
                }
            }
        }
        return lMat;
    }

    private void GetProjectMaterial()
    {
        _mlProjMats.Clear();
        string[] sGuis = AssetDatabase.FindAssets("t:Material");
        string path = null;
        Material mat = null;
        for (int i = 0; i < sGuis.Length; i++)
        {
            path = AssetDatabase.GUIDToAssetPath(sGuis[i]);
            mat = AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
            _mlProjMats.Add(mat);
        }
    }

    private static List<TextureStruct> GetWrongExtensionFile(string pathFilter = "Assets")
    {
        List<TextureStruct> texs = new List<TextureStruct>();
        string[] guids = AssetDatabase.FindAssets("t:Texture2D",new string[] { pathFilter });
        FileInfo info = null;
        string path = null;
        string sExten = null;
        string sShortName = null;
        for (int i = 0; i < guids.Length; i++)
        {
            path = AssetDatabase.GUIDToAssetPath(guids[i]);
            info = new FileInfo(path);
            sExten = info.Extension.ToLower();
            sShortName = info.Name.Substring(0, info.Name.Length - info.Extension.Length);
            if (sExten.Equals(".tga") ||
                sExten.Equals(".tif") ||
                sExten.Equals(".jpg") ||
                sExten.Equals(".psd") ||
                sExten.Equals(".bmp"))
            {
                texs.Add(new TextureStruct() { mShortName = sShortName, mPath = path });
            }
        }
        return texs;
    }

    private void ReGeneratePng(string texPath,string pngPath)
    {
        // change formate
        TextureImporter texIm = TextureImporter.GetAtPath(texPath) as TextureImporter;
        if (texIm == null)
        {
            Debug.Log(texPath);
            return;
        }
        TextureImporterType preType = texIm.textureType;
        bool isNormal = texIm.normalmap;
        bool isFromHeight = texIm.convertToNormalmap;
        texIm.isReadable = true;
        texIm.normalmap = false;
        texIm.SetPlatformTextureSettings(BuildTarget.StandaloneWindows.ToString(), 2048, TextureImporterFormat.RGBA32);
        texIm.SetPlatformTextureSettings(BuildTarget.iOS.ToString(), 2048, TextureImporterFormat.RGBA32);
        texIm.SetPlatformTextureSettings(BuildTarget.Android.ToString(), 2048, TextureImporterFormat.RGBA32);
        AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.SaveAssets();

        // generate png
        Texture2D tex = AssetDatabase.LoadAssetAtPath(texPath,typeof(Texture2D)) as Texture2D;
        Color[] cols = tex.GetPixels();
        Texture2D texPng = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
        texPng.SetPixels(cols);
        texPng.Apply();
        File.WriteAllBytes(pngPath, texPng.EncodeToPNG());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        texIm = TextureImporter.GetAtPath(pngPath) as TextureImporter;
        texIm.isReadable = false;
        if(isNormal)
        {
            texIm.normalmap = isNormal;
            texIm.convertToNormalmap = isFromHeight;
        }
        texIm.textureType = preType;
        texIm.SetPlatformTextureSettings(BuildTarget.StandaloneWindows.ToString(), 2048, TextureImporterFormat.DXT5);
        texIm.SetPlatformTextureSettings(BuildTarget.iOS.ToString(), 2048, TextureImporterFormat.PVRTC_RGBA4);
        texIm.SetPlatformTextureSettings(BuildTarget.Android.ToString(), 2048, TextureImporterFormat.ETC2_RGBA8);
        AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

    }

    private void RemoveAllChangeTexture(List<TextureStruct> allTex,List<TextureStruct> notHandleTex)
    {
        if (!Directory.Exists(_mReStorePath))
            Directory.CreateDirectory(_mReStorePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        for (int allIdx = 0; allIdx < allTex.Count; allIdx++)
        {
            if (notHandleTex.Contains(allTex[allIdx])) continue;
            FileInfo info = new FileInfo(allTex[allIdx].mPath);
            string path = _mReStorePath + "/" + GetRightName(info);
            AssetDatabase.MoveAsset(allTex[allIdx].mPath, path);
        }
    }

    private void RemoveAllChangeTexture(List<TextureStruct> moveTex)
    {
        if (!Directory.Exists(_mReStorePath))
            Directory.CreateDirectory(_mReStorePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        for (int allIdx = 0; allIdx < moveTex.Count; allIdx++)
        {
            FileInfo info = new FileInfo(moveTex[allIdx].mPath);
            string path = _mReStorePath + "/" + GetRightName(info);
            AssetDatabase.MoveAsset(moveTex[allIdx].mPath, path);
        }
    }

    private string GetRightName(FileInfo info)
    {
        string rightN = info.Name;
        if (File.Exists(_mReStorePath + "/" + info.Name))
        {
            int index = 0;
            string rightName = info.Name.Replace(".","_重复名字(" +index+").");
            while (File.Exists(_mReStorePath + "/" + rightName))
            {
                ++index;
                rightName = info.Name.Replace(".", "_重复名字(" + index + ").");
            }
            return rightName;
        }
        return info.Name;
    }
}
