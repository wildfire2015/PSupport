using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.EventSystems;
using PSupport;
using PSupport.LoadSystem;

/*******************************************************************************
* 
*             类名: UIManager
*             功能: UI系统UI对象管理器
*             作者: 彭谦
*             日期: 2015.12.7
*             修改:
*             备注:管理UI对象的创建,和最高层UI之间的互相调用其方法
*                 直接加载根节点下的UI对象由管理器统一管理,直接创建的对象由各自创建的系统自己管理
*                 
*             
* *****************************************************************************/

namespace UISystem
{
    /// <summary>
    /// UI层级枚举
    /// </summary>
    public enum eOderLayer
    {
        eOderBottom = 0,
        eOderMid = 100,
        eOderTop = 200,
        eOderTopest = 300,
        eOderDebugLog = 1000,
    }
    /// <summary>
    /// UI管理器类
    /// </summary>
    public class UIManager
    {
        private UIManager() { }
        //UI根节点
        private static GameObject _mUIRoot = null;
        
        //根节点下UI管理器
        private static Dictionary<string, UIBase> _mdicUIUnderRoot = new Dictionary<string, UIBase>();
        //执行UI的Func异步机制记录
        private static Dictionary<UIBase, string> _mdicUIFuncCache = new Dictionary<UIBase, string>();

        //UI摄像机
        public static Camera mUICamera = null;
        public static Vector2 mreferenceResolution = new Vector2(640, 1136);


        /// <summary>
        /// 创建唯一UI,自动加入到UIRoot下,并通过回调返回创建的对象
        /// </summary>
        /// <typeparam name="T">UI类名</typeparam>
        /// <param name="path">路径</param>
        /// <param name="eLoadResType">资源位置</param>

        public static void addUIToRoot<T>(string spath, eLoadResPath eLoadResType = eLoadResPath.RP_URL) where T : UIBase, new()
        {
            if (_mUIRoot == null)
            {
                //保证场景中只有一个UIRoot下的EventSystem
                EventSystem[] evsys = Object.FindObjectsOfType<EventSystem>();
                foreach (EventSystem ev in evsys)
                {
                    Object.DestroyImmediate(ev.gameObject);
                }
                ResourceLoadManager.requestRes("local/ui/UIRoot", typeof(Object),eLoadResPath.RP_Resources, null, null, true);
                _mUIRoot = Object.Instantiate(ResourceLoadManager.getRes("local/ui/UIRoot", eLoadResPath.RP_Resources)) as GameObject;
                _mUIRoot.name = "UIRoot";
                Object.DontDestroyOnLoad(_mUIRoot);

                mUICamera = _mUIRoot.transform.Find("UICamera").GetComponent<Camera>();

            }
            if (_getUIUnderRoot<T>() == null)
            {//如果没有此UI

                GameObject obj = new GameObject(typeof(T).Name);
                T t = obj.AddComponent<T>();
                obj.transform.SetParent(_mUIRoot.transform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = Vector3.one;
                _registUI(typeof(T).Name, t);
                obj.SetActive(false);

                ResourceLoadManager.requestRes(spath, typeof(Object), eLoadResType, _onLoadedUIAddToRoot, spath + '#' + (int)eLoadResType, true);
            }


        }
        /// <summary>
        /// 调用UI对象的函数
        /// </summary>
        /// <typeparam name="T">UI类名</typeparam>
        /// <param name="funname">函数名</param>
        public static void setUIDoFunc<T>(string funname) where T : UIBase, new()
        {
            T ui = _getUIUnderRoot<T>();
            if (ui != null)
            {
                if (_mdicUIFuncCache.ContainsKey(ui))
                {
                    _mdicUIFuncCache[ui] = funname;
                }
                else
                {
                    _mdicUIFuncCache.Add(ui, funname);
                }
                if (ui.bShowUILoaded)
                {
                    ui.Invoke(funname, 0);
                    _mdicUIFuncCache.Remove(ui);
                }


            }
            else
            {
                DLoger.LogError("setUIDoFunc failed! ==" + typeof(T).Name + "== not found!");
            }

        }
        /// <summary>
        /// 调用UI对象的refresh
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void refreshUI<T>() where T : UIBase, new()
        {
            setUIDoFunc<T>("refresh");
        }

        //打开UI,并设置是否打开在最前面,是否关闭其他UI
        public static void openUIInRoot<T>(bool bcloseOtherUI = false, eOderLayer eOder = eOderLayer.eOderMid, int iaddoder = 0) where T : UIBase, new()
        {
            T ui = _getUIUnderRoot<T>();
            if (ui != null)
            {
                ui.gameObject.SetActive(true);
                ui.setCanvasOder((int)eOder + iaddoder);
                if (bcloseOtherUI)
                {
                    foreach (string key in _mdicUIUnderRoot.Keys)
                    {
                        if (key != ui.name)
                        {
                            _mdicUIUnderRoot[key].gameObject.SetActive(false);
                        }
                    }
                }
            }
            else
            {
                DLoger.LogError("openUI failed! ==" + typeof(T).Name + "== not found!");
            }
        }
        /// <summary>
        /// 关闭根节点下的UI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void closeUIInRoot<T>() where T : UIBase, new()
        {
            T ui = _getUIUnderRoot<T>();
            if (ui != null)
            {
                ui.gameObject.SetActive(false);
                ui.recoverOder();
            }
            else
            {
                DLoger.LogError("closeUI failed! ==" + typeof(T).Name + "== not found!");
            }
        }
        /// <summary>
        /// 获取根节点
        /// </summary>
        public GameObject UIRoot
        {
            get { return _mUIRoot; }
        }
        /// <summary>
        /// 移除静态UI
        /// </summary>
        /// <param name="uiname"></param>
        public static void removeUIFromUIRoot(string uiname)
        {
            if (_mdicUIUnderRoot.ContainsKey(uiname))
            {
                _mdicUIUnderRoot[uiname].onAutoRelease();
                Object.DestroyImmediate(_mdicUIUnderRoot[uiname].gameObject);
                _mdicUIUnderRoot.Remove(uiname);
            }
        }
        /// <summary>
        /// 删除根节点下所有的UI
        /// </summary>
        public static void removeAllUIUnderRoot()
        {
            foreach (UIBase uiobj in _mdicUIUnderRoot.Values)
            {
                uiobj.onAutoRelease();
                Object.DestroyImmediate(uiobj.gameObject);
            }
            _mdicUIUnderRoot.Clear();
        }
        /// <summary>
        /// 创建一个UI,但是并不管理该UI的对象,需要得到它的模块自己管理,用于公共动态UI的创建
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="eLoadResType"></param>
        /// <returns></returns>
        public static T createUI<T>(string path, eLoadResPath eLoadResType = eLoadResPath.RP_URL) where T : UIBase, new()
        {
            GameObject obj = new GameObject(path);
            T t = obj.AddComponent<T>();
            ResourceLoadManager.requestRes(path, typeof(Object), eLoadResType, _OnLoadedUI, new KeyValuePair<UIBase, eLoadResPath>(t, eLoadResType), true);
            return t;
        }
        private static T _getUIUnderRoot<T>() where T : UIBase, new()
        {
            UIBase uiobj = null;
            _mdicUIUnderRoot.TryGetValue(typeof(T).Name, out uiobj);
            return uiobj as T;
        }
        private static void _registUI(string uiname, UIBase uiobj)
        {
            removeUIFromUIRoot(uiname);
            _mdicUIUnderRoot.Add(uiname, uiobj);
        }
        private static void _onLoadedUIAddToRoot(object o, eLoadedNotify loadedNotify)
        {
            
            if (loadedNotify == eLoadedNotify.Load_Successfull)
            {
                string path = (o as string).Split('#')[0];
                string temp = (o as string).Split('#')[1];
                eLoadResPath eLoadResType = (eLoadResPath)int.Parse(temp);

                GameObject uishowobj = Object.Instantiate(ResourceLoadManager.getRes(path, eLoadResType)) as GameObject;
                string uiobjkeyname = Path.GetFileNameWithoutExtension(path);
                uishowobj.name = uiobjkeyname + "(Show)";
                UIBase ui = _mdicUIUnderRoot[uiobjkeyname];
                GameObject logicUIObj = ui.gameObject;
                ui.mUIShowObj = uishowobj;
                uishowobj.transform.SetParent(logicUIObj.transform);
                ui._onLoadedShowUI();
                ui.onAutoLoadedUIObj();
                ui.refresh();
                //读完UI的ShowObj,调用之前设置要执行的函数
                if (_mdicUIFuncCache.ContainsKey(ui))
                {
                    ui.Invoke(_mdicUIFuncCache[ui], 0);
                    _mdicUIFuncCache.Remove(ui);
                }


            }
            else if (loadedNotify == eLoadedNotify.Load_Failed)
            {
                string path = (o as string).Split('#')[0];
                DLoger.LogError(path + " load failed!");
            }
        }



        private static void _OnLoadedUI(object o, eLoadedNotify loadedNotify)
        {
            
            if (loadedNotify == eLoadedNotify.Load_Successfull)
            {

                UIBase ui = ((KeyValuePair<UIBase, eLoadResPath>)o).Key;
                eLoadResPath eLoadResType = ((KeyValuePair<UIBase, eLoadResPath>)o).Value;
                GameObject logicuiobj = ui.gameObject;
                string path = logicuiobj.name;
                logicuiobj.name = Path.GetFileNameWithoutExtension(path);
                GameObject showuiobj = Object.Instantiate(ResourceLoadManager.getRes(path, eLoadResType)) as GameObject;
                showuiobj.name = logicuiobj.name;
                ui.mUIShowObj = showuiobj;
                showuiobj.transform.SetParent(logicuiobj.transform);
                showuiobj.transform.localPosition = Vector3.zero;
                showuiobj.transform.localScale = Vector3.one;
                ui._onLoadedShowUI();
                ui.onAutoLoadedUIObj();
                ui.refresh();
            }
            else if (loadedNotify == eLoadedNotify.Load_Failed)
            {
                UIBase ui = ((KeyValuePair<UIBase, eLoadResPath>)o).Key;
                GameObject logicuiobj = ui.gameObject;
                DLoger.LogError(logicuiobj.name + " load failed!");
            }
        }
    }
}



