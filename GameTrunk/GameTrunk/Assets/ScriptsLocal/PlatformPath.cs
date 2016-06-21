using UnityEngine;
using System.Collections;

public class PlatformPath 
{
    //不同平台下StreamingAssets的路径是不同的，这里需要注意一下。  
    public static readonly string PathURL =
#if UNITY_STANDALONE_WIN || UNITY_EDITOR  //windows平台和web平台
 "file://" + Application.dataPath + "/StreamingAssets/";
#elif UNITY_ANDROID   //安卓  
                    Application.streamingAssetsPath + "/";
#elif UNITY_IPHONE  //iPhone  
 "file://" + Application.streamingAssetsPath + "/"; 
#else
        string.Empty;  
#endif
}