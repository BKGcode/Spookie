using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ExamineRaycast : MonoBehaviour
{
    [SerializeField] private int rayLength = 5;
    [SerializeField] private LayerMask LayerMaskInteract;
    private ObjectExaminer raycastedObj;

    [SerializeField] private Image crosshair;
    [SerializeField] private GameObject OnlyPickUpCanvas;
    private bool isCrosshairActive;
    private bool doOnce;
    [SerializeField] public KeyCode DescriptionKey = KeyCode.Q;
    public List<string> examineTags;
    private GameObject lastHitObject;

    private void Update()
    {
        RaycastHit hit;
        Vector3 fwd = transform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(transform.position, fwd, out hit, rayLength, LayerMaskInteract.value))
        {
            if (examineTags.Contains(hit.collider.tag))
            {
                if (!doOnce)
                {
                    raycastedObj = hit.collider.gameObject.GetComponent<ObjectExaminer>();
                    if (raycastedObj.wantOutline)
                    {
                        EnableOutline(hit.collider.gameObject, true);
                        lastHitObject = hit.collider.gameObject;
                    }
                    if (raycastedObj.OnlyTakeNoExamine == false)
                    {
                        raycastedObj.ShowObjectName();
                        CrosshairChange(true);
                    }
                    else
                    {
                        OnlyPickUpCanvas.SetActive(true);
                        CrosshairChange(true);
                    }
                }

                if (Input.GetKeyDown(raycastedObj.addToInventoryKey) && raycastedObj.OnlyTakeNoExamine == true)
                {
                    raycastedObj.AddToInventoryWithoutExamine();
                }

                isCrosshairActive = true;
                doOnce = true;

            }
        }
        else
        {
            if (isCrosshairActive)
            {
                raycastedObj.HideObjectName();
                CrosshairChange(false);
                doOnce = false;
                OnlyPickUpCanvas.SetActive(false);
                DisableOutline();
            }
        }
        if (Input.GetKeyDown(DescriptionKey) && raycastedObj.examining && raycastedObj.Description == false)
        {
            raycastedObj.ShowDescription2();
        }
        else if (Input.GetKeyDown(DescriptionKey) && raycastedObj.examining && raycastedObj.Description == true)
        {
            raycastedObj.DontShowDescription2();
        }
    }

    void CrosshairChange(bool on)
    {
        if (on && !doOnce)
        {
            crosshair.color = Color.red;
        }
        else
        {
            crosshair.color = Color.white;
            isCrosshairActive = false;
        }
    }

    public void EnableOutline(GameObject obj, bool enable)
    {
        Renderer objectRenderer = obj.GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
            objectRenderer.GetPropertyBlock(materialPropertyBlock);
            int enableValue = enable ? 1 : 0;
            materialPropertyBlock.SetInt("_ShowOutline", enableValue);
            objectRenderer.SetPropertyBlock(materialPropertyBlock);
        }
    }

    public void DisableOutline()
    {
        if (lastHitObject != null)
        {
            EnableOutline(lastHitObject, false);
            lastHitObject = null;
        }
    }
}
