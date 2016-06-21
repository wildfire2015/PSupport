using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/*******************************************************************************
 * 
 *             类名: ResourceLoadManager
 *             功能: 资源加载管理器
 *             作者: 彭谦
 *             日期: 2014.6.13
 *             修改:
 *             备注:该资源加载模块是适应unity5的新的加载机制,用提供的公共接口(若干重载)加载资源,资源原文件夹路径都为Resources下的路径,配合打包插件,会打包对应的
 *             assetbundle文件,只需修改开关mbusepack便可在源资源和打包资源之间切换,无需关心依赖资源,和缓存资源的管理,此模块会自动从下载路径(资源服务器)下载更新
 *             的assetbundle文件,如果没有更新的assetbundle,默认用缓存中的
 *             
 * *****************************************************************************/



namespace PSupport
{
    namespace LoadSystem
    {
        /// <summary>
        /// 回调函数的委托
        /// </summary>
        /// <param name="obj">回调参数</param>
        /// <param name="loadedNotify">加载结果</param>
        public delegate void ProcessDelegateArgc(object obj = null, eLoadedNotify loadedNotify = eLoadedNotify.Load_Successfull);

        /// <summary>
        /// 资源加载管理器
        /// </summary>
        public class ResourceLoadManager
        {

            private ResourceLoadManager() { }
            /// <summary>
            /// 在URLPath上读取类型为Object的资源,如果设置了URL和StreamingAssets,则会比较本地资源和服务器资源,如果本地和服务器一样则用本地的,如果不一样则用服务器的
            /// 如果只设置了服务器则用服务器,如果只设置了本地则用本地
            /// 请求资源,指定同步加载还是异步加载,如果已经加载成功,返回,如果正在加载返回空,如果没有加载,则同步或者异步加载,返回空,异步加载完毕,调用回调函数proc
            /// 求情资源的类型,如果设置了类型,则getRes相应的资源时,也要带上请求时用的资源类型,默认都是Object,注意:在removeRes的时候,要对应创建时类型
            /// </summary>
            /// <param name="spath">路径</param>
            /// <param name="proc">加载结果回调</param>
            /// <param name="o">回调传递参数</param>
            /// <param name="basyn">解压是否异步</param>
            public static void requestRes(string spath, ProcessDelegateArgc proc = null, object o = null, bool basyn = true)
            {
                if (mbuseassetbundle)
                {
                    _checkDependenceList(new CloadParam(spath, typeof(Object), eLoadResPath.RP_URL, proc, o, basyn));
                }
                else
                {
                    string[] spaths = new string[1];
                    System.Type[] types = new System.Type[1];
                    eLoadResPath[] eloadResTypes = new eLoadResPath[1];
                    spaths[0] = spath;
                    types[0] = typeof(Object);
                    eloadResTypes[0] = eLoadResPath.RP_Resources;
                    _requestRes(spaths, types, eloadResTypes, proc, o, basyn);
                }

            }
            /// <summary>
            /// 在URLPath上读取类型为Object的资源,如果设置了URL和StreamingAssets,则会比较本地资源和服务器资源,如果本地和服务器一样则用本地的,如果不一样则用服务器的
            /// 如果只设置了服务器则用服务器,如果只设置了本地则用本地
            /// 请求资源,指定同步加载还是异步加载,如果已经加载成功,返回,如果正在加载返回空,如果没有加载,则同步或者异步加载,返回空,异步加载完毕,调用回调函数proc
            /// 求情资源的类型,如果设置了类型,则getRes相应的资源时,也要带上请求时用的资源类型,默认都是Object,注意:在removeRes的时候,要对应创建时类型
            /// </summary>
            /// <param name="spath"></param>
            /// <param name="type">资源类型</param>
            /// <param name="proc"></param>
            /// <param name="o"></param>
            /// <param name="basyn">解压是否异步</param>
            public static void requestRes(string spath, System.Type type, ProcessDelegateArgc proc = null, object o = null, bool basyn = true)
            {
                if (mbuseassetbundle)
                {
                    _checkDependenceList(new CloadParam(spath, type, eLoadResPath.RP_URL, proc, o, basyn));
                }
                else
                {
                    string[] spaths = new string[1];
                    System.Type[] types = new System.Type[1];
                    eLoadResPath[] eloadResTypes = new eLoadResPath[1];
                    spaths[0] = spath;
                    types[0] = type;
                    eloadResTypes[0] = eLoadResPath.RP_Resources;
                    _requestRes(spaths, types, eloadResTypes, proc, o, basyn);
                }

            }
            /// <summary>
            /// 在URLPath上读取类型为Object的资源,如果设置了URL和StreamingAssets,则会比较本地资源和服务器资源,如果本地和服务器一样则用本地的,如果不一样则用服务器的
            /// 如果只设置了服务器则用服务器,如果只设置了本地则用本地
            /// 请求资源,指定同步加载还是异步加载,如果已经加载成功,返回,如果正在加载返回空,如果没有加载,则同步或者异步加载,返回空,异步加载完毕,调用回调函数proc
            /// 求情资源的类型,如果设置了类型,则getRes相应的资源时,也要带上请求时用的资源类型,默认都是Object,注意:在removeRes的时候,要对应创建时类型
            /// </summary>
            /// <param name="spath">资源路径名称数组(相对于Resources/),不要带扩展名</param>
            /// <param name="eloadResType">加载资源的路径类型,如果设置了Resources,是不受管理器mbuseassetbundle影响的</param>
            /// <param name="proc">加载完毕回调函数</param>
            /// <param name="o">回调函数的参数</param>
            /// <param name="basyn"> 解压是否异步</param>

            public static void requestRes(string spath, eLoadResPath eloadResType = eLoadResPath.RP_URL, ProcessDelegateArgc proc = null, object o = null, bool basyn = true)
            {
                if (mbuseassetbundle && eloadResType != eLoadResPath.RP_Resources)
                {
                    _checkDependenceList(new CloadParam(spath, typeof(Object), eloadResType, proc, o, basyn));
                }
                else
                {
                    string[] spaths = new string[1];
                    System.Type[] types = new System.Type[1];
                    eLoadResPath[] eloadResTypes = new eLoadResPath[1];
                    spaths[0] = spath;
                    types[0] = typeof(Object);
                    eloadResTypes[0] = eloadResType;
                    _requestRes(spaths, types, eloadResTypes, proc, o, basyn);
                }

            }
            /// <summary>
            /// 在URLPath上读取类型为Object的资源,如果设置了URL和StreamingAssets,则会比较本地资源和服务器资源,如果本地和服务器一样则用本地的,如果不一样则用服务器的
            /// 如果只设置了服务器则用服务器,如果只设置了本地则用本地
            /// 请求资源,指定同步加载还是异步加载,如果已经加载成功,返回,如果正在加载返回空,如果没有加载,则同步或者异步加载,返回空,异步加载完毕,调用回调函数proc
            /// 求情资源的类型,如果设置了类型,则getRes相应的资源时,也要带上请求时用的资源类型,默认都是Object,注意:在removeRes的时候,要对应创建时类型
            /// </summary>
            /// <param name="spaths">所有路径</param>
            /// <param name="eloadResType">加载资源的路径类型</param>
            /// <param name="proc">加载完毕回调函数</param>
            /// <param name="o">回调参数</param>
            /// <param name="basyn">解压是否异步</param>


            public static void requestRes(string[] spaths, eLoadResPath eloadResType = eLoadResPath.RP_URL, ProcessDelegateArgc proc = null, object o = null, bool basyn = true)
            {
                if (mbuseassetbundle && eloadResType != eLoadResPath.RP_Resources)
                {
                    System.Type[] types = new System.Type[spaths.Length];
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        types[i] = typeof(Object);
                    }
                    eLoadResPath[] eloadResTypes = new eLoadResPath[spaths.Length];
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        eloadResTypes[i] = eloadResType;
                    }
                    _checkDependenceList(new CloadParam(spaths, types, eloadResTypes, proc, o, basyn));
                }
                else
                {
                    System.Type[] types = new System.Type[spaths.Length];
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        types[i] = typeof(Object);
                    }
                    eLoadResPath[] eloadResTypes = new eLoadResPath[spaths.Length];
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        eloadResTypes[i] = eloadResType;
                    }
                    _requestRes(spaths, types, eloadResTypes, proc, o, basyn);
                }


            }
            /// <summary>
            /// 在URLPath上读取类型为Object的资源,如果设置了URL和StreamingAssets,则会比较本地资源和服务器资源,如果本地和服务器一样则用本地的,如果不一样则用服务器的
            /// 如果只设置了服务器则用服务器,如果只设置了本地则用本地
            /// 请求资源,指定同步加载还是异步加载,如果已经加载成功,返回,如果正在加载返回空,如果没有加载,则同步或者异步加载,返回空,异步加载完毕,调用回调函数proc
            /// 求情资源的类型,如果设置了类型,则getRes相应的资源时,也要带上请求时用的资源类型,默认都是Object,注意:在removeRes的时候,要对应创建时类型
            /// </summary>
            /// <param name="spaths">所有路径</param>
            /// <param name="type">类型</param>
            /// <param name="eloadResType">加载资源的路径类型</param>
            /// <param name="proc">加载完毕回调函数</param>
            /// <param name="o">回调参数</param>
            /// <param name="basyn">解压是否异步</param>

            public static void requestRes(string[] spaths, System.Type type, eLoadResPath eloadResType = eLoadResPath.RP_URL, ProcessDelegateArgc proc = null, object o = null, bool basyn = true)
            {
                if (mbuseassetbundle && eloadResType != eLoadResPath.RP_Resources)
                {
                    System.Type[] types = new System.Type[spaths.Length];
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        types[i] = type;
                    }
                    eLoadResPath[] eloadResTypes = new eLoadResPath[spaths.Length];
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        eloadResTypes[i] = eloadResType;
                    }
                    _checkDependenceList(new CloadParam(spaths, types, eloadResTypes, proc, o, basyn));
                }
                else
                {
                    System.Type[] types = new System.Type[spaths.Length];
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        types[i] = type;
                    }
                    eLoadResPath[] eloadResTypes = new eLoadResPath[spaths.Length];
                    for (int i = 0; i < spaths.Length; i++)
                    {
                        eloadResTypes[i] = eloadResType;
                    }
                    _requestRes(spaths, types, eloadResTypes, proc, o, basyn);
                }

            }
            /// <summary>
            /// 在URLPath上读取类型为Object的资源,如果设置了URL和StreamingAssets,则会比较本地资源和服务器资源,如果本地和服务器一样则用本地的,如果不一样则用服务器的
            /// 如果只设置了服务器则用服务器,如果只设置了本地则用本地
            /// 请求资源,指定同步加载还是异步加载,如果已经加载成功,返回,如果正在加载返回空,如果没有加载,则同步或者异步加载,返回空,异步加载完毕,调用回调函数proc
            /// 求情资源的类型,如果设置了类型,则getRes相应的资源时,也要带上请求时用的资源类型,默认都是Object,注意:在removeRes的时候,要对应创建时类型
            /// </summary>
            /// <param name="spath"></param>
            /// <param name="type"></param>
            /// <param name="eloadResType"></param>
            /// <param name="proc"></param>
            /// <param name="o"></param>
            /// <param name="basyn">解压是否异步</param>
            /// 
            /// <returns></returns>

            public static void requestRes(string spath, System.Type type, eLoadResPath eloadResType = eLoadResPath.RP_URL, ProcessDelegateArgc proc = null, object o = null, bool basyn = true)
            {

                if (mbuseassetbundle && eloadResType != eLoadResPath.RP_Resources)
                {
                    _checkDependenceList(new CloadParam(spath, type, eloadResType, proc, o, basyn));
                }
                else
                {
                    string[] spaths = new string[1];
                    System.Type[] types = new System.Type[1];
                    eLoadResPath[] eloadResTypes = new eLoadResPath[1];
                    spaths[0] = spath;
                    types[0] = type;
                    eloadResTypes[0] = eloadResType;
                    _requestRes(spaths, types, eloadResTypes, proc, o, basyn);
                }


            }

            /// <summary>
            /// 读取的资源的bundle不会自动释放,要手动调用unloadNoAutoReleasebundle
            /// </summary>
            /// <param name="spath"></param>
            /// <param name="type"></param>
            /// <param name="eloadResType"></param>
            /// <param name="proc"></param>
            /// <param name="o"></param>
            /// <param name="basyn"></param>
            public static void requestResNoAutoRelease(string spath, System.Type type, eLoadResPath eloadResType = eLoadResPath.RP_URL, ProcessDelegateArgc proc = null, object o = null, bool basyn = true)
            {

                if (mbuseassetbundle && eloadResType != eLoadResPath.RP_Resources)
                {
                    _checkDependenceList(new CloadParam(spath, type, eloadResType, proc, o, basyn,false));
                }
                else
                {
                    string[] spaths = new string[1];
                    System.Type[] types = new System.Type[1];
                    eLoadResPath[] eloadResTypes = new eLoadResPath[1];
                    spaths[0] = spath;
                    types[0] = type;
                    eloadResTypes[0] = eloadResType;
                    _requestRes(spaths, types, eloadResTypes, proc, o, basyn, false);
                }


            }
            /// <summary>
            /// 释放对应调用requestResNoAutoRelease的资源
            /// </summary>
            /// <param name="respath"></param>
            /// <param name="type"></param>
            /// <param name="eloadResType"></param>
            public static void unloadNoAutoReleasebundle(string respath, System.Type type, eLoadResPath eloadResType = eLoadResPath.RP_URL)
            {
                if(mbuseassetbundle)
                {
                    AssetBundleManifest mainfest = null;
                    eLoadResPath loadrespath = eLoadResPath.RP_Unknow;
                    eLoadResPathState eloadresstate = _getLoadResPathState();
                    if (eloadresstate == eLoadResPathState.LS_ReadURLOnly)
                    {

                        mainfest = _getAssetBundleManifest(eLoadResPath.RP_URL);
                        loadrespath = eLoadResPath.RP_URL;

                    }

                    else if (eloadresstate == eLoadResPathState.LS_ReadStreamingOnly)
                    {

                        mainfest = _getAssetBundleManifest(eLoadResPath.RP_StreamingAssets);
                        loadrespath = eLoadResPath.RP_StreamingAssets;
                    }
                    else if (eloadresstate == eLoadResPathState.LS_ReadURLForUpdate)
                    {

                        mainfest = _getAssetBundleManifest(eloadResType);
                        loadrespath = eloadResType;
                    }
                    string[] depbundles = mainfest.GetAllDependencies(respath);
                    foreach (string bundlepath in depbundles)
                    {
                        string sReskeybundle = _getResKey(bundlepath, typeof(AssetBundle), eloadResType);
                        _releaseAssetDependenceBundle(sReskeybundle);
                        removeRes(bundlepath, typeof(AssetBundle), eloadResType);
                    }
                    string sReskey = _getResKey(respath, type, eloadResType);
                    _releaseAssetDependenceBundle(sReskey);
                    removeRes(respath, type, eloadResType);
                }
                
            }
            /// <summary>
            /// 在URLPath上读取类型为Object的资源,如果设置了URL和StreamingAssets,则会比较本地资源和服务器资源,如果本地和服务器一样则用本地的,如果不一样则用服务器的
            /// 如果只设置了服务器则用服务器,如果只设置了本地则用本地
            /// 请求资源,指定同步加载还是异步加载,如果已经加载成功,返回,如果正在加载返回空,如果没有加载,则同步或者异步加载,返回空,异步加载完毕,调用回调函数proc
            /// 求情资源的类型,如果设置了类型,则getRes相应的资源时,也要带上请求时用的资源类型,默认都是Object,注意:在removeRes的时候,要对应创建时类型
            /// </summary>
            /// <param name="spaths">所有路径</param>
            /// <param name="types">所有路径对应的类型</param>
            /// <param name="eloadResType">加载资源的路径类型</param>
            /// <param name="proc">加载完毕回调函数</param>
            /// <param name="o">回调参数</param>
            /// <param name="basyn">解压是否异步</param>

            public static void requestRes(string[] spaths, System.Type[] types, eLoadResPath eloadResType = eLoadResPath.RP_URL, ProcessDelegateArgc proc = null, object o = null, bool basyn = true)
            {
                eLoadResPath[] eloadResTypes = new eLoadResPath[spaths.Length];
                for (int i = 0; i < spaths.Length; i++)
                {
                    eloadResTypes[i] = eloadResType;
                }
                if (mbuseassetbundle && eloadResType != eLoadResPath.RP_Resources)
                {
                    _checkDependenceList(new CloadParam(spaths, types, eloadResTypes, proc, o, basyn));
                }
                else
                {
                    _requestRes(spaths, types, eloadResTypes, proc, o, basyn);
                }
            }

            /// <summary>
            /// 更新服务器上最新的资源
            /// </summary>
            /// <param name="loadedproc">回调函数</param>
            /// <param name="updateOnlyPack">这里的资源包只做更新,不做初始化下载,就是说,除非本地已经下载过这个包,才会参与包更新,如果本地没有这个包,这不会去下载</param>
            public static void updateToLatestBundles(ProcessDelegateArgc loadedproc,string[] updateOnlyPack)
            {
                Hashtable proc_pack = new Hashtable();
                proc_pack.Add("proc", loadedproc);
                proc_pack.Add("updateOnlyPack", updateOnlyPack);
                if (mbuseassetbundle)
                {
                    eLoadResPathState eloadresstate = _getLoadResPathState();
                    if (eloadresstate == eLoadResPathState.LS_ReadURLOnly)
                    {
                        //,意味着只要读取资源服务器的资源
                        if (_mURLAssetBundleManifest == null)
                        {
                            string[] paths = new string[1];
                            System.Type[] tps = new System.Type[1];
                            eLoadResPath[] eloadResTypes = new eLoadResPath[1];
                            paths[0] = _getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_URL);
                            tps[0] = typeof(AssetBundleManifest);
                            eloadResTypes[0] = eLoadResPath.RP_URL;
                            _requestRes(paths, tps, eloadResTypes, _OnLoadedLatestManifestForUpdate, proc_pack, true, true);
                        }
                        else
                        {
                            _OnLoadedLatestManifestForUpdate(proc_pack, eLoadedNotify.Load_Successfull);
                        }
                    }
                    else if (eloadresstate == eLoadResPathState.LS_ReadStreamingOnly)
                    {
                        //如果指明了要读取本地资源,并且设置了本地资源目录
                        if (_mLocalAssetBundleManifest == null)
                        {
                            string[] paths = new string[1];
                            System.Type[] tps = new System.Type[1];
                            eLoadResPath[] eloadResTypes = new eLoadResPath[1];
                            paths[0] = _getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_StreamingAssets);
                            tps[0] = typeof(AssetBundleManifest);
                            eloadResTypes[0] = eLoadResPath.RP_StreamingAssets;
                            _requestRes(paths, tps, eloadResTypes, _OnLoadedLatestManifestForUpdate, proc_pack, true, true);
                        }
                        else
                        {
                            _OnLoadedLatestManifestForUpdate(proc_pack, eLoadedNotify.Load_Successfull);
                        }
                    }
                    else if (eloadresstate == eLoadResPathState.LS_ReadURLForUpdate)
                    {
                        //如果URL的资源表或者本地资源表没有加载,则一起加载
                        if (_mURLAssetBundleManifest == null || _mLocalAssetBundleManifest)
                        {
                            //为加载依赖关系列表,先读取依赖关系表
                            string[] paths = new string[2];
                            System.Type[] tps = new System.Type[2];
                            eLoadResPath[] eloadResTypes = new eLoadResPath[2];
                            paths[0] = _getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_URL);
                            paths[1] = _getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_StreamingAssets);
                            tps[0] = typeof(AssetBundleManifest);
                            tps[1] = typeof(AssetBundleManifest);
                            eloadResTypes[0] = eLoadResPath.RP_URL;
                            eloadResTypes[1] = eLoadResPath.RP_StreamingAssets;
                            _requestRes(paths, tps, eloadResTypes, _OnLoadedLatestManifestForUpdate, proc_pack, true, true);

                        }
                        else
                        {
                            //已经加载完
                            _OnLoadedLatestManifestForUpdate(proc_pack, eLoadedNotify.Load_Successfull);
                        }
                    }
                    else
                    {
                        DLoger.LogError("you request a assetsbundle from  error Paths!");
                    }

                }
                else
                {
                    _OnLoadedLatestManifestForUpdate(proc_pack, eLoadedNotify.Load_Successfull);
                }

            }
            /// <summary>
            /// 获得Manifest的bundle的名字,URL上的名字和本地的名字不能重名
            /// </summary>
            /// <param name="eloadrespath"></param>
            /// <returns></returns>
            internal static string _getStreamingAssetsNameByLoadStyle(eLoadResPath eloadrespath)
            {
                if(eloadrespath == eLoadResPath.RP_URL)
                {
                    return mBundlesInfoFileName + "URL" + "/AssetBundleManifest";
                }
                else
                {
                    return mBundlesInfoFileName + "/AssetBundleManifest";
                }
            }
            //检查依赖列表
            private static void _checkDependenceList(CloadParam p)
            {
                eLoadResPathState eloadresstate = _getLoadResPathState();
                if (eloadresstate == eLoadResPathState.LS_ReadURLOnly)
                {
                    //,意味着只要读取资源服务器的资源
                    if (_mURLAssetBundleManifest == null)
                    {
                        string[] paths = new string[1];
                        System.Type[] tps = new System.Type[1];
                        eLoadResPath[] eloadResTypes = new eLoadResPath[1];
                        paths[0] = _getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_URL);
                        tps[0] = typeof(AssetBundleManifest);
                        eloadResTypes[0] = eLoadResPath.RP_URL;
                        _requestRes(paths, tps, eloadResTypes, _OnLoadedAssetBundleManifestForDepdence, p, true, true);
                    }
                    else
                    {
                        _OnLoadedAssetBundleManifestForDepdence(p, eLoadedNotify.Load_Successfull);
                    }
                }
                else if (eloadresstate == eLoadResPathState.LS_ReadStreamingOnly)
                {
                    //如果指明了要读取本地资源,并且设置了本地资源目录
                    if (_mLocalAssetBundleManifest == null)
                    {
                        string[] paths = new string[1];
                        System.Type[] tps = new System.Type[1];
                        eLoadResPath[] eloadResTypes = new eLoadResPath[1];
                        paths[0] = _getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_StreamingAssets);
                        tps[0] = typeof(AssetBundleManifest);
                        eloadResTypes[0] = eLoadResPath.RP_StreamingAssets;
                        _requestRes(paths, tps, eloadResTypes, _OnLoadedAssetBundleManifestForDepdence, p, true, true);
                    }
                    else
                    {
                        _OnLoadedAssetBundleManifestForDepdence(p, eLoadedNotify.Load_Successfull);
                    }
                }
                else if (eloadresstate == eLoadResPathState.LS_ReadURLForUpdate)
                {
                    //如果URL的资源表或者本地资源表没有加载,则一起加载
                    if (_mURLAssetBundleManifest == null || _mLocalAssetBundleManifest)
                    {
                        //为加载依赖关系列表,先读取依赖关系表
                        string[] paths = new string[2];
                        System.Type[] tps = new System.Type[2];
                        eLoadResPath[] eloadResTypes = new eLoadResPath[2];
                        paths[0] = _getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_URL);
                        paths[1] = _getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_StreamingAssets);
                        tps[0] = typeof(AssetBundleManifest);
                        tps[1] = typeof(AssetBundleManifest);
                        eloadResTypes[0] = eLoadResPath.RP_URL;
                        eloadResTypes[1] = eLoadResPath.RP_StreamingAssets;
                        _requestRes(paths, tps, eloadResTypes, _OnLoadedAssetBundleManifestForDepdence, p, true, true);

                    }
                    else
                    {
                        //已经加载完
                        _OnLoadedAssetBundleManifestForDepdence(p, eLoadedNotify.Load_Successfull);
                    }
                }
                else
                {
                    DLoger.LogError("you request a assetsbundle from  error Paths!");
                }
            }


            //已经加载完依赖列表
            private static void _OnLoadedAssetBundleManifestForDepdence(object obj = null, eLoadedNotify loadedNotify = eLoadedNotify.Load_Successfull)
            {//开始加载依赖资源
                if (loadedNotify == eLoadedNotify.Load_Successfull)
                {
                    if (_mURLAssetBundleManifest == null && mResourcesURLAddress != string.Empty)
                    {
                        _mURLAssetBundleManifest = (AssetBundleManifest)getRes(_getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_URL), typeof(AssetBundleManifest), eLoadResPath.RP_URL);
                    }
                    if (_mLocalAssetBundleManifest == null && mResourceStreamingAssets != string.Empty)
                    {
                        _mLocalAssetBundleManifest = (AssetBundleManifest)getRes(_getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_StreamingAssets), typeof(AssetBundleManifest), eLoadResPath.RP_StreamingAssets);
                    }
                    CloadParam param = (CloadParam)obj;
                    eLoadResPathState eloadresstate = _getLoadResPathState();
                    List<string> depBundleNameList = new List<string>();
                    List<eLoadResPath> depBundleLoadPathlist = new List<eLoadResPath>();
                    for (int i = 0; i < param.mpaths.Length; i++)
                    {
                        string bundlepath = "";
                        if (param.mtypes[i] != typeof(AssetBundle))
                        {
                            bundlepath = Path.GetDirectoryName(param.mpaths[i]);
                        }
                        else
                        {
                            bundlepath = param.mpaths[i];
                        }
                        if (_mDicLoadedRes.ContainsKey(_getResKey(param.mpaths[i], param.mtypes[i],eLoadResPath.RP_URL)))
                        {
                            continue;
                        }
                        AssetBundleManifest mainfest = null;
                        eLoadResPath loadrespath = eLoadResPath.RP_Unknow;
                        if (eloadresstate == eLoadResPathState.LS_ReadURLOnly)
                        {

                            mainfest = _getAssetBundleManifest(eLoadResPath.RP_URL);
                            loadrespath = eLoadResPath.RP_URL;

                        }

                        else if (eloadresstate == eLoadResPathState.LS_ReadStreamingOnly)
                        {

                            mainfest = _getAssetBundleManifest(eLoadResPath.RP_StreamingAssets);
                            loadrespath = eLoadResPath.RP_StreamingAssets;
                        }
                        else if (eloadresstate == eLoadResPathState.LS_ReadURLForUpdate)
                        {

                            mainfest = _getAssetBundleManifest(param.meloadResTypes[i]);
                            loadrespath = param.meloadResTypes[i];
                        }
                        else
                        {
                            DLoger.LogError("you request a assetsbundle from  error Paths!");
                            return;
                        }
                        string[] deppaths = mainfest.GetAllDependencies(bundlepath);
                        for (int j = 0; j < deppaths.Length; j++)
                        {
                            _addAssetDependenceBundle(param.mpaths[i],param.mtypes[i],param.meloadResTypes[i], deppaths[j], loadrespath);
                            if (!depBundleNameList.Contains(deppaths[j]))
                            {
                                depBundleNameList.Add(deppaths[j]);
                                depBundleLoadPathlist.Add(loadrespath);
                            }
                        }
                        _addAssetDependenceBundle(param.mpaths[i], param.mtypes[i], param.meloadResTypes[i], bundlepath, loadrespath);
                        if (!depBundleNameList.Contains(bundlepath))
                        {
                            depBundleNameList.Add(bundlepath);
                            depBundleLoadPathlist.Add(loadrespath);
                        }

                    }


                    if (depBundleNameList.Count != 0)
                    {
                        _loadDependenceBundles(depBundleNameList.ToArray(), depBundleLoadPathlist.ToArray(), _OnloadedDependenceBundles, param);
                    }
                    else
                    {
                        _OnloadedDependenceBundles(param, eLoadedNotify.Load_Successfull);
                    }

                }
            }
            internal static void _addAssetDependenceBundle(string respath, System.Type restype, eLoadResPath eresloadrespath, string bundlepath,  eLoadResPath ebundleloadrespath)
            {
                string ireskey = _getResKey(respath, restype, eresloadrespath);
                string sbundlekey = _getRealPath(bundlepath, typeof(AssetBundle), ebundleloadrespath).msRealPath;
                if (!_mDicAssetsDependBundles.ContainsKey(ireskey))
                {
                    _mDicAssetsDependBundles.Add(ireskey, new List<string>());

                }
                _mDicAssetsDependBundles[ireskey].Add(sbundlekey);
                if (!_mDicBundlescounts.ContainsKey(sbundlekey))
                {
                    _mDicBundlescounts.Add(sbundlekey, 0);
                }
                _mDicBundlescounts[sbundlekey]++;
            }
            internal static void _releaseAssetDependenceBundle(string sReskey)
            {
                if (_mDicAssetsDependBundles.ContainsKey(sReskey))
                {
                    List<string> depbundles = _mDicAssetsDependBundles[sReskey];
                    foreach (string depbundlekey in depbundles)
                    {
                        if (_mDicBundlescounts.ContainsKey(depbundlekey))
                        {
                            _mDicBundlescounts[depbundlekey]--;
                            if (_mDicBundlescounts[depbundlekey] == 0)
                            {
                                _mDicBundlescounts.Remove(depbundlekey);
                            }

                            
                        }
                    }
                    _mDicAssetsDependBundles.Remove(sReskey);
                }
            }
            internal static bool _getDepBundleUesed(string ibundlekey)
            {
                if (_mDicBundlescounts.ContainsKey(ibundlekey))
                {
                    return _mDicBundlescounts[ibundlekey] != 0;
                }
                else
                {
                    return false;
                }
            }
            
            
            private static void _OnLoadedLatestManifestForUpdate(object obj = null, eLoadedNotify loadedNotify = eLoadedNotify.Load_Successfull)
            {//加载完manifest
                if (loadedNotify == eLoadedNotify.Load_Successfull)
                {
                    ProcessDelegateArgc proc = (ProcessDelegateArgc)((Hashtable)obj)["proc"];
                    List<string> updateOnlyPacks = new List<string>((string[])((Hashtable)obj)["updateOnlyPack"]);
                    for (int i = 0; i < updateOnlyPacks.Count; i++)
                    {
                        updateOnlyPacks[i] = _getRealPath(updateOnlyPacks[i], typeof(AssetBundle), eLoadResPath.RP_URL).msRealPath;
                    }
                    if (mbuseassetbundle)
                    {
                        List<string> needUpdateBundleList = new List<string>();
                        List<eLoadResPath> needUpdateBundleResPathList = new List<eLoadResPath>();
                        if (_mURLAssetBundleManifest == null && mResourcesURLAddress != string.Empty)
                        {
                            _mURLAssetBundleManifest = (AssetBundleManifest)getRes(_getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_URL), typeof(AssetBundleManifest), eLoadResPath.RP_URL);
                        }
                        if (_mLocalAssetBundleManifest == null && mResourceStreamingAssets != string.Empty)
                        {
                            _mLocalAssetBundleManifest = (AssetBundleManifest)getRes(_getStreamingAssetsNameByLoadStyle(eLoadResPath.RP_StreamingAssets), typeof(AssetBundleManifest), eLoadResPath.RP_StreamingAssets);
                        }
                        AssetBundleManifest mainfest = _mURLAssetBundleManifest != null ? _mURLAssetBundleManifest : _mLocalAssetBundleManifest;
                        string[] bundles = mainfest.GetAllAssetBundles();
                        for (int i = 0; i < bundles.Length; i++)
                        {
                            CPathAndHash pathhash = _getRealPath(bundles[i], typeof(AssetBundle), eLoadResPath.RP_URL);

                            if (updateOnlyPacks.Contains(pathhash.msRealPath))
                            {//这里判断那些不需要获取的资源包(例如各个国家的语言包)
                                continue;
                            }

                            if (Caching.IsVersionCached(pathhash.msRealPath, pathhash.mHash) == false)
                            {
                                needUpdateBundleList.Add(bundles[i]);
                                needUpdateBundleResPathList.Add(pathhash.meLoadResType);
                            }
                        }
                        if (needUpdateBundleList.Count != 0)
                        {
                            System.Type[] types = new System.Type[needUpdateBundleList.Count];
                            for (int i = 0; i < needUpdateBundleList.Count; i++)
                            {
                                types[i] = typeof(AssetBundle);
                            }
                            _requestRes(needUpdateBundleList.ToArray(), types, needUpdateBundleResPathList.ToArray(), proc);
                        }
                        else
                        {
                            proc(null, eLoadedNotify.Load_Successfull);
                        }
                    }
                    else
                    {
                        proc(null, eLoadedNotify.Load_Successfull);
                    }
                }
            }
            //private static void OnLoadedUpdateAssetBundles(object obj = null, eLoadedNotify loadedNotify = eLoadedNotify.Load_Successfull)
            //{
            //    if (loadedNotify == eLoadedNotify.Load_Successfull)
            //    {
            //        ProcessDelegateArgc proc = (ProcessDelegateArgc)obj;
            //        proc(null, eLoadedNotify.Load_Successfull);
            //        clearAll();
            //    }
            //    else if (loadedNotify == eLoadedNotify.Load_OneSuccessfull)
            //    {
            //        ProcessDelegateArgc proc = (ProcessDelegateArgc)obj;
            //        proc(null, eLoadedNotify.Load_Successfull);
            //        clearAll();
            //    }
            //}
            private static void _loadDependenceBundles(string[] dependbundlepath, eLoadResPath[] eloadResTypes, ProcessDelegateArgc proc = null, object o = null, bool basyn = true)
            {
                System.Type[] types = new System.Type[dependbundlepath.Length];
                for (int i = 0; i < types.Length; i++)
                {
                    types[i] = typeof(AssetBundle);
                }
                CloadParam param = (CloadParam)o;
                _requestRes(dependbundlepath, types, eloadResTypes, proc, o, basyn,false, param.mbautoreleasebundle);
            }
            private static void _OnloadedDependenceBundles(object o, eLoadedNotify loadedNotify = eLoadedNotify.Load_Successfull)
            {
                if (loadedNotify == eLoadedNotify.Load_Successfull && o != null)
                {
                    CloadParam param = (CloadParam)o;
                    _requestRes(param.mpaths, param.mtypes, param.meloadResTypes, param.mproc, param.mo, param.mbasyn,false,param.mbautoreleasebundle);
                }
            }
            //加载资源组,指定每个资源的类型,资源都加载完会执行回调proc
            private static void _requestRes(string[] spaths, System.Type[] types, eLoadResPath[] eloadResTypes, ProcessDelegateArgc proc = null, object o = null, bool basyn = true, bool bNoUseCatching = false, bool bautoReleaseBundle = true)
            {

                //将请求的资源组加入正在加载的资源组列表,并返回资源组ID
                string sResGroupKey = _makeResGroupMap(spaths, types, eloadResTypes, proc, o);
                for (int i = 0; i < spaths.Length; i++)
                {
                    string truepath = _getRealPath(spaths[i], types[i], eloadResTypes[i]).msRealPath;
                    string sResKey = _getResKey(spaths[i], types[i], eloadResTypes[i]);
                    //如果资源组中的此个资源已经加载完毕(剔除资源组中已经加载完毕的资源)
                    if (_mDicLoadedRes.ContainsKey(sResKey))
                    {
                        //将该资源从资源组中移除
                        _removePathInResGroup(sResGroupKey, sResKey,true,bautoReleaseBundle);
                        continue;

                    }
                    if (mbuseassetbundle == true && eloadResTypes[i] != eLoadResPath.RP_Resources)
                    {//如果是打包读取,并且不直接从Resources读取

                        //异步读取资源

                        string temppath;
                        if (types[i] != typeof(AssetBundle))
                        {//不是只加载AssetBundle
                            string name = Path.GetFileNameWithoutExtension(spaths[i]);
                            string dir = spaths[i].Substring(0, spaths[i].Length - name.Length - 1);
                            temppath = dir + "|" + name;
                        }
                        else
                        {
                            temppath = spaths[i];
                        }
                        Hash128 hash;
                        _getRealLoadResPathType(temppath, eloadResTypes[i], out hash);
                        LoadAsset.getInstance().loadAsset(truepath, types[i], sResGroupKey, hash, basyn, bNoUseCatching, bautoReleaseBundle);

                        _mListLoadingRes.Add(sResKey);

                    }
                    else if(types[i] != typeof(AssetBundle))
                    {//否则直接读取Resources散开资源

                        Object t = null;

                        t = Resources.Load(truepath, types[i]);

                        if (t != null)
                        {
                            _mDicLoadedRes.Add(sResKey, t);
                            _removePathInResGroup(sResGroupKey, sResKey,true, bautoReleaseBundle);

                        }
                        else
                        {
                            _removePathInResGroup(sResGroupKey, sResKey, false, bautoReleaseBundle);
                            DLoger.LogError("Load===" + spaths[i] + "===Failed");
                        }
                        continue;
                    }
                    else
                    {
                        _removePathInResGroup(sResGroupKey, sResKey, true, bautoReleaseBundle);
                    }
                }
            }
            /// <summary>
            /// 获得已经加ResourcesManager.request加载完毕的对象
            /// </summary>
            /// <param name="respath">路径</param>
            /// <param name="type">类型</param>
            /// <param name="eloadResType">加载资源的路径类型</param>
            /// <returns></returns>
            public static Object getRes(string respath, System.Type type, eLoadResPath eloadResType = eLoadResPath.RP_URL)
            {
                string skey = _getResKey(respath, type, eloadResType);
                if (_mDicLoadedRes.ContainsKey(skey))
                {
                    return _mDicLoadedRes[skey];
                }
                else
                {
                    return null;
                }
            }
            /// <summary>
            /// 获得已经加ResourcesManager.request加载完毕的对象
            /// </summary>
            /// <param name="respath">路径</param>
            /// <param name="eloadResType">加载资源的路径类型</param>
            /// <returns></returns>
            public static Object getRes(string respath, eLoadResPath eloadResType = eLoadResPath.RP_URL)
            {
                return getRes(respath, typeof(Object), eloadResType);
            }
            /// <summary>
            /// 清空所有缓存资源
            /// </summary>
            public static void clearAll()
            {
                _mDicLoadedRes.Clear();
                Resources.UnloadUnusedAssets();
                System.GC.Collect();
            }
            /// <summary>
            /// 移除资源
            /// </summary>
            /// <param name="respath">资源路径</param>
            /// <param name="eloadResType">加载资源的路径类型</param>
            public static void removeRes(string respath, eLoadResPath eloadResType = eLoadResPath.RP_URL)
            {
                removeRes<Object>(respath, eloadResType);
            }
            /// <summary>
            /// 移除资源
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="respath"></param>
            /// <param name="eloadResType">加载资源的路径类型</param>
            public static void removeRes<T>(string respath, eLoadResPath eloadResType = eLoadResPath.RP_URL)
            {
                string skey = _getResKey(respath, typeof(T), eloadResType);
                if (_mDicLoadedRes.ContainsKey(skey))
                {
                    _mDicLoadedRes.Remove(skey);

                }
                Resources.UnloadUnusedAssets();

            }
            /// <summary>
            /// 移除资源,专门在LoadAsset里面移除bundle用
            /// </summary>
            /// <param name="sReskey"></param>
            internal static void _removeRes(string sReskey)
            {
                if (_mDicLoadedRes.ContainsKey(sReskey))
                {
                    _mDicLoadedRes.Remove(sReskey);

                }
                Resources.UnloadUnusedAssets();
            }
            /// <summary>
            /// 移除资源
            /// </summary>
            /// <param name="respath"></param>
            /// <param name="type"></param>
            /// <param name="eloadResType"></param>
            public static void removeRes(string respath,System.Type type, eLoadResPath eloadResType = eLoadResPath.RP_URL)
            {
                string skey = _getResKey(respath, type, eloadResType);
                if (_mDicLoadedRes.ContainsKey(skey))
                {
                    _mDicLoadedRes.Remove(skey);

                }
                Resources.UnloadUnusedAssets();

            }
            internal static void _addResAndRemoveInLoadingList(string skey, Object t)
            {
                if (!_mDicLoadedRes.ContainsKey(skey))
                {
                    _mDicLoadedRes.Add(skey, t);
                    _mListLoadingRes.Remove(skey);
                }

            }
            /// <summary>
            /// 是否是已经加载完毕的资源
            /// </summary>
            /// <param name="skey">(sAssetPath + type.ToString())</param>
            /// <returns></returns>
            internal static bool _isLoadedRes(string skey)
            {
                return _mDicLoadedRes.ContainsKey(skey);
            }
            internal static void _removeLoadingResFromList(string skey)
            {
                if (_mListLoadingRes.Contains(skey))
                {
                    _mListLoadingRes.Remove(skey);
                }

            }
            //获取最终的eloadrespath
            private static eLoadResPath _getRealLoadResPathType(string sAssetPath, eLoadResPath eloadResType, out Hash128 hash)
            {
                hash = new Hash128();
                eLoadResPath finalloadrespath;

                string assetsbundlepath;
                if (sAssetPath.Contains("|"))
                {
                    assetsbundlepath = sAssetPath.Split('|')[0];

                }
                else
                {//没有'|',表示只是加载assetbundle,不加载里面的资源(例如场景Level对象,依赖assetbundle)
                    assetsbundlepath = sAssetPath;
                }

                if (eloadResType == eLoadResPath.RP_Resources)
                {
                    finalloadrespath = eloadResType;
                }
                eLoadResPathState eloadrespathstate = _getLoadResPathState();
                if (eloadrespathstate == eLoadResPathState.LS_ReadURLOnly)
                {
                    if (_mURLAssetBundleManifest != null && !assetsbundlepath.Contains(mBundlesInfoFileName))
                    {
                        hash = _mURLAssetBundleManifest.GetAssetBundleHash(assetsbundlepath);
                    }
                    finalloadrespath = eLoadResPath.RP_URL;
                }
                else if (eloadrespathstate == eLoadResPathState.LS_ReadStreamingOnly)
                {
                    if (_mLocalAssetBundleManifest != null && !assetsbundlepath.Contains(mBundlesInfoFileName))
                    {
                        hash = _mLocalAssetBundleManifest.GetAssetBundleHash(assetsbundlepath);
                    }
                    finalloadrespath =  eLoadResPath.RP_StreamingAssets;
                }
                else if (eloadrespathstate == eLoadResPathState.LS_ReadURLForUpdate && eloadResType == eLoadResPath.RP_StreamingAssets)
                {
                    if (_mLocalAssetBundleManifest != null && !assetsbundlepath.Contains(mBundlesInfoFileName))
                    {
                        hash = _mLocalAssetBundleManifest.GetAssetBundleHash(assetsbundlepath);
                    }
                    finalloadrespath = eLoadResPath.RP_StreamingAssets;
                }
                else if (eloadrespathstate == eLoadResPathState.LS_ReadURLForUpdate && eloadResType == eLoadResPath.RP_URL)
                {
                    if (_mLocalAssetBundleManifest == null || _mURLAssetBundleManifest == null)
                    {//说明还没有加载manifest,无须比较,按照设定的来加载
                        finalloadrespath = eloadResType;
                    }
                    else
                    {
                        
                        if (assetsbundlepath.Contains(mBundlesInfoFileName))
                        {
                            return  eloadResType;
                        }
                        Hash128 hashlocal = _mLocalAssetBundleManifest.GetAssetBundleHash(assetsbundlepath);
                        Hash128 hashurl = _mURLAssetBundleManifest.GetAssetBundleHash(assetsbundlepath);
                        if (hashlocal.GetHashCode() == hashurl.GetHashCode())
                        {

                            hash = hashlocal;
                            finalloadrespath = eLoadResPath.RP_StreamingAssets;

                        }
                        else
                        {

                            hash = hashurl;
                            finalloadrespath = eLoadResPath.RP_URL;

                        }
                    }
                }
                else
                {
                    DLoger.LogError("you request a assetsbundle from  error Paths!");
                    finalloadrespath = eLoadResPath.RP_Resources;
                }
                return finalloadrespath;
            }

            //获取资源的真实路径
            private static CPathAndHash _getRealPath(string respath, System.Type type, eLoadResPath eloadResType)
            {
                CPathAndHash pathhash = new CPathAndHash();
                if (mbuseassetbundle == false || eloadResType == eLoadResPath.RP_Resources)
                {
                    pathhash.msRealPath = respath;
                    pathhash.meLoadResType = eLoadResPath.RP_Resources;
                }
                else
                {

                    string temppath;
                    if (type != typeof(AssetBundle))
                    {//不是只加载AssetBundle
                        string name = Path.GetFileNameWithoutExtension(respath);
                        string dir = respath.Substring(0, respath.Length - name.Length - 1);
                        temppath = dir + "|" + name;
                    }
                    else
                    {
                        temppath = respath;
                    }
                    pathhash.meLoadResType = _getRealLoadResPathType(temppath, eloadResType, out pathhash.mHash);
                    string address = _getResAddressByPath(pathhash.meLoadResType);
                    pathhash.msRealPath = address + temppath;
                }
                return pathhash;
            }
            //获得资源的资源ID
            private static string _getResKey(string respath, System.Type type, eLoadResPath eloadResType)
            {
                return (_getRealPath(respath, type, eloadResType).msRealPath + type.ToString());
            }
            //将资源ID从正在加载的资源组中移除,并判断是否资源组全部加载完,全部加载完毕,执行回调
            internal static void _removePathInResGroup(string sReseskey, string sReskey, bool bsuccessful,bool bautorelease)
            {
                CResesState rs;
                if (_mDicLoadingResesGroup.ContainsKey(sReseskey))
                {
                    rs = _mDicLoadingResesGroup[sReseskey];
                    int index = rs.mlistpathskey.FindIndex(0, delegate (string s) { return s == sReskey; });
                    string path = "";
                    if (index != -1)
                    {
                        path = rs.mlistpaths[index];
                        rs.mlistpaths.Remove(path);
                        //DLoger.Log("加载====" + path + "=====完毕!" + bsuccessful.ToString());
                        rs.mlistpathskey.Remove(sReskey);
                    }

                    if (rs.mlistpathskey.Count == 0)
                    {
                        eLoadedNotify eloadnotify = bsuccessful == true ? eLoadedNotify.Load_OneSuccessfull : eLoadedNotify.Load_Failed;
                        for (int i = 0; i < rs.listproc.Count; i++)
                        {
                            Hashtable loadedinfo = new Hashtable();
                            loadedinfo.Add("path", path);
                            loadedinfo.Add("loaded", rs.maxpaths - rs.mlistpathskey.Count);
                            loadedinfo.Add("max", rs.maxpaths);
                            rs.listproc[i](eloadnotify == eLoadedNotify.Load_Failed? rs.listobj[i] : loadedinfo, eloadnotify);
                        }
                        eloadnotify = bsuccessful == true ? eLoadedNotify.Load_Successfull : eLoadedNotify.Load_Failed;
                        _mDicLoadingResesGroup.Remove(sReseskey);
                        for (int i = 0; i < rs.listproc.Count; i++)
                        {
                            rs.listproc[i](rs.listobj[i], eloadnotify);
                        }
                        rs.listproc.Clear();
                        rs.listobj.Clear();
                        
                    }
                    else
                    {
                        eLoadedNotify eloadnotify = bsuccessful == true ? eLoadedNotify.Load_OneSuccessfull : eLoadedNotify.Load_Failed;
                        for (int i = 0; i < rs.listproc.Count; i++)
                        {
                            Hashtable loadedinfo = new Hashtable();
                            loadedinfo.Add("path", path);
                            loadedinfo.Add("loaded", rs.maxpaths - rs.mlistpathskey.Count);
                            loadedinfo.Add("max", rs.maxpaths);
                            rs.listproc[i](eloadnotify == eLoadedNotify.Load_Failed ? rs.listobj[i] : loadedinfo, eloadnotify);
                        }
                    }
                    if (bautorelease)
                    {
                        _releaseAssetDependenceBundle(sReskey);
                    }
                }

            }
            //资源组产生一个资源组的唯一ID
            private static string _makeResGroupMap(string[] spaths, System.Type[] types, eLoadResPath[] eloadResTypes, ProcessDelegateArgc proc, object o)
            {//将资源组和回调函数记录

                CResesState rs;
                string skey;
                _getResGroupMapKey(spaths, types, eloadResTypes, out rs, out skey);
                if (!_mDicLoadingResesGroup.ContainsKey(skey))
                {
                    if (proc != null)
                    {
                        rs.listproc.Add(proc);
                        rs.listobj.Add(o);
                    }
                    _mDicLoadingResesGroup.Add(skey, rs);
                }
                else
                {
                    if (proc != null)
                    {
                        _mDicLoadingResesGroup[skey].listproc.Add(proc);
                        _mDicLoadingResesGroup[skey].listobj.Add(o);
                    }
                }
                return skey;
            }
            private static void _getResGroupMapKey(string[] spaths, System.Type[] types, eLoadResPath[] eloadResTypes, out CResesState resstate, out string skey)
            {
                CResesState rs = new CResesState();
                string paths = "";
                rs.maxpaths = spaths.Length;
                for (int i = 0; i < spaths.Length; i++)
                {
                    string pathkey = _getResKey(spaths[i], types[i], eloadResTypes[i]);
                    string truepath = _getRealPath(spaths[i], types[i], eloadResTypes[i]).msRealPath;
                    rs.mlistpathskey.Add(pathkey);
                    rs.mlistpaths.Add(truepath);
                    paths += truepath;
                }
                resstate = rs;
                skey = paths;
            }
            internal static string _getResAddressByPath(eLoadResPath eloadResType)
            {
                if (eloadResType == eLoadResPath.RP_StreamingAssets)
                {
                    return mResourceStreamingAssets;
                }
                else
                {
                    return mResourcesURLAddress;
                }
            }
            internal static AssetBundleManifest _getAssetBundleManifest(eLoadResPath eloadResType)
            {
                if (eloadResType == eLoadResPath.RP_StreamingAssets)
                {
                    return _mLocalAssetBundleManifest;
                }
                else
                {
                    return _mURLAssetBundleManifest;
                }
            }
            internal static eLoadResPathState _getLoadResPathState()
            {
                if (mResourcesURLAddress != string.Empty && mResourceStreamingAssets != string.Empty)
                {
                    return eLoadResPathState.LS_ReadURLForUpdate;
                }
                else if ((mResourcesURLAddress != string.Empty && mResourceStreamingAssets == string.Empty)
                        || (mResourcesURLAddress != string.Empty && mResourceStreamingAssets != string.Empty))
                {
                    return eLoadResPathState.LS_ReadURLOnly;
                }
                else if (mResourcesURLAddress == string.Empty && mResourceStreamingAssets != string.Empty)
                {
                    return eLoadResPathState.LS_ReadStreamingOnly;
                }
                else
                {
                    return eLoadResPathState.LS_EmptyAddress;
                }
            }
            /// <summary>
            /// 是否读取打包资源
            /// </summary>
            public static bool mbuseassetbundle = true;
            /// <summary>
            /// 指定资源的网络路径
            /// </summary>
            public static string mResourcesURLAddress = string.Empty;
            /// <summary>
            /// 指定资源的网络路径
            /// </summary>
            public static string mResourceStreamingAssets = string.Empty;
            /// <summary>
            /// 是否开启自动释放
            /// </summary>
            //public static bool mBAutoRelease = true;
            /// <summary>
            /// 设定记录bundle信息的文件名
            /// </summary>
            private static string mBundlesInfoFileName = "StreamingAssets";
            
            /// <summary>
            /// AssetBundleManifest对象
            /// </summary>
            internal static AssetBundleManifest _mURLAssetBundleManifest = null;
            internal static AssetBundleManifest _mLocalAssetBundleManifest = null;

            private static Dictionary<string, Object> _mDicLoadedRes = new Dictionary<string, Object>();
            private static List<string> _mListLoadingRes = new List<string>();
            private static Dictionary<string, CResesState> _mDicLoadingResesGroup = new Dictionary<string, CResesState>();
            /// <summary>
            /// 记录依赖包的引用和计数
            /// </summary>
            private static Dictionary<string, List<string>> _mDicAssetsDependBundles = new Dictionary<string, List<string>>();
            private static Dictionary<string, int> _mDicBundlescounts = new Dictionary<string, int>();
            ///
            

        }
        /// <summary>
        /// 加载结果
        /// </summary>
        public enum eLoadedNotify
        {
            /// <summary>
            /// 加载失败
            /// </summary>
            Load_Failed,
            /// <summary>
            /// 加载一个成功
            /// </summary>
            Load_OneSuccessfull,
            /// <summary>
            /// 加载全部完成
            /// </summary>
            Load_Successfull
        }
        /// <summary>
        /// 枚举3种读取资源的来源
        /// </summary>
        public enum eLoadResPath
        {
            /// <summary>
            /// 读取Resources下的资源
            /// </summary>
            RP_Resources,
            /// <summary>
            /// 读取本地StreamingAssets下的资源
            /// </summary>
            RP_StreamingAssets,
            /// <summary>
            /// 读取网络路径
            /// </summary>
            RP_URL,
            /// <summary>
            /// 未知
            /// </summary>
            RP_Unknow

        }
        internal enum eLoadResPathState
        {
            /// <summary>
            /// 只读取URL的资源
            /// </summary>
            LS_ReadURLOnly,
            /// <summary>
            /// 只读取StreamingAssets的资源
            /// </summary>
            LS_ReadStreamingOnly,
            /// <summary>
            /// 优先读取URL的资源
            /// </summary>
            LS_ReadURLForUpdate,
            /// <summary>
            /// 没有配置资源路径
            /// </summary>
            LS_EmptyAddress,
        }
        internal class CPathAndHash
        {
            public string msRealPath;
            public eLoadResPath meLoadResType;
            public Hash128 mHash;
        }

        //资源组管理
        internal class CResesState
        {
            public List<string> mlistpathskey = new List<string>();
            public List<string> mlistpaths = new List<string>();
            public List<ProcessDelegateArgc> listproc = new List<ProcessDelegateArgc>();
            public List<object> listobj = new List<object>();
            public int maxpaths = 0;
        }
        //加载参数
        internal class CloadParam
        {
            public string[] mpaths = null;
            public System.Type[] mtypes = null;
            public eLoadResPath[] meloadResTypes = null;
            public ProcessDelegateArgc mproc = null;
            public object mo = null;
            public bool mbasyn = true;
            public bool mbautoreleasebundle = true;

            public CloadParam(string spath, System.Type type, eLoadResPath eloadResType = eLoadResPath.RP_URL, ProcessDelegateArgc proc = null, object o = null, bool basyn = true, bool bautoreleasebundle = true)
            {
                string[] spaths = new string[1];
                System.Type[] types = new System.Type[1];
                eLoadResPath[] eloadResTypes = new eLoadResPath[1];
                spaths[0] = spath;
                types[0] = type;
                eloadResTypes[0] = eloadResType;

                mpaths = spaths;
                mtypes = types;
                mproc = proc;
                mo = o;
                mbasyn = basyn;
                mbautoreleasebundle = bautoreleasebundle;
                meloadResTypes = eloadResTypes;
            }
            public CloadParam(string[] spaths, System.Type[] types, eLoadResPath[] eloadResTypes, ProcessDelegateArgc proc = null, object o = null, bool basyn = true, bool bautoreleasebundle = true)
            {
                mpaths = spaths;
                mtypes = types;
                mproc = proc;
                mo = o;
                mbasyn = basyn;
                mbautoreleasebundle = bautoreleasebundle;
                meloadResTypes = eloadResTypes;
            }
        }
    }

}

