using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UISystem;
using System;
using PSupport;
using PSupport.LoadSystem;
public class UI_1:UIBase
{

    // Use this for initialization
    Button btn;
    Button btn2;
    ImageObj imageobj = null;
    void Start()
    {
       

    }
    public override void onAutoLoadedUIObj()
    {
  
        btn = mUIShowObj.transform.Find("Button1").GetComponent<Button>();
        btn2 = mUIShowObj.transform.Find("Button2").GetComponent<Button>();
        Image[] images = mUIShowObj.GetComponentsInChildren<Image>();
        for (int i = 0; i < images.Length; i++)
        {
            LoadSpriteManager.setImageSprite(images[i], "assetsbundles/ui/loadsprite/vip01-10_/TY_pic_VIP0" + (i + 1) + "@obj");
        }
        //btn.onClick.AddListener(onClickBtn);
        //btn2.onClick.AddListener(onClickBtn);
        imageobj = UIManager.createUI<ImageObj>("assetsbundles/ui/commonui/ImageObj");
        imageobj.transform.parent = mUIShowObj.transform;
        imageobj.transform.localPosition = Vector3.zero;
        imageobj.transform.localScale = Vector3.one;
       
        EventTriggerListener.Get(btn.gameObject).onClick += onClickBtn;
        EventTriggerListener.Get(btn2.gameObject).onClick += onClickBtn;
    }
    // Update is called once per frame
    public void onClickBtn(GameObject go)
    {
        EventSystem es = EventSystem.current;
        //DLoger.Log(es.currentSelectedGameObject.name);
        DLoger.Log(go.name);
    }
    void Update()
    {

    }

    public override void refresh()
    {
        //throw new NotImplementedException();
    }

    public override void onAutoRelease()
    {
        //throw new NotImplementedException();
    }

    public override void onLoadedUIObj()
    {
        //throw new NotImplementedException();
    }

    public override void onRelease()
    {
        throw new NotImplementedException();
    }
}
