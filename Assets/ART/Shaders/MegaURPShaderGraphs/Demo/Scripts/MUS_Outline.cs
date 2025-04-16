using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MUS_Outline : MonoBehaviour
{

    Material myMaterial;
    void Start()
    {
        myMaterial = gameObject.GetComponent<Renderer>().material;
        myMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Front);
        myMaterial.SetInt("_ZWrite", 1);
        myMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Less);
        myMaterial.SetPass(0);
    }

}
