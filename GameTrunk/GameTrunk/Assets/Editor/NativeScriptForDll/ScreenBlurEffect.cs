using UnityEngine;
using System.Collections;
public class ScreenBlurEffect : MonoBehaviour {

    private Camera curCam;

    [HideInInspector]
    public RenderTexture cameraRenderTexture;

    [HideInInspector]
    public RenderTexture BlurRenderTexture;

    private bool IsRenderToTexture = false;
    private int OriCullingMask;

    public Material BlurMaterial;
    public float BrightScale = 0.7f;
    public bool updateEveryFrame = false;

    private Texture2D screenTex;

    public void Awake()
    {
        cameraRenderTexture = null;
        BlurRenderTexture = null;
    }


    void ReleaseTexture()
    {
        RenderTexture.ReleaseTemporary(BlurRenderTexture);
        RenderTexture.ReleaseTemporary(cameraRenderTexture);
    }

    void OnDisable()
    {
        ReleaseTexture();
    }

    void OnEnable()
    {
        curCam = this.GetComponent<Camera>();
        IsRenderToTexture = false;

        int BufferWidth = curCam.pixelWidth/2 ;
        int BufferHeight = curCam.pixelHeight/2 ;
        if (curCam.orthographic)
        {
            BufferWidth = curCam.pixelWidth;
            BufferHeight = curCam.pixelHeight;
        }
        //cameraRenderTexture = new RenderTexture(BufferWidth, BufferHeight, 0, RenderTextureFormat.ARGB32);
        //cameraRenderTexture.wrapMode = TextureWrapMode.Clamp;
        //cameraRenderTexture.useMipMap = false;
        //cameraRenderTexture.filterMode = FilterMode.Bilinear;
        //cameraRenderTexture.DiscardContents(true, true);
        //cameraRenderTexture.Create();

        //BlurRenderTexture = new RenderTexture(BufferWidth, BufferHeight, 0, RenderTextureFormat.ARGB32);
        //BlurRenderTexture.wrapMode = TextureWrapMode.Clamp;
        //BlurRenderTexture.useMipMap = false;
        //BlurRenderTexture.filterMode = FilterMode.Bilinear;
        //BlurRenderTexture.DiscardContents(true, true);
        //BlurRenderTexture.Create();

        cameraRenderTexture = RenderTexture.GetTemporary(BufferWidth, BufferHeight, 0, RenderTextureFormat.ARGB32);
        BlurRenderTexture = RenderTexture.GetTemporary(BufferWidth, BufferHeight, 0, RenderTextureFormat.ARGB32);
        // BrightScale = 0.7f;          

        OriCullingMask = curCam.cullingMask;
      
    }
  
    public void RestorCullingMask()
    {
        curCam.cullingMask = OriCullingMask;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (BlurMaterial == null) 
        {
            return;
        }
        if (!IsRenderToTexture)
        {
            Graphics.Blit(src, cameraRenderTexture);            
            Graphics.Blit(cameraRenderTexture, BlurRenderTexture, BlurMaterial, 0);
            Graphics.Blit(BlurRenderTexture, cameraRenderTexture, BlurMaterial, 1);
         //   Graphics.Blit(cameraRenderTexture, BlurRenderTexture, BlurMaterial, 3);
            Graphics.Blit(BlurRenderTexture, (RenderTexture)dst);
        }
        else
        {          
            Graphics.Blit(BlurRenderTexture, (RenderTexture)dst);
        }

        if (!updateEveryFrame)
        {
            IsRenderToTexture = true;
            //curCam.targetTexture = null;
            curCam.cullingMask = 0;
        }
    }
  
}
