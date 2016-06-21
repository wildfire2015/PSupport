using PSupport;
using UnityEngine;
using UnityEngine.UI;
/*******************************************************************************
* 
*             类名: UIBase
*             功能: UI系统UI对象基类
*             作者: 彭谦
*             日期: 2015.12.7
*             修改:
*             备注:所有UI对象都继承自该类,才能由UIManager来管理,派生类必须实现onLoadedUIObj接口
*                 和refresh接口
*             
* *****************************************************************************/
namespace UISystem
{

    public abstract class UIBase : MonoBehaviour
    {
        //UI显示的GameObject
        public GameObject mUIShowObj = null;

        protected Canvas _mCanvas = null;
        protected CanvasScaler _mCanvasScaler = null;

        //排序
        protected int _miUIOder = 0;
        //原始排序
        protected int _miOriginOder = 0;

        protected bool _mbShowUILoaded = false;



        internal void _onLoadedShowUI()
        {
            _mbShowUILoaded = true;
            _mCanvas = mUIShowObj.GetComponent<Canvas>();
            _mCanvasScaler = mUIShowObj.GetComponent<CanvasScaler>();
            if (_mCanvas != null)
            {
                _miOriginOder = _mCanvas.sortingOrder;
                setCanvasOder(_miUIOder);
                _mCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                _mCanvas.worldCamera = UIManager.mUICamera;
                _mCanvas.targetDisplay = UIManager.mUICamera.targetDisplay;
                _mCanvas.planeDistance = UIManager.mUICamera.farClipPlane;
                _mCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                _mCanvasScaler.matchWidthOrHeight = 0;
                _mCanvasScaler.referenceResolution = UIManager.mreferenceResolution;
            }
        }

        //显示UIprefab加载完毕后调用,所有控件对象的初始化赋值在这里进行
        public abstract void onAutoLoadedUIObj();
        public abstract void onLoadedUIObj();
        //UI对象的数据刷新
        public abstract void refresh();
        public abstract void onAutoRelease();
        public abstract void onRelease();
        public bool bShowUILoaded
        {
            get { return _mbShowUILoaded; }
        }
        public void recoverOder()
        {
            setCanvasOder(0);
        }
        public void setCanvasOder(int ioder)
        {
            _miUIOder = ioder;

            if (_mCanvas != null)
            {
                _mCanvas.sortingOrder = _miOriginOder + _miUIOder;
            }
        }

        //一些功能函数
        public string getTextColorString(string text, string color = "ffffff")
        {
            return "<color=#" + color + ">" + text + "</color>";
        }
        public void setRectTransformByPosSize(RectTransform setrect, Vector2 pos, Vector2 size)
        {
            int screenw = (int)UIManager.mreferenceResolution.x;
            int screenh = (int)UIManager.mreferenceResolution.y;

            int posx = (int)pos.x;
            int posy = (int)pos.y;

            int sizex = (int)size.x;
            int sizey = (int)size.y;

            float pivx = setrect.pivot.x;
            float pivy = setrect.pivot.y;

            Vector2 offsetmin = new Vector2(screenw / 2.0f + (posx - sizex * pivx), screenh / 2.0f + (posy - sizey * pivy));
            Vector2 offsetmax = new Vector2(-screenw / 2.0f + (posx + sizex *(1 - pivx)), -screenh / 2.0f + (posy + sizey * (1 - pivy)));

            

            setRectTransformByOffset(setrect, offsetmin, offsetmax);



        }
        public void setRectTransformByOffset(RectTransform setrect, Vector2 offsetmin, Vector2 offsetmax)
        {

            int screenw = (int)UIManager.mreferenceResolution.x;
            int screenh = (int)UIManager.mreferenceResolution.y;

            int minx = (int)offsetmin.x;
            int miny = (int)offsetmin.y;

            int maxx = (int)offsetmax.x;
            int maxy = (int)offsetmax.y;

            Vector2 offsetMin = new Vector2((minx - setrect.anchorMin.x * screenw) , (miny - setrect.anchorMin.y * screenh));
            Vector2 offsetMax = new Vector2((maxx + (1 - setrect.anchorMax.x) * screenw) , (maxy + (1 - setrect.anchorMax.y) * screenh));

            setrect.offsetMin = offsetMin;
            setrect.offsetMax = offsetMax;
        }
    }
}



