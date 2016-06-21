using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using PSupport.LoadSystem;
using System.IO;
/*******************************************************************************
* 
*             类名: LoadSpriteManager
*             功能: 动态UISprite管理器
*             作者: 彭谦
*             日期: 2015.12.7
*             修改:
*             备注:用来管理动态加载的sprite,通过插件生成的含有动态加载预制体渲染器的prefab,
*                 调用setImageSprite接口来完成设置动态加载的sprite到image对象
*             
* *****************************************************************************/

namespace UISystem
{
    public class LoadSpriteManager
    {
        private LoadSpriteManager() { }

        static Dictionary<string, Sprite> mdicLoadSprite = new Dictionary<string, Sprite>();


        //如果在安卓平台下,运用ETC1格式,加alpha通道shader,统一材质合并DP
#if UNITY_ANDROID
    static Dictionary<Texture2D, Material> mdicmat = new Dictionary<Texture2D, Material>();
    static void addMatETC1(Texture2D texture, Texture2D texturealpha)
    {
            if (!mdicmat.ContainsKey(texture))
            {
                ResourceLoadManager.requestRes("local/material/ui/etc1alpha", typeof(Material), eLoadResPath.RP_Resources);
                Material mat = (Material)ResourceLoadManager.getRes("local/material/ui/etc1alpha", typeof(Material), eLoadResPath.RP_Resources);
                mat.SetTexture("_MainTex", texture);
                mat.SetTexture("_AlphaTex", texturealpha);
                mdicmat.Add(texture, mat);
            }

    }



    static Material getMatbySprite(Sprite sprite)
    {
        if (mdicmat.ContainsKey(sprite.texture))
        {
            return mdicmat[sprite.texture];
        }
        else
        {
            addMatETC1(sprite.texture, sprite.associatedAlphaSplitTexture);
            return mdicmat[sprite.texture];
        }

    }
#endif


        public static void setImageSprite(Image image, string loadspritepath)
        {
            if (mdicLoadSprite.ContainsKey(loadspritepath))
            {
                image.sprite = mdicLoadSprite[loadspritepath];
                image.SetNativeSize();
            }
            else
            {
                ResourceLoadManager.requestRes(loadspritepath, eLoadResPath.RP_URL, onLoadedSprite, new KeyValuePair<string, Image>(loadspritepath, image), true);
            }
        }
        public static void removeSprite(string loadspritepath)
        {
            if (mdicLoadSprite.ContainsKey(loadspritepath))
            {
                mdicLoadSprite.Remove(loadspritepath);
                ResourceLoadManager.removeRes(loadspritepath);
            }
        }
        public static void clear()
        {
            foreach (string path in mdicLoadSprite.Keys)
            {
                ResourceLoadManager.removeRes(path);
            }
            mdicLoadSprite.Clear();
        }
        static void onLoadedSprite(object o, eLoadedNotify loadedNotify)
        {
            if (loadedNotify == eLoadedNotify.Load_Successfull)
            {

                KeyValuePair<string, Image> obj = (KeyValuePair<string, Image>)o;
                Sprite sprite = ((GameObject)(ResourceLoadManager.getRes(obj.Key))).GetComponent<SpriteRenderer>().sprite;
                if (!mdicLoadSprite.ContainsKey(obj.Key))
                {
                    mdicLoadSprite.Add(obj.Key, sprite);
                }
                if (obj.Value != null)
                {
#if UNITY_ANDROID
                    //如果是ect1格式
                    if (sprite.texture.format == TextureFormat.ETC_RGB4)
                    {
                        Material mat = getMatbySprite(sprite);
                        obj.Value.material = mat;
                    }
#endif
                    obj.Value.sprite = sprite;
                    obj.Value.SetNativeSize();
                }



            }
        }
    }

}

