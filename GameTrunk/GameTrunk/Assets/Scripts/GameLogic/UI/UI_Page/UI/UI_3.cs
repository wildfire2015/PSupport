using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UISystem;
using System;
using PSupport;
using PSupport.LoadSystem;

public class UI_3 : UIBase
{

    public override void onLoadedUIObj()
    {
        LoadSpriteManager.setImageSprite(Img_haha21, "assetsbundles/ui/loadsprite/vip01-10_/TY_pic_VIP08@obj");
    }
	public override void refresh()
    {

    }

    public override void onRelease()
    {

    }
	void Update()
    {

    }
//auto generatescript,do not make script under this line==
    public override void onAutoLoadedUIObj()
	{
		Img_haha21 = mUIShowObj.transform.Find("Img_haha21").GetComponent<Image>();
		Img_haha2444 = mUIShowObj.transform.Find("Img_haha2444").GetComponent<Image>();
		
		onLoadedUIObj();

	}
	public override void onAutoRelease()
	{
		onRelease();
		Img_haha21 = null;
		Img_haha2444 = null;
		
	}
	public Image Img_haha21 = null;
	public Image Img_haha2444 = null;
	
}
