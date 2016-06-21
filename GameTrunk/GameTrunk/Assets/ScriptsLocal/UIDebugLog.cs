using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
/// <summary>
/// 在游戏中的信息输出窗口
/// </summary>
public class UIDebugLog : MonoBehaviour
{
    GameObject Obj_ShowArea = null;
    Canvas _mCanvas = null;
    CanvasScaler _mCanvasScaler = null;
    Button Btn_DebugWin = null;
    Button Btn_ClearWin = null;
    Slider Sld_slider = null;
    Text Txt_Contant = null;
    Toggle Tog_Stack = null;
    Toggle Tog_Normal = null;
    Toggle Tog_Warning = null;
    Toggle Tog_Error = null;
    Text TogText_Normal = null;
    Text TogText_Warning = null;
    Text TogText_Error = null;
    RectTransform _mRectShowAreaTransform = null;
    List<LogType> _mlistlogtype = new List<LogType>();
    List<string> _mlistLog = new List<string>();
    List<string> _mlistStack = new List<string>();
    

    int maxline = 15;
    
    void Start()
    {
        Obj_ShowArea = gameObject.transform.Find("Obj_ShowArea").gameObject;
        Btn_DebugWin = gameObject.transform.Find("Btn_DebugWin").gameObject.GetComponent<Button>();
        Btn_DebugWin.onClick.AddListener(onClickBtnDebugWin);
        Btn_ClearWin = gameObject.transform.Find("Btn_ClearWin").gameObject.GetComponent<Button>();
        Btn_ClearWin.interactable = Obj_ShowArea.activeSelf;
        Btn_ClearWin.onClick.AddListener(onClickBtnClearWin);
        Txt_Contant = gameObject.transform.Find("Obj_ShowArea/Viewport/Txt_Contant").gameObject.GetComponent<Text>();
        Tog_Stack = gameObject.transform.Find("Tog_Stack").gameObject.GetComponent<Toggle>();
        Tog_Normal = gameObject.transform.Find("Tog_NormalLog").gameObject.GetComponent<Toggle>();
        TogText_Normal = Tog_Normal.transform.Find("Label").gameObject.GetComponent<Text>();
        Tog_Warning = gameObject.transform.Find("Tog_WarningLog").gameObject.GetComponent<Toggle>();
        TogText_Warning = Tog_Warning.transform.Find("Label").gameObject.GetComponent<Text>();
        Tog_Error = gameObject.transform.Find("Tog_ErrorLog").gameObject.GetComponent<Toggle>();
        TogText_Error = Tog_Error.transform.Find("Label").gameObject.GetComponent<Text>();
        Tog_Stack.interactable = Obj_ShowArea.activeSelf;
        Tog_Stack.isOn = false;
        Tog_Stack.onValueChanged.AddListener(onClickCheckStack);
        Tog_Normal.onValueChanged.AddListener(onClickCheckStack);
        Tog_Warning.onValueChanged.AddListener(onClickCheckStack);
        Tog_Error.onValueChanged.AddListener(onClickCheckStack);
        Txt_Contant.text = "";
        Sld_slider = gameObject.transform.Find("Obj_ShowArea/Slider").gameObject.GetComponent<Slider>();
        Sld_slider.wholeNumbers = true;
        Sld_slider.minValue = 0;
        Sld_slider.onValueChanged.AddListener(onSliderValueChanged);

        _mCanvas = gameObject.GetComponent<Canvas>();
        _mCanvasScaler = gameObject.GetComponent<CanvasScaler>();
        if (_mCanvas != null)
        {
            _mCanvas.sortingOrder = 1000;
            _mCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _mCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _mCanvasScaler.matchWidthOrHeight = 0;
            _mCanvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);
        }
        _mRectShowAreaTransform = Obj_ShowArea.GetComponent<RectTransform>();

        Application.logMessageReceived += HandleLog;
        refresh();
    }

    void onSliderValueChanged(float fvalue)
    {
        refresh();
    }
  
    void refresh()
    {

        //如果显示区域开启,则刷新输出窗口内容
        if (Obj_ShowArea && Obj_ShowArea.activeSelf == true)
        {
            Sld_slider.maxValue = Mathf.Max(_mlistLog.Count - 1,0);
            Txt_Contant.text = "";
            for(int i = (int)Sld_slider.value,line = 0; i < _mlistLog.Count;i++)
            {
                if(_mlistlogtype[i] == LogType.Log && Tog_Normal.isOn 
                    || _mlistlogtype[i] == LogType.Warning && Tog_Warning.isOn
                    || _mlistlogtype[i] == LogType.Error && Tog_Error.isOn)
                {
                    Txt_Contant.text += i + "\n";
                    Txt_Contant.text += _mlistLog[i] + "\n";
                    if (Tog_Stack.isOn)
                    {
                        Txt_Contant.text += _mlistStack[i];
                    }
                    Txt_Contant.text += "-------------------------" + "\n";
                    if (line > maxline)
                    {
                        break;
                    }
                    line++;
                }
                
            }
            TogText_Normal.text = _mlistlogtype.FindAll((type) =>
            {
                return type == LogType.Log;
            }).Count.ToString();
            TogText_Warning.text = _mlistlogtype.FindAll((type) =>
            {
                return type == LogType.Warning;
            }).Count.ToString();
            TogText_Error.text = _mlistlogtype.FindAll((type) =>
            {
                return type == LogType.Error;
            }).Count.ToString();
        }
        
        
    }
    void OnEnable()
    {
        refresh();
    }
    void OnDestroy()
    {
        release();
    }
    void release()
    {
        Application.logMessageReceived -= HandleLog;
        Obj_ShowArea = null;
        Btn_DebugWin = null;
        Txt_Contant = null;
        Tog_Stack = null;
        _mlistLog.Clear();
        _mlistStack.Clear();
    }
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        
        string color = "ffffff";
        switch(type)
        {
            case LogType.Log:
                color = "ffffff";
                break;
            case LogType.Error:
            case LogType.Exception:
            case LogType.Assert:
                color = "ff0000";
                break;
            case LogType.Warning:
                color = "ffff00";
                break;
        }
        //这里把回调过来的英文半角空格换成全角空格,不然会自动换行,不知道为什么,可能是UGUI的BUG
        string log = logString.Replace((char)32, (char)12288);
        string stack = stackTrace.Replace((char)32, (char)12288);
        _mlistLog.Add(getTextColorString(log, color));
        _mlistStack.Add(getTextColorString(stack, color));
        _mlistlogtype.Add(type);
        refresh();


    }
    private void onClickBtnDebugWin()
    {
        Obj_ShowArea.SetActive(!Obj_ShowArea.activeSelf);
        Tog_Stack.interactable = Obj_ShowArea.activeSelf;
        Btn_ClearWin.interactable = Obj_ShowArea.activeSelf;
    }
    private void onClickBtnClearWin()
    {
        _mlistLog.Clear();
        _mlistStack.Clear();
        refresh();
    }
    private void onClickCheckStack(bool bcheck)
    {
        //Vector2 outputwinpos;
        //Vector2 outputwinsize;
        //if (bcheck)
        //{
        //    outputwinpos = new Vector2(0, _mRectShowAreaTransform.localPosition.y);
        //    outputwinsize = new Vector2(Screen.width, Screen.height - 20);
        //}
        //else
        //{
        //    outputwinpos = new Vector2(-Screen.width/4, _mRectShowAreaTransform.localPosition.y);
        //    outputwinsize = new Vector2(Screen.width/2, Screen.height - 20);
        //}
        //setRectTransformByPosSize(_mRectShowAreaTransform, outputwinpos, outputwinsize);
        refresh();
    }

    //一些功能函数
    string getTextColorString(string text, string color = "ffffff")
    {
        return "<color=#" + color + ">" + text + "</color>";
    }
    void setRectTransformByPosSize(RectTransform setrect, Vector2 pos, Vector2 size)
    {
        int screenw = (int)Screen.width;
        int screenh = (int)Screen.height;

        int posx = (int)pos.x;
        int posy = (int)pos.y;

        int sizex = (int)size.x;
        int sizey = (int)size.y;

        float pivx = setrect.pivot.x;
        float pivy = setrect.pivot.y;

        Vector2 offsetmin = new Vector2(screenw / 2.0f + (posx - sizex * pivx), screenh / 2.0f + (posy - sizey * pivy));
        Vector2 offsetmax = new Vector2(-screenw / 2.0f + (posx + sizex * (1 - pivx)), -screenh / 2.0f + (posy + sizey * (1 - pivy)));



        setRectTransformByOffset(setrect, offsetmin, offsetmax);



    }
    void setRectTransformByOffset(RectTransform setrect, Vector2 offsetmin, Vector2 offsetmax)
    {

        int screenw = (int)Screen.width;
        int screenh = (int)Screen.height;

        int minx = (int)offsetmin.x;
        int miny = (int)offsetmin.y;

        int maxx = (int)offsetmax.x;
        int maxy = (int)offsetmax.y;

        Vector2 offsetMin = new Vector2((minx - setrect.anchorMin.x * screenw), (miny - setrect.anchorMin.y * screenh));
        Vector2 offsetMax = new Vector2((maxx + (1 - setrect.anchorMax.x) * screenw), (maxy + (1 - setrect.anchorMax.y) * screenh));

        setrect.offsetMin = offsetMin;
        setrect.offsetMax = offsetMax;
    }
}
