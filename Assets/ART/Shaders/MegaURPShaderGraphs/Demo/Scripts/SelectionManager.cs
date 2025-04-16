using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectionManager : MonoBehaviour
{
    //Tag name for the selectable Gameobjects
    [SerializeField] string selectableTag = "Selectable";
    //Layer number for the selectable Gameobjects
    [SerializeField] int layerNumber = 10;

    //Put this boolean to true if you prefer using a tag name for your selectable gameobjects.
    [SerializeField] bool useTag=false;

    Transform _selection;
    [SerializeField] public Material[] TestMatShaders;
    Material highlightMaterial;
    Material defaultMaterial;
    GameObject lastSelected;
    float mixEffectValue=0.1f;
    bool canHitAgain=true;
    int matNumi;
    // Text ShaderName;
    [SerializeField]
    TextMeshProUGUI ShaderName;
    [SerializeField]
    GameObject CanvasItem;

    private void Start()
    {
        CanvasItem.SetActive(true);
        matNumi = 0;
        highlightMaterial =TestMatShaders[0];
        ShaderName.text = ""+ (matNumi+1)+". "+TestMatShaders[0].name;
    }

    // Update is called once per frame
    //! Important ! You need to add a Physical Raycast component to your main camera 
    void Update()
    {
        /*
      if (Input.GetButton("Jump"))
        {
            canHitAgain = false;
            Invoke("HitAgain", 0.2f);
            if (CanvasItem.activeSelf)
            {
                CanvasItem.SetActive(false);
            }
            else
            {
                CanvasItem.SetActive(true);
            }
        }
      */

        if (_selection !=null)
        {
            //RESET SELECTION
            var selectionRenderer = _selection.GetComponent<Renderer>();
            if (selectionRenderer != null)
            {
              
                selectionRenderer.material = this.defaultMaterial;
                lastSelected = null;
                


            }
        }

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        _selection = null;
        if (Physics.Raycast(ray,out var hit))
        {
            //SELECT A NEW OBJECT
            var selection = hit.transform;
            if (useTag)
            {
                if (selection.CompareTag(selectableTag)) // Tag method
                {

                    _selection = selection;
                    defaultMaterial = _selection.GetComponent<Renderer>().material;
                   
                    ;
                }
              
            }
            else
            {
              
                if (selection.gameObject.layer == layerNumber) // Layer method
                {
                    _selection = selection;
                    defaultMaterial = _selection.GetComponent<Renderer>().material;
                   
                }
            }
        }

        //APPLY NEW MATERIAL ON SELECTION
        if(_selection!=null)
        {
            var selectionRenderer = _selection.GetComponent<Renderer>();
            if (selectionRenderer != null)
            {
                if (lastSelected != selectionRenderer.gameObject)
                {
                    selectionRenderer.material = this.highlightMaterial;
                    //TRANSFER ALL BASE PARAMETERS OF THE INITIAL MATERIAL
                    if (defaultMaterial.HasProperty("_BaseMap"))
                    {
                        selectionRenderer.material.SetTexture("_BaseMap", defaultMaterial.GetTexture("_BaseMap"));
                    }
                    if (defaultMaterial.HasProperty("_BaseColor"))
                    {
                        selectionRenderer.material.SetColor("_BaseColor", defaultMaterial.GetColor("_BaseColor"));
                    }
                 /*
                    if (defaultMaterial.HasProperty("_Cutoff"))
                    {

                        selectionRenderer.material.SetFloat("_Cutoff", defaultMaterial.GetFloat("_Cutoff"));

                    }
                    */
                    if (defaultMaterial.HasProperty("_BumpMap"))
                    {
                        selectionRenderer.material.SetTexture("_BumpMap", defaultMaterial.GetTexture("_BumpMap"));
                    }
                    if (defaultMaterial.HasProperty("_BumpScale"))
                    {

                        selectionRenderer.material.SetFloat("_BumpScale", defaultMaterial.GetFloat("_BumpScale"));

                    }
                    if (defaultMaterial.HasProperty("_OcclusionMap"))
                    {
                        selectionRenderer.material.SetTexture("_OcclusionMap", defaultMaterial.GetTexture("_OcclusionMap"));
                    }
                    if (defaultMaterial.HasProperty("_MetallicGlossMap"))
                    {
                        selectionRenderer.material.SetTexture("_MetallicGlossMap", defaultMaterial.GetTexture("_MetallicGlossMap"));
                    }
                    if (defaultMaterial.HasProperty("_Metallic"))
                    {

                        selectionRenderer.material.SetFloat("_Metallic", defaultMaterial.GetFloat("_Metallic"));

                    }
                    if (defaultMaterial.HasProperty("_Smoothness"))
                    {

                        selectionRenderer.material.SetFloat("_Smoothness", defaultMaterial.GetFloat("_Smoothness"));

                    }

                    if (defaultMaterial.HasProperty("_Tiling"))
                    {

                        selectionRenderer.material.SetVector("_Tiling", defaultMaterial.GetVector("_Tiling"));

                    }
                    if (defaultMaterial.HasProperty("_Offset"))
                    {

                        selectionRenderer.material.SetVector("_Offset", defaultMaterial.GetVector("_Offset"));

                    }
                    if (defaultMaterial.HasProperty("_EmissionMap"))
                    {
                        selectionRenderer.material.SetTexture("_EmissionMap", defaultMaterial.GetTexture("_EmissionMap"));
                    }
                    if (defaultMaterial.HasProperty("_EmissionColor"))
                    {
                        selectionRenderer.material.SetColor("_EmissionColor", defaultMaterial.GetColor("_EmissionColor"));
                    }
                 

                }
            }
        }

      
    }

    public void NextShader()
    {
        canHitAgain = false;
        Invoke("HitAgain", 0.2f);
        matNumi++;
      
        if (matNumi >= TestMatShaders.Length)
        {
            matNumi = 0;
        }
        highlightMaterial = TestMatShaders[matNumi];
        ShaderName.text = "" + (matNumi + 1) + ". " + TestMatShaders[matNumi].name;
    }

    public void PreviousShader()
    {
        canHitAgain = false;
        Invoke("HitAgain", 0.2f);
        matNumi--;
       
        if (matNumi == -1)
        {
            matNumi = TestMatShaders.Length - 1;
        }
        highlightMaterial = TestMatShaders[matNumi];
        ShaderName.text = "" + (matNumi + 1) + ". " + TestMatShaders[matNumi].name;
    }

    public void GoDemoScene(int numi)
    {
        switch(numi)
        {
            case 1:
                Application.LoadLevel("PBRDemo");
                break;
            case 2:
                Application.LoadLevel("UnlitDemo");
                break;
            case 3:
                Application.LoadLevel("SpriteDemo");
                break;
            default:
                Application.LoadLevel("PBRDemo");
                break;
        }
    }
    void HitAgain()
    {
        canHitAgain = true;
    }
   

}
