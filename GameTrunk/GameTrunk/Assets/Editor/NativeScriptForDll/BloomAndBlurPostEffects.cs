using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
public class BloomAndBlurPostEffects : MonoBehaviour {

	[HideInInspector]
	public RenderTexture cameraRenderTexture;
	[HideInInspector]
	public RenderTexture customGrabRenderTextureA; 
	[HideInInspector]
	public RenderTexture customGrabRenderTextureB;
	[HideInInspector]
	public RenderTexture customGrabRenderTextureC;
	[HideInInspector]
	public RenderTexture customGrabRenderTextureD;

	[HideInInspector]
	public bool enableScreenChangeColor;

    [HideInInspector]
    public Material BloomAndBlurMat;

    [HideInInspector]
    public Material ScreenChangeColorMat;

	private int cameraBufferWidth;
	private int cameraBufferHeight;
	public int BufferWidth = 360;
	public int BufferHeight = 360;
	
	public float _BloomIntensity = 1.1f;
	public Color _BloomThreshhold = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	public float _SoftFocusIntensity = 0.15f;
	private Camera _cam;

	void setShaderParams()
	{
        Shader.SetGlobalVector("_Parameter", new Vector4(1, 1, _BloomIntensity, _SoftFocusIntensity));
        Shader.SetGlobalColor("_BloomThreshhold", _BloomThreshhold);
	}

    void InitMaterial()
    {
        BloomAndBlurMat = new Material(Shader.Find("Custom/BloomAndBlurShader"));
        BloomAndBlurMat.hideFlags = HideFlags.DontSave;
    }

    void Start ()
	{
        InitMaterial();
        setShaderParams();
		enableScreenChangeColor = false;
	}


    void OnEnable()
    {
        _cam = this.GetComponent<Camera>();
        cameraBufferWidth = _cam.pixelWidth;
        cameraBufferHeight = _cam.pixelHeight;

        BufferWidth = (int)(cameraBufferWidth / 2);
        BufferHeight = BufferWidth;

        customGrabRenderTextureA = RenderTexture.GetTemporary(BufferWidth, BufferHeight, 0, RenderTextureFormat.ARGB32);
        customGrabRenderTextureB = RenderTexture.GetTemporary(BufferWidth, BufferHeight, 0, RenderTextureFormat.ARGB32);
        customGrabRenderTextureC = RenderTexture.GetTemporary(BufferWidth / 2, BufferHeight / 2, 0, RenderTextureFormat.ARGB32);
        customGrabRenderTextureD = RenderTexture.GetTemporary(BufferWidth / 2, BufferHeight / 2, 0, RenderTextureFormat.ARGB32);

        setShaderParams();
    }

	public void EnableScreenColorChange(Color changeColor)
	{
		enableScreenChangeColor = true;
		Shader.SetGlobalColor("_Color", changeColor);
	}

	public void DisableScreenColorChange()
	{
		enableScreenChangeColor = false;
	}


    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (!enableScreenChangeColor)
        {
            if (BloomAndBlurMat != null)
            {
                BloomAndBlurMat.SetTexture("_Bloom", customGrabRenderTextureD);
                Graphics.Blit(src, customGrabRenderTextureA);
                Graphics.Blit(customGrabRenderTextureA, customGrabRenderTextureB, BloomAndBlurMat, 0);
                Graphics.Blit(customGrabRenderTextureB, customGrabRenderTextureA, BloomAndBlurMat, 1);
                Graphics.Blit(customGrabRenderTextureA, customGrabRenderTextureC, BloomAndBlurMat, 0);
                Graphics.Blit(customGrabRenderTextureC, customGrabRenderTextureD, BloomAndBlurMat, 1);
                Graphics.Blit(src, null, BloomAndBlurMat, 2);
            }

        }
        else
        {
            if (ScreenChangeColorMat != null)
            {
                Graphics.Blit(src, dest, ScreenChangeColorMat, 0);
            }
        }

    }


    void ReleaseTexture()
    {
        RenderTexture.ReleaseTemporary(customGrabRenderTextureA);
        RenderTexture.ReleaseTemporary(customGrabRenderTextureB);
        RenderTexture.ReleaseTemporary(customGrabRenderTextureC);
        RenderTexture.ReleaseTemporary(customGrabRenderTextureD);
    }
	void OnDisable()
	{
     
        ReleaseTexture();
    }
	
}
