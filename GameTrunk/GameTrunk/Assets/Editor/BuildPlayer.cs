using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Callbacks;
using PSupport;
/*******************************************************************************
* 
*             类名: BuildPlayer
*             功能: 自动生成包,会把必要的操作全部执行一遍,时间较慢
*             作者: 彭谦
*             日期: 2015.12.7
*             修改:
*            
*             
* *****************************************************************************/
public class BuildPlayer
{
    [MenuItem("Build/BuildPlayer/BuildApk")]
    public static void BuildPlayerApk()
    {
        bulidPlayerForPlatForm(BuildTarget.Android, Path.GetDirectoryName(Application.dataPath) + "/PackOut/Android/" + Application.productName  + ".apk");
    }
    [MenuItem("Build/BuildPlayer/BuildXcode")]
    public static void BuildPlayerXcode()
    {
        bulidPlayerForPlatForm(BuildTarget.iOS, Path.GetDirectoryName(Application.dataPath) + "/PackOut/IOS/" + Application.productName);
    }
    [MenuItem("Build/BuildPlayer/BuildPCWin64")]
    public static void BuildPlayerPCWin64()
    {
        bulidPlayerForPlatForm(BuildTarget.StandaloneWindows64, Path.GetDirectoryName(Application.dataPath) + "/PackOut/PC/" + Application.productName + ".exe");
    }


    public static void bulidPlayerForPlatForm(BuildTarget targetplatform, string path)
    {
        ToolFunctions.ClearConsole();
        ChangePlatformDo(targetplatform);
        List<string> names = new List<string>();
        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
                continue;
            if (e.enabled && (e.path.Contains("LoadScene") || e.path.Contains("Launcher")))
                names.Add(e.path);
        }
        //BuildPrefabForLoadSprite.BuildSpritePrefab();
        //ExportAssetBundle.ClearAllAssetBundlesNames();
        //================打包所有资源bundle=============
        // ExportAssetBundle.BuildAssetBundlesForPlatform(targetplatform,ExportAssetBundle.pathlocal);
        //================删除不必要的资源,放到临时目录=============
        ToolFunctions.CreateNewFolder(Path.GetDirectoryName(path));
        
        string tempprefabpath = System.Environment.CurrentDirectory + "/TempPrefab";
        ToolFunctions.CreateNewFolder(tempprefabpath);
        string tempArtRes = tempprefabpath + "/ArtRes/artscripts";
        string tempAssetsbundles = tempprefabpath + "/Resources/assetsbundles";
        string tempScripts = tempprefabpath + "/Scripts";
        //string tempStreamingAssets = tempprefabpath + "/StreamingAssets";
        string sourceArtRes = Application.dataPath + "/ArtRes/artscripts";
        string sourceAssetsbundles = Application.dataPath + "/Resources/assetsbundles";
        string sourceScripts = Application.dataPath + "/Scripts";
        //string sourceStreamingAssets = Application.dataPath + "/StreamingAssets";
        //备份美术目录(待定)和/Resources/assetsbundles目录
        ToolFunctions.CopyDirectory(sourceArtRes, tempArtRes);
        ToolFunctions.CopyDirectory(sourceAssetsbundles, tempAssetsbundles);
        
        if(targetplatform != BuildTarget.iOS)
        {//如果不是IOS平台,删除本地代码
            ToolFunctions.CopyDirectory(sourceScripts, tempScripts);
            ToolFunctions.DeleteFolder(sourceScripts);
        }
        //删除美术目录(待定)和/Resources/assetsbundles目录
        ToolFunctions.DeleteFolder(sourceArtRes);
        ToolFunctions.DeleteFolder(sourceAssetsbundles);

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        //================开始打包=============
        string resoult = BuildPipeline.BuildPlayer(names.ToArray(), path, targetplatform, BuildOptions.ConnectWithProfiler | BuildOptions.Development);

        if (resoult != "")
        {
            Debug.LogError(resoult);
        }

        ///================还原操作=============
        ToolFunctions.CopyDirectory(tempArtRes, sourceArtRes);

        ToolFunctions.CopyDirectory(tempAssetsbundles, sourceAssetsbundles);
        if (targetplatform != BuildTarget.iOS)
        {
            ToolFunctions.CopyDirectory(tempScripts, sourceScripts);
        }

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }
    [PostProcessBuild(0)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        Debug.Log("生成 " + pathToBuiltProject + " 成功!");
    }
    [InitializeOnLoadMethod]
    static void onUnityLoaded()
    {
        EditorUserBuildSettings.activeBuildTargetChanged += OnChangePlatform;
    }
    static void OnChangePlatform()
    {
        Debug.Log("changeplatform :" + EditorUserBuildSettings.activeBuildTarget);
        BuildTarget targetplatform = EditorUserBuildSettings.activeBuildTarget;
        ChangePlatformDo(targetplatform);


    }
    static void ChangePlatformDo(BuildTarget targetplatform)
    {
        if (targetplatform == BuildTarget.iOS)
        {
            PlayerSettings.apiCompatibilityLevel = ApiCompatibilityLevel.NET_2_0;
            PlayerSettings.SetPropertyInt("ScriptingBackend", (int)ScriptingImplementation.IL2CPP, targetplatform);
            PlayerSettings.strippingLevel = StrippingLevel.Disabled;
        }
        else if (targetplatform == BuildTarget.Android)
        {
            PlayerSettings.apiCompatibilityLevel = ApiCompatibilityLevel.NET_2_0_Subset;
            PlayerSettings.SetPropertyInt("ScriptingBackend", (int)ScriptingImplementation.Mono2x, targetplatform);
            PlayerSettings.strippingLevel = StrippingLevel.Disabled;
            PlayerSettings.Android.targetDevice = AndroidTargetDevice.FAT;

        }
        else if (targetplatform == BuildTarget.StandaloneWindows64)
        {
            PlayerSettings.apiCompatibilityLevel = ApiCompatibilityLevel.NET_2_0_Subset;
            PlayerSettings.SetPropertyInt("ScriptingBackend", (int)ScriptingImplementation.Mono2x, targetplatform);
            PlayerSettings.strippingLevel = StrippingLevel.Disabled;
            
        }
    }
}
