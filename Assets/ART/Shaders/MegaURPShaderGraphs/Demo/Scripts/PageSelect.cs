using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PageSelect : MonoBehaviour
{
    

    [SerializeField]
    TextMeshProUGUI PageName;
    [SerializeField]
    GameObject CanvasItem;
    [SerializeField]
    GameObject ButtonNext;
    [SerializeField]
    GameObject ButtonPrevious;
    int pageNumi;
    [SerializeField]
    int maxPage=2;
    [SerializeField]
    GameObject myCamera;
    Vector3 myCameraPos;
    bool CanHitAgain = true;

    // Start is called before the first frame update
    void Start()
    {
        pageNumi = 1;
        PageName.text = "Page " + pageNumi;
        CanvasItem.SetActive(true);
        ButtonPrevious.SetActive(false);
      
        myCameraPos = myCamera.transform.position;

    }

    public void PageNext()
    {
        if (CanHitAgain)
        {
            CanHitAgain = false;
            pageNumi++;
            ButtonPrevious.SetActive(true);
            if (pageNumi == maxPage)
            {
                ButtonNext.SetActive(false);
            }
            PageName.text = "Page " + pageNumi;
            myCamera.transform.position = new Vector3(myCameraPos.x + ((pageNumi - 1) * 100), myCameraPos.y, myCameraPos.z);
        }
        Invoke("HitAgain", 0.2f);
    }

    public void PagePrevious()
    {
        if (CanHitAgain)
        {
            CanHitAgain = false;
            pageNumi--;
            ButtonNext.SetActive(true);
            if (pageNumi == 1)
        {
            ButtonPrevious.SetActive(false);
        }
        PageName.text = "Page " + pageNumi;
        myCamera.transform.position = new Vector3(myCameraPos.x + ((pageNumi - 1) * 100), myCameraPos.y, myCameraPos.z);
        }
        Invoke("HitAgain", 0.2f);
    }

    void HitAgain()
    {
        CanHitAgain = true;
    }
    public void GoDemoScene(int numi)
    {
        switch (numi)
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
}
