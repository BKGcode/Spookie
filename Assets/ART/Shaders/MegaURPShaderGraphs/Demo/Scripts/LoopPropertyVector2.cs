using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopPropertyVector2 : MonoBehaviour
{
    [SerializeField]
    string PropertyName;
    [SerializeField]
   
    float Smooth=2.0f;
    [SerializeField]
    Vector2 ValueMin = new Vector2(0.0f,0.0f);
    [SerializeField]
    Vector2 ValueMax = new Vector2(1.0f, 1.0f);

    Vector2 TestMax;
    Vector2 TestMin;
    float TestRange=0.02f;
    Vector2 PropertyValue;
    bool LerpUp;

    Material myMaterial;
    void Start()
    {
    LerpUp = false;
        PropertyValue = ValueMax;
        myMaterial = gameObject.GetComponent<Renderer>().material;
    
        if (ValueMax.x>0)
        {
            TestMax.x = ValueMax.x - TestRange;
            TestMax.y = ValueMax.y - TestRange;
        }
        else
        {
            TestMax.x = ValueMax.x + TestRange;
            TestMax.y = ValueMax.y + TestRange;
        }
       

            TestMin .x= ValueMin.x + TestRange;
            TestMin.y = ValueMin.y + TestRange;


    }

    private void Update()
    {
        
        if (LerpUp & PropertyValue.x >=TestMax.x)
        {
            LerpUp = false;
           PropertyValue = ValueMax;
        }
        else if (!LerpUp & PropertyValue.x <= TestMin.x)
        {
            LerpUp = true;
            PropertyValue = ValueMin;
        }
        if (myMaterial.HasProperty(PropertyName))
        {

            myMaterial.SetVector(PropertyName, PropertyValue);
        }
        LoopMe();
       
    }
    void LoopMe()
    {
        if (LerpUp)
        {

            PropertyValue.x = Mathf.Lerp(PropertyValue.x, ValueMax.x,Time.deltaTime * Smooth);
            PropertyValue.y = Mathf.Lerp(PropertyValue.y, ValueMax.y, Time.deltaTime * Smooth);



        }
        else
        {
            PropertyValue.x = Mathf.Lerp(PropertyValue.x, ValueMin.x, Time.deltaTime * Smooth);
            PropertyValue.y = Mathf.Lerp(PropertyValue.y, ValueMin.y, Time.deltaTime * Smooth);
        }
      
    }
}
