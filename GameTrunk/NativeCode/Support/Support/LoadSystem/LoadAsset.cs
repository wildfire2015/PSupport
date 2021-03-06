﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/*******************************************************************************
* 
*             类名: LoadAsset
*             功能: 资源加载器
*             作者: 彭谦
*             日期: 2014.6.13
*             修改: 2015.12.7
*             
* *****************************************************************************/

namespace PSupport
{
    namespace LoadSystem
    {
        internal class LoadAsset : MonoBehaviour
        {
            static public LoadAsset getInstance()
            {
                return SingleMono.getInstance<LoadAsset>() as LoadAsset;
            }
            public void loadAsset(string sAssetPath, System.Type type, string sResGroupkey, Hash128 hash, bool basyn, bool bNoUseCatching, bool bautoReleaseBundle)
            {//异步加载
                StartCoroutine(beginToLoad(sAssetPath, type, sResGroupkey, hash ,basyn, bNoUseCatching,bautoReleaseBundle));
            }
            public IEnumerator beginToLoad(string sAssetPath, System.Type type, string sResGroupkey, Hash128 hash, bool basyn, bool bNoUseCatching,bool bautoReleaseBundle)
            {
                string assetsbundlepath;
                string assetname;
                if (sAssetPath.Contains("|"))
                {
                    assetsbundlepath = sAssetPath.Split('|')[0];
                    assetname = sAssetPath.Split('|')[1];
                }
                else
                {//没有'|',表示只是加载assetbundle,不加载里面的资源(例如场景Level对象,依赖assetbundle)
                    assetsbundlepath = sAssetPath;
                    assetname = string.Empty;
                }
                //CLog.Log("start to load===" + assetname);


                //资源标示
                string sReskey = (sAssetPath + type.ToString());

                //包路径Hash

                //记录此bundle被要求加载的协程次数,外部可能在一个资源加载挂起时多次调用同一资源的加载协程
                string sAssetbundlepath = assetsbundlepath;
                

                if (mDicbundleNum.ContainsKey(sAssetbundlepath))
                {
                    mDicbundleNum[sAssetbundlepath]++;
                }
                else
                {
                    mDicbundleNum.Add(sAssetbundlepath, 1);
                }


                //记录此资源被要求加载的协程次数,外部可能在一个资源加载挂起时多次调用同一资源的加载协程
                if (mDicAssetNum.ContainsKey(sReskey))
                {
                    mDicAssetNum[sReskey]++;
                }
                else
                {
                    mDicAssetNum.Add(sReskey, 1);
                }
                WWW mywww = null;
                if (mDicLoadingWWW.ContainsKey(sAssetbundlepath))
                {//如果已经在加载,等待
                    while (!mDicLoadedWWW.ContainsKey(sAssetbundlepath))
                    {
                        yield return 1;
                    }
                    mywww = mDicLoadedWWW[sAssetbundlepath];
                }
                else
                {

                    if (mDicLoadedWWW.ContainsKey(sAssetbundlepath))
                    {//如果已经加载完毕,则取出
                        mywww = mDicLoadedWWW[sAssetbundlepath];
                    }
                    else
                    {//如果没有,则开始加载,等待
                     //CLog.Log("begin to load www :" + assetsbundlepath);
                        if (bNoUseCatching)
                        {
                            //如果强行下载最新(强行更新version)
                            //int version = 0;
                            //while (Caching.IsVersionCached(assetsbundlepath, version) || version > 100)
                            //{
                            //    version++;
                            //}
                            ////Loger.Log("ABM_Version ===========" + version);
                            mywww = new WWW(assetsbundlepath);


                        }
                        else
                        {
                            //Loger.Log(abp + "===========" + abhash);
                            mywww = WWW.LoadFromCacheOrDownload(assetsbundlepath, hash);
                        }

                        mDicLoadingWWW.Add(sAssetbundlepath, mywww);
                    }



                    //第一个开启加载此资源的在这挂起
                    yield return mywww;

                }

                //加载完毕,加入完成列表,从正在加载中列表移除
                if (!mDicLoadedWWW.ContainsKey(sAssetbundlepath))
                {
                    mDicLoadedWWW.Add(sAssetbundlepath, mywww);
                    mDicLoadingWWW.Remove(sAssetbundlepath);
                }

                if (string.IsNullOrEmpty(mywww.error))
                {//加载assetsbundle成功

                    //缓存中不用的的资源,秒删
                    //Caching.expirationDelay = 0;
                    //CLog.Log("load www :" + assetsbundlepath + "===successful!");
                    if (assetname != string.Empty)
                    {
                        Object t = null;
                        //开始加载asset
                        if (basyn)
                        {//如果是异步加载

                            if (mDicLoadingAssets.ContainsKey(sReskey))
                            {//如果正在加载,则返回等待

                                while (!ResourceLoadManager._isLoadedRes(sReskey))
                                {
                                    yield return 1;
                                }

                            }
                            else
                            {//否则,开始加载

                                //文件对象名称
                                //CLog.Log("begin to load asset ==" + assetname);

                                AssetBundleRequest request = mywww.assetBundle.LoadAssetAsync(assetname, type);
                                mDicLoadingAssets.Add(sReskey, request);

                                //第一个要求加载此资源的在这挂起
                                yield return request;

                            }
                            //加载完毕
                            //                    CLog.Log("load asset ==" + assetname + "===successful!");
                            AssetBundleRequest myrequest = mDicLoadingAssets[sReskey];

                            
                            t = myrequest.asset as Object;
                            
                       
                           

                        }
                        else
                        {
                            //CLog.Log("begin to load asset ==" + assetname);

                            t = mywww.assetBundle.LoadAsset(assetname, type) as Object;

                        }

                        if (t != null)
                        {//加载成功,加入资源管理器,执行回调

                            //GameObject go = t as GameObject;
                            //if (go != null)
                            //{
                            //    Transform[] trans = go.GetComponentsInChildren<Transform>();
                            //    foreach (Transform tran in trans)
                            //    {
                            //        //这里把所有对象的资源都引用一遍,uniy5.3.5 bug导致不引用,有可能造成Assetbundle.Unload的时候会被卸载掉
                            //        //Renderer[] Renderers = tran.GetComponentsInChildren<Renderer>();
                            //        //foreach (Renderer com in Renderers)
                            //        //{
                            //        //    //材质会出这个bug
                            //        //    Material[] mats = com.sharedMaterials;
                            //        //}
                            //        //Animator[] Animators = tran.GetComponentsInChildren<Animator>();
                            //        //foreach (Animator com in Animators)
                            //        //{
                            //        //    avatar和rac会出这个bug
                            //        //    Avatar avt = com.avatar;
                            //        //    RuntimeAnimatorController rc = com.runtimeAnimatorController;
                            //        //}

                            //        //ParticleRenderer[] ParticleRenderers = tran.GetComponentsInChildren<ParticleRenderer>();
                            //        //foreach (ParticleRenderer com in ParticleRenderers)
                            //        //{
                            //        //    //粒子的材质会出这个bug
                            //        //    Material[] mats = com.sharedMaterials;

                            //        //}
                            //    }
                            //}
                            //CLog.Log("load asset ==" + assetname + "===successful");
                            ResourceLoadManager._addResAndRemoveInLoadingList(sReskey, t);
                            ResourceLoadManager._removePathInResGroup(sResGroupkey, sReskey,true, bautoReleaseBundle);
                        }
                        else
                        {

                            ResourceLoadManager._removeLoadingResFromList(sReskey);
                            ResourceLoadManager._removePathInResGroup(sResGroupkey, sReskey, false, bautoReleaseBundle);
                            DLoger.LogError("Load===" + sAssetPath + "===Failed");
                        }
                    }
                    else
                    {//只加载assetbundle的资源,不加载asset的时候的操作
                        if (bautoReleaseBundle)
                        {
                            ResourceLoadManager._removeLoadingResFromList(sReskey);
                        }
                        else
                        {
                            ResourceLoadManager._addResAndRemoveInLoadingList(sReskey, mywww.assetBundle);
                        }

                        ResourceLoadManager._removePathInResGroup(sResGroupkey, sReskey,true, bautoReleaseBundle);

                        //if(bautoReleaseBundle)
                        //{
                        //    ResourceLoadManager._removeRes(ireskey);
                        //}
                    }

                }
                else
                {//加载失败

                    ResourceLoadManager._removeLoadingResFromList(sReskey);
                    ResourceLoadManager._removePathInResGroup(sResGroupkey, sReskey, false, bautoReleaseBundle);
                    DLoger.LogError("Load www assetsbundle ==" + assetsbundlepath + "  failed!===" + mywww.error);

                }

                //处理完此资源的加载协程,对请求此资源的加载协程计数减一
                mDicAssetNum[sReskey]--;

                if (mDicAssetNum[sReskey] == 0)
                {//如果所有加载此资源的协程都处理完毕,释放资源
                    mDicLoadingAssets.Remove(sReskey);
                    mDicAssetNum.Remove(sReskey);
                }
                
                while (ResourceLoadManager._getDepBundleUesed(sAssetbundlepath) || (bautoReleaseBundle == false && ResourceLoadManager._isLoadedRes(sReskey)) /*|| ResourceLoadManager.mBAutoRelease == false*/)
                {//一些依赖bundle没有释放,或者非自动释放的bundle没有释放,继续等待
                    yield return 1;
                }
                //延迟释放,有些资源貌似有bug,实例化之后,引用要延迟计算,导致这里释放的时候会将实例化的资源释放掉,U3D的bug带日后能否解决
                //yield return 1;
               
                //yield return new WaitForSeconds(0.5f);
                mDicbundleNum[sAssetbundlepath]--;
                //DLoger.Log(sAssetbundlepath + "===引用计数===" + mDicbundleNum[sAssetbundlepath]);
                if (mDicbundleNum[sAssetbundlepath] == 0)
                {//如果用到这个bundle的协程全部结束
                    if (string.IsNullOrEmpty(mywww.error))
                    {
                        if(mDicLoadedWWW.ContainsValue(mywww))
                        {
                            //已经被释放(加载过程中,某些bundle计数为0了之后,没有马上调用unload,然后新的加载需求又使得计数增加,就会造成多次unload请求,所以有空的情况产生)
                            //if (mywww.assetBundle != null)
                            //{
                            mywww.assetBundle.Unload(false);
                            //DLoger.Log("mywwww.dispose");
                            mywww.Dispose();
                            mywww = null;
                            //}

                            mDicLoadedWWW.Remove(sAssetbundlepath);
                        }
                        
                        
                    }
                    

                }
                //if (mDicLoadedWWW.Count == 1 && mDicLoadingWWW.Count == 0)
                //{
                //    foreach (int item in mDicLoadedWWW.Keys)
                //    {
                //        //DLoger.Log(mDicbundleNum[item] + "," + item);

                //        DLoger.Log(mDicLoadedWWW[item].assetBundle.name);

                //    }
                //}
                //DLoger.Log(mDicLoadedWWW.Count + "," + mDicLoadingWWW.Count);




            }
            
            //记录同一资源加载协程的个数
            private Dictionary<string, int> mDicAssetNum = new Dictionary<string, int>();
            //记录同一assetsbundle加载协程的个数
            private Dictionary<string, int> mDicbundleNum = new Dictionary<string, int>();
            //记录正在加载的www
            private Dictionary<string, WWW> mDicLoadingWWW = new Dictionary<string, WWW>();
            //记录已经加载的www
            private Dictionary<string, WWW> mDicLoadedWWW = new Dictionary<string, WWW>();
            //记录正在加载的资源请求
            private Dictionary<string, AssetBundleRequest> mDicLoadingAssets = new Dictionary<string, AssetBundleRequest>();

        }
       
    }
   
}


