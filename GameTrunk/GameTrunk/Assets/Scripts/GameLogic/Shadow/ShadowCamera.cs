using UnityEngine;
using System.Collections;

public class ShadowCamera : MonoBehaviour
{

    void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        Matrix4x4 proj = ShadowSystem.mShadowCamera.projectionMatrix;
        Matrix4x4 view = ShadowSystem.mShadowCamera.worldToCameraMatrix;
        Matrix4x4 viewProj = proj * view;
        Shader.SetGlobalMatrix("myShadowCameraVP", viewProj);
    }
}
