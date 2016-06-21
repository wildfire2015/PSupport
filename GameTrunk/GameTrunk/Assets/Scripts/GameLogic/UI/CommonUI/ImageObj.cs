using UISystem;
using System;
using PSupport;
public class ImageObj : UIBase
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public override void onAutoLoadedUIObj()
    {
        DLoger.Log(mUIShowObj.name);
    }

    public override void refresh()
    {
       
    }

    public override void onAutoRelease()
    {
        
    }

    public override void onLoadedUIObj()
    {
       
    }

    public override void onRelease()
    {
       
    }
}
