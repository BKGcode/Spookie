using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveSpecularHighlights : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Renderer>().material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        gameObject.GetComponent<Renderer>().material.SetFloat("_SpecularHighlights", 0f);

        
    }

   
}
