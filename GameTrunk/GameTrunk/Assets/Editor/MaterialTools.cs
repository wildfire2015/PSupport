using UnityEngine;
using System.Collections;
using UnityEditor;

public class MaterialTools
{
    [MenuItem("材质工具/导出选中光方向")]
    public static void GetLightDir()
    {
        GameObject ob = Selection.activeObject as GameObject;
        if (ob == null) return;
        if (ob.GetComponent<Light>() == null) return;
        Debug.Log(-ob.transform.forward.normalized); 
    }

}
