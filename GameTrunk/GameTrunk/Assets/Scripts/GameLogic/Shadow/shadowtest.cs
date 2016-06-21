using UnityEngine;
using System.Collections;
using PSupport.LoadSystem;

public class shadowtest : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        ResourceLoadManager.mbuseassetbundle = false;
        ShadowSystem.refresh(eShadowType.eShadowType_Depth,true);
        //ShadowSystem.refresh();

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
