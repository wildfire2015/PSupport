using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using PSupport;
/*******************************************************************************
* 
*             类名: ExportAssetBundle
*             功能: unity5打包插件,自动命名assetbundle 的 Name,支持共享包的文件结构和命名规则,对脚本打包做特殊处理
*             作者: 彭谦
*             日期: 2015.12.7
*             修改:
*             备注:Resources/assetbundle文件夹下的目录制作assetbundle,并且包含依赖资源,如果源资源文件夹含有#share字段的名字,表示是共享文件夹,则单独打包
*                  包名由预制体所在的文件夹觉得,如果是共享资源,则是共享资源的文件夹名字作为报名(不包含#share)
*             
* *****************************************************************************/
public class ExportAssetBundle
{
    public static string pathlocal = "Assets/StreamingAssets";
    public static string pathURL = "Assets/StreamingAssetsURL";
    private static Dictionary<string, List<string>> _mDicUsedBundleNames = new Dictionary<string, List<string>>();
    [MenuItem("Build/BuildAssetBundles/Local/BuildForPC")]
    public static void BuildAssetBundlesForPC()
    {
        UpDateScriptDllProj.BulidScriptDll();
        SetAllAssetBunldesNames();
        BuildAssetBundlesForPlatform(BuildTarget.StandaloneWindows64, pathlocal);
    }
    [MenuItem("Build/BuildAssetBundles/Local/BuildForIOS")]
    public static void BuildAssetBundlesForIOS()
    {
        UpDateScriptDllProj.BulidScriptDll();
        SetAllAssetBunldesNames();
        BuildAssetBundlesForPlatform(BuildTarget.iOS, pathlocal);
    }
    [MenuItem("Build/BuildAssetBundles/Local/BuildForAndriod")]
    public static void BuildAssetBundlesForAndriod()
    {
        UpDateScriptDllProj.BulidScriptDll();
        SetAllAssetBunldesNames();
        BuildAssetBundlesForPlatform(BuildTarget.Android, pathlocal);
    }
    [MenuItem("Build/BuildAssetBundles/URL/BuildForPC")]
    public static void BuildAssetBundlesURLForPC()
    {
        UpDateScriptDllProj.BulidScriptDll();
        SetAllAssetBunldesNames();
        BuildAssetBundlesForPlatform(BuildTarget.StandaloneWindows64,pathURL);
    }
    [MenuItem("Build/BuildAssetBundles/URL/BuildForIOS")]
    public static void BuildAssetBundlesURLForIOS()
    {
        UpDateScriptDllProj.BulidScriptDll();
        SetAllAssetBunldesNames();
        BuildAssetBundlesForPlatform(BuildTarget.iOS, pathURL);
    }
    [MenuItem("Build/BuildAssetBundles/URL/BuildForAndriod")]
    public static void BuildAssetBundlesURLForAndriod()
    {
        UpDateScriptDllProj.BulidScriptDll();
        SetAllAssetBunldesNames();
        BuildAssetBundlesForPlatform(BuildTarget.Android, pathURL);
    }
    [MenuItem("Build/RemoveAllAssetBundleNames")]
    public static void ClearAllAssetBundlesNames()
    {
        //清空所有的assetbundlename
        string[] assetbundlenames = AssetDatabase.GetAllAssetBundleNames();
        foreach (string path in assetbundlenames)
        {
            AssetDatabase.RemoveAssetBundleName(path, true);
        }
        AssetDatabase.RemoveUnusedAssetBundleNames();
    }

    public static void SetAllAssetBunldesNames()
    {
        //需要打包的预制体所在的文件夹
        string prefabpath = "Assets/Resources/assetsbundles";
        //清空用过的bundle名字
        _mDicUsedBundleNames.Clear();

        //重新设定资源的assetbundlename
        string[] allassetpaths = AssetDatabase.GetAllAssetPaths();

        var listneedtoassetbundlepaths =
            from path in allassetpaths//包含在prefabpath中，并且不是文件夹，并且所在文件夹没有子文件夹的文件
            where path.Contains(prefabpath) && !Directory.Exists(path) && Directory.GetDirectories(Path.GetDirectoryName(path)).Length == 0
            group path by path.Contains("/level/") ? path : Path.GetDirectoryName(path);


        foreach (var assetpaths in listneedtoassetbundlepaths)
        {
            string[] assetdepadencepaths;

            //设置依赖资源的assetBundleName
            assetdepadencepaths = AssetDatabase.GetDependencies(assetpaths.ToArray());

            foreach (string assetpath in assetdepadencepaths)
            {
                SetAssetBunldesNames(assetpaths.Key, assetpath);
            }
        }
        //如果不在记录的bundlenames里面,则清空这些bundlenames
        string[] allassetbundlenames = AssetDatabase.GetAllAssetBundleNames();
        foreach (string bundlename in allassetbundlenames)
        {
            if(!_mDicUsedBundleNames.Keys.Contains(bundlename))
            {
                AssetDatabase.RemoveAssetBundleName(bundlename, true);
            }
        }
        AssetDatabase.RemoveUnusedAssetBundleNames();
        foreach (string usedbundlename in _mDicUsedBundleNames.Keys)
        {
            string[] assetspaths = AssetDatabase.GetAssetPathsFromAssetBundle(usedbundlename);
            foreach (string assetpath in assetspaths)
            {
                if(!_mDicUsedBundleNames[usedbundlename].Contains(assetpath))
                {
                    AssetImporter assetip = AssetImporter.GetAtPath(assetpath);
                    assetip.assetBundleName = "";
                }
            }
        }
    }
        

    public static void SetAssetBunldesNames(string assetpath, string refAssetpath) 
    {
        if (refAssetpath.Contains("Plugins"))
        {//引用plugins的文件不做标记
            AssetImporter asp = AssetImporter.GetAtPath(refAssetpath);
            asp.assetBundleName = null;
            return;
        }
        //共享资源的标志
        string sharetag = "#share";

        AssetImporter assetip = AssetImporter.GetAtPath(refAssetpath);
        string bundlename = "";
        if (assetip.GetType() != typeof(MonoImporter))
        {
            
            if (refAssetpath.Contains(sharetag))
            {//如果是需要独立的共享资源,或者是需要动态加载的资源

                //如果文件夹路径中重复出现#share#，发出警告
                if (Regex.Matches(refAssetpath, sharetag).Count > 1)
                {
                    Debug.LogError(refAssetpath + "---- 包含 #share# 大于 1 个，打包失败！");
                    return;
                }
               
                bundlename = "sharedbundles/" + Path.GetFileNameWithoutExtension(Regex.Split(refAssetpath, sharetag)[0]);
                assetip.assetBundleName = bundlename;
                
            }
            else
            {
                if (!assetpath.Contains("/level/") || refAssetpath == assetpath)
                {//如果不是场景的依赖资源,才标记
                    //如果不是需要独立的共享资源
                    bundlename = assetpath.Split('.')[0];
                    bundlename = bundlename.Substring("Assets/Resources/".Length);
                    assetip.assetBundleName = bundlename;
                    
                }
            }
            if (Regex.IsMatch(bundlename, "[A-Z]"))
            {
                Debug.LogError(refAssetpath + "包含在" + bundlename + "中===bundle名字中含有大写字母,该bundle不会被打包");
                assetip.assetBundleName = "";
                bundlename = "";
            }
            if(bundlename != "")
            {
                if(!_mDicUsedBundleNames.Keys.Contains(bundlename))
                {
                    _mDicUsedBundleNames.Add(bundlename,new List<string>());
                }
                _mDicUsedBundleNames[bundlename].Add(refAssetpath);
            }
            
        }
    }



    public static void BuildAssetBundlesForPlatform(BuildTarget targetplatform,string outputpath)
    {
        
        EditorUserBuildSettings.SwitchActiveBuildTarget(targetplatform);

        string[] allassetpaths = AssetDatabase.GetAllAssetPaths();

        ToolFunctions.CheckAndCreateFolder(outputpath);

        
        try
        {
            BuildPipeline.BuildAssetBundles(outputpath, BuildAssetBundleOptions.DeterministicAssetBundle, targetplatform);
        }
        catch
        {
            ToolFunctions.DeleteFolder(Application.dataPath + "/Resources/assetsbundles/scriptdll");

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            Debug.Log(outputpath + "--bundles 生成出错!");
        }
        ToolFunctions.DeleteFolder(Application.dataPath + "/Resources/assetsbundles/scriptdll");


        //删除无用的的assetbundle 

        //收集有用的assetbundle
        string[] assetbundlenames = AssetDatabase.GetAllAssetBundleNames();
        List<string> listusedfiles = new List<string>();
        listusedfiles.AddRange(assetbundlenames);
        //收集有用的ab的manifest
        for (int i = 0; i < assetbundlenames.Length; i++)
        {
            listusedfiles.Add(assetbundlenames[i] + ".manifest");
        }
        //收集总的ab和其manifest文件
        listusedfiles.Add(Path.GetFileNameWithoutExtension(outputpath));
        listusedfiles.Add(Path.GetFileNameWithoutExtension(outputpath) + ".manifest");


        string streamingassetpath = outputpath + "/";
        //获取所有的StreamingAsset下无用的文件
        var listUnUsedAssetFile =
            from filepath in allassetpaths
            where filepath.Contains(streamingassetpath)
                    && !listusedfiles.Contains(filepath.Substring(streamingassetpath.Length))
                    && !Directory.Exists(filepath)
            select filepath;
        //删除无用的文件
        foreach (string unusedpath in listUnUsedAssetFile)
        {
            AssetDatabase.DeleteAsset(unusedpath);
        }
        //删除无用的文件夹
        var listUnUsedAssetDir =
            from dirpath in allassetpaths
            where dirpath.Contains(streamingassetpath)
                    && Directory.Exists(dirpath)
                    && (Directory.GetFiles(dirpath).Length == 0 || Directory.GetFiles(dirpath).Length == 1)//空或者只包含一个meta文件
            select dirpath;
        foreach (string unuseddir in listUnUsedAssetDir)
        {
            AssetDatabase.DeleteAsset(unuseddir);
        }


        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        Debug.Log(outputpath + "--bundles 生成成功!");

    }
    [MenuItem("Build/检查共享资源")]

    public static void ShowShareAssetsInBundles()
    {
        string[] allAssets = AssetDatabase.GetAllAssetPaths();
        string outputpath = "Assets/StreamingAssets/StreamingAssets";
        AssetBundle manifestBundle = AssetBundle.LoadFromFile(Application.dataPath + "/StreamingAssets/StreamingAssets");
        AssetBundleManifest manifest = (AssetBundleManifest)manifestBundle.LoadAsset("AssetBundleManifest");
        string[] bundlespaths =  manifest.GetAllAssetBundles();
        foreach (string bundlepath in bundlespaths)
        {
            string[] deppaths = manifest.GetDirectDependencies(bundlepath);
            foreach (string deppath in deppaths)
            {
                if (!deppath.Contains("sharedbundles/"))
                {
                    var vardepprefabs = from path in allAssets where path.Contains("Assets/Resources/" + deppath) select path;
                    string[] depprefabs = vardepprefabs.ToArray<string>();
                    string[] depPathDepAssets = AssetDatabase.GetDependencies(depprefabs);
                    var varprefabs = from path in allAssets where path.Contains("Assets/Resources/" + bundlepath) select path;
                    string[] prefabs = varprefabs.ToArray<string>();
                    List<string> bundlePathDepAssets = new List<string>(AssetDatabase.GetDependencies(prefabs));
                    var varshareAssets =
                        from shareAsset in depPathDepAssets
                        where bundlePathDepAssets.Contains(shareAsset) && !shareAsset.Contains("#share")
                        select shareAsset;
                    Debug.Log("=========******=========");
                    Debug.LogWarning(bundlepath + "=不正确的依赖了=" + deppath + "==以下是他们共享的资源");
                    string[] shareAssets = varshareAssets.ToArray<string>();
                    foreach (string sa in shareAssets.ToArray<string>())
                    {
                        Debug.Log(sa);
                    }
                    Debug.Log("========================");
                }
            }
        }
        manifestBundle.Unload(true);
        AssetBundle.DestroyImmediate(manifestBundle);
    }
        
}
