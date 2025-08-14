using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Television : MonoBehaviour
{
    public GameObject tvOffScreen;
    public GameObject tvOnScreen;
    public bool isGeneratorOn = false;
    void Update()
    {
        if(!isGeneratorOn)
        {
            tvOnScreen.SetActive(false);
            tvOffScreen.SetActive(true);
        }
        else
        {
            tvOnScreen.SetActive(true);
            tvOffScreen.SetActive(false);
        }
    }
}
