using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopProperty : MonoBehaviour
{
    [SerializeField]
    string PropertyName;
    [SerializeField]
   
    float Smooth=2.0f;
    [SerializeField]
    float ValueMin= 0f;
    [SerializeField]
    float ValueMax = 1.0f;
    float TestMax;
    float TestMin;
    float TestRange=0.02f;
    float PropertyValue;
    bool LerpUp;

    Material myMaterial;
    void Start()
    {
    LerpUp = false;
        PropertyValue = ValueMax;
        myMaterial = gameObject.GetComponent<Renderer>().material;
    
        if (ValueMax>0)
        {
            TestMax = ValueMax - TestRange;
        }
        else
        {
            TestMax = ValueMax + TestRange;
        }
       

            TestMin = ValueMin + TestRange;
       

    }

    private void Update()
    {
        
        if (LerpUp & PropertyValue >=TestMax)
        {
            LerpUp = false;
           PropertyValue = ValueMax;
        }
        else if (!LerpUp & PropertyValue <= TestMin)
        {
            LerpUp = true;
            PropertyValue = ValueMin;
        }
        if (myMaterial.HasProperty(PropertyName))
        {

            myMaterial.SetFloat(PropertyName, PropertyValue);
        }
        LoopMe();
       
    }
    void LoopMe()
    {
        if (LerpUp)
        {

            PropertyValue = Mathf.Lerp(PropertyValue, ValueMax,Time.deltaTime * Smooth);
            
          
           
        }
        else
        {
            PropertyValue = Mathf.Lerp(PropertyValue, ValueMin, Time.deltaTime * Smooth);
        }
      
    }
}
