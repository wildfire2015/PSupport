using UnityEngine;
using PSupport.LoadSystem;
using PSupport;

public class Launcher : MonoBehaviour
{
    //在Editor模式下是否读取bundle资源
    public bool UseBundleInEditor = true;
    //是否开启Debug输出窗口
    public bool ShowDebugLogWin = true;
    //是否开启DLoger.Log输出
    public bool ShowLog = true;
    //是否显示FPS
    public bool ShowPFS = true;
    //在Editor模式下是否读取本地代码(如果不用bundle,就不能使用分离代码,代码也是bundle)
    public bool UseLocalCodeInEditor = true;



    void Start()
    {
        if (ShowPFS)
        {
           
        }


        //必须最开始就设置好资源地址
#if LOCAL_URL
      //  ResourceLoadManager.mResourcesURLAddress = "file://" + Application.dataPath + "/StreamingAssetsURL/";
#endif
        ResourceLoadManager.mResourceStreamingAssets = PlatformPath.PathURL;

        DLoger.EnableLog = ShowLog;

        if (ShowDebugLogWin)
        {
            ResourceLoadManager.requestRes("local/ui/UIDebugLog", typeof(Object), eLoadResPath.RP_Resources, null, null, true);
            GameObject UIDebugLogWin = Instantiate(ResourceLoadManager.getRes("local/ui/UIDebugLog", eLoadResPath.RP_Resources)) as GameObject;
            UIDebugLogWin.AddComponent<UIDebugLog>();
            DontDestroyOnLoad(UIDebugLogWin);
        }




#if UNITY_EDITOR
        ResourceLoadManager.mbuseassetbundle = UseBundleInEditor;
        if (!UseBundleInEditor)
        {//如果不用bundle,那就无法读取bundle代码,只能使用本地代码
            UseLocalCodeInEditor = true;
        }
        if (!UseLocalCodeInEditor)
        {
            gameObject.AddComponent<GameMainDll>();
        }
        else
        {
            gameObject.AddComponent<GameMain>();
        }
#else
        ResourceLoadManager.mbuseassetbundle = true;
#if UNITY_IOS
        gameObject.AddComponent<GameMain>();
#else
        gameObject.AddComponent<GameMainDll>();

#endif
#endif
    }

}
