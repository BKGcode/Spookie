using UnityEngine;
using System.Collections.Generic;

public class OutlineRaycast : MonoBehaviour
{
    public List<string> outlineTags;
    public List<LayerMask> layerMasks;
    public float rayLength = 10f;

    private GameObject currentRaycastedObj;

    private void Update()
    {
        RaycastHit hit;
        Vector3 fwd = transform.TransformDirection(Vector3.forward);

        for (int i = 0; i < layerMasks.Count; i++)
        {
            LayerMask layerMask = layerMasks[i];
            if (Physics.Raycast(transform.position, fwd, out hit, rayLength, layerMask))
            {
                if (outlineTags.Contains(hit.collider.tag))
                {
                    GameObject hitObject = hit.collider.gameObject;

                    if (currentRaycastedObj != hitObject)
                    {
                        if (currentRaycastedObj != null)
                        {
                            EnableOutline(currentRaycastedObj, false);
                        }

                        currentRaycastedObj = hitObject;
                        EnableOutline(currentRaycastedObj, true);
                    }

                    return;
                }
            }
        }

        DisableOutline();
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
        if (currentRaycastedObj != null)
        {
            EnableOutline(currentRaycastedObj, false);
            currentRaycastedObj = null;
        }
    }
}
