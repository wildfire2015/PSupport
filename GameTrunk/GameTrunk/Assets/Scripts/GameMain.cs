using UnityEngine;
using UISystem;
using PSupport.LoadSystem;
using PSupport;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;


public class GameMain : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        ResourceLoadManager.updateToLatestBundles(onUpdateLatestBundles,new string[] { });
        
    }
	
	// Update is called once per frame
	void Update () {
	
	}
    void onUpdateLatestBundles(object o, eLoadedNotify loadedNotify)
    {
        if (loadedNotify == eLoadedNotify.Load_OneSuccessfull)
        {
            //string realpath = (o as string).Split('#')[0];
            //int  loadednum = int.Parse((o as string).Split('#')[1]);
            //int maxnum = int.Parse((o as string).Split('#')[2]);
            //DLoger.Log(realpath + "(" + loadednum + "/" + maxnum + ")");
           
        }
        else if(loadedNotify == eLoadedNotify.Load_Successfull)
        {
            //UIManager.addUIToRoot<UI_1>("assetsbundles/ui/systemui/UI_1");
            //UIManager.addUIToRoot<UI_2>("assetsbundles/ui/systemui/UI_2");
            //UIManager.addUIToRoot<UI_3>("assetsbundles/ui/systemui/UI_3");
            //UIManager.openUIInRoot<UI_1>(false, eOderLayer.eOderMid, 1);
            //UIManager.openUIInRoot<UI_2>();
            //UIManager.openUIInRoot<UI_3>();
            //ResourceLoadManager.requestRes("assetsbundles/model/normalmodel/chara_guyongbing/chara_guyongbing",typeof(GameObject), OnLoadedModel);
            //ResourceLoadManager.requestResNoAutoRelease("assetsbundles/level/shadowtest", typeof(AssetBundle), eLoadResPath.RP_URL, OnLoadedSceneBundle);
            

        }
    }
    void OnLoadedModel(object o, eLoadedNotify loadedNotify)
    {
        if (loadedNotify == eLoadedNotify.Load_Successfull)
        {
            DLoger.Log("assetsbundles/model/normalmodel/chara_guyongbing/chara_guyongbing");
            Object g = ResourceLoadManager.getRes("assetsbundles/model/normalmodel/chara_guyongbing/chara_guyongbing", typeof(GameObject));
            GameObject go = GameObject.Instantiate(g) as GameObject;
        }
        else if (loadedNotify == eLoadedNotify.Load_Failed)
        {
            DLoger.LogError("assetsbundles/model/normalmodel/chara_guyongbing/chara_guyongbing");
        }


    }
    void OnLoadedSceneBundle(object o, eLoadedNotify loadedNotify)
    {
        if (loadedNotify == eLoadedNotify.Load_Successfull)
        {
            StartCoroutine(loadscene());
            
        }
    }

    IEnumerator loadscene()
    {
        AsyncOperation a = SceneManager.LoadSceneAsync("shadowtest",LoadSceneMode.Additive);
        a.allowSceneActivation = false;
        while(a.progress < 0.9f)
        {
            yield return new WaitForFixedUpdate();
        }
        a.allowSceneActivation = true;
        while (a.isDone == false)
        {
            yield return new WaitForFixedUpdate();
        }
        
        GameObject b = GameObject.FindGameObjectWithTag("ShadowCameraTag");
        b.AddComponent<shadowtest>();
        ResourceLoadManager.unloadNoAutoReleasebundle("assetsbundles/level/shadowtest", typeof(AssetBundle), eLoadResPath.RP_URL);
    }
}
