using UnityEngine;
using System.Collections;

public enum eShadowType
{
    eShadowType_ProjectTexture,
    eShadowType_Depth
};
public class ShadowSystem
{
    public static Camera mShadowCamera = null;
    public static Camera mRenderShdowDepthCamera = null;
    static public void refresh(eShadowType eSt = eShadowType.eShadowType_ProjectTexture, bool bUpdateInRealtime = false,int iShadowMapSize = 1024)
    {
        foreach (Camera camera in Camera.allCameras)
        {
            if (camera.tag == "ShadowCameraTag")
            {
                mShadowCamera = camera;
                break;
            }
        }
       // mRenderShdowDepthCamera = GameObject.Find("DepthShadowCamera") as Camera;
        //GameObject[] receiveobjs = GameObject.FindGameObjectsWithTag("ReceiveShadowObject");
        //foreach (GameObject obj in receiveobjs)
        //{
        //    Material[] mats = obj.GetComponentsInChildren<Material>();
        //    foreach (Material mat in mats)
        //    {
        //        mat.shader = Shader.Find("Custom/Shadow/RenderShadow");
        //    }
        //}


        if (mShadowCamera != null)
        {

            if (eSt == eShadowType.eShadowType_Depth)
            {
                Shader.EnableKeyword("SHADOW_DEPTH");
                mShadowCamera.backgroundColor = new Color(1, 1, 1, 1);
                mShadowCamera.targetTexture = new RenderTexture(iShadowMapSize, iShadowMapSize, 16, RenderTextureFormat.ARGB32);
            }
            else
            {
                Shader.DisableKeyword("SHADOW_DEPTH");
                mShadowCamera.backgroundColor = new Color(0, 0, 0, 0);
                mShadowCamera.targetTexture = new RenderTexture(iShadowMapSize, iShadowMapSize, 0, RenderTextureFormat.ARGB32);
            }
            mShadowCamera.cullingMask = 1 << LayerMask.NameToLayer("Character");
            mShadowCamera.clearFlags = CameraClearFlags.SolidColor;
            mShadowCamera.depth = 1;
            mShadowCamera.farClipPlane = 30;
            GameObject.DestroyImmediate(mShadowCamera.GetComponent<AudioListener>());
            GameObject.DestroyImmediate(mShadowCamera.GetComponent<GUILayer>());
            Shader rendershadow = Shader.Find("Custom/Shadow/RenderShadowMap");
            mShadowCamera.SetReplacementShader(rendershadow, "");

            Matrix4x4 proj = mShadowCamera.projectionMatrix;
            Matrix4x4 view = mShadowCamera.worldToCameraMatrix;
            Matrix4x4 viewProj = proj * view;
            Shader.SetGlobalMatrix("myShadowCameraVP", viewProj);

            GameObject[] Receiveobjs = GameObject.FindGameObjectsWithTag("ReceiveShadowObject");
            foreach (GameObject Receiveobj in Receiveobjs)
            {
                Material matreceiveshadow = Receiveobj.GetComponentInChildren<Renderer>().material;
                matreceiveshadow.SetTexture("ShadowMap", mShadowCamera.targetTexture);
            }
            if (bUpdateInRealtime)
            {
                mShadowCamera.gameObject.AddComponent<ShadowCamera>();
            }
        }
        

    }

}
