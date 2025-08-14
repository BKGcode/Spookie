using UnityEngine;
using UnityEngine.UI;

public class SwitcherRayCast : MonoBehaviour
{
    [SerializeField] private int rayLength = 5;
    [SerializeField] private LayerMask interactableLayerMask; 
    [SerializeField] private LayerMask obstacleLayerMask; 
    private Switcher currentRaycastedObj;

    [SerializeField] private Image crosshair;
    private bool isCrosshairActive;
    [SerializeField] public KeyCode UseKey = KeyCode.Mouse0;

    private void Update()
    {
        RaycastHit hit;
        Vector3 fwd = transform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(transform.position, fwd, out hit, rayLength, interactableLayerMask))
        {
            if (!Physics.Raycast(transform.position, fwd, hit.distance, obstacleLayerMask))
            {
                if (hit.collider.CompareTag("Switcher"))
                {
                    Switcher hitSwitcher = hit.collider.gameObject.GetComponent<Switcher>();

                    if (currentRaycastedObj != hitSwitcher)
                    {
                        if (currentRaycastedObj != null)
                        {
                            currentRaycastedObj.HideObjectName();
                            CrosshairChange(false);
                        }

                        currentRaycastedObj = hitSwitcher;
                        currentRaycastedObj.ShowObjectName();
                        CrosshairChange(true);
                        isCrosshairActive = true;
                    }

                    if (Input.GetKeyDown(UseKey))
                    {
                        currentRaycastedObj.ToggleSwitcher();
                    }
                }
                else
                {
                    ResetRaycast();
                }
            }
            else
            {
                ResetRaycast();
            }
        }
        else
        {
            ResetRaycast();
        }
    }

    void ResetRaycast()
    {
        if (currentRaycastedObj != null)
        {
            currentRaycastedObj.HideObjectName();
            CrosshairChange(false);
            isCrosshairActive = false;
            currentRaycastedObj = null;
        }
        else if (isCrosshairActive)
        {
            currentRaycastedObj.HideObjectName();
            isCrosshairActive = false;
            CrosshairChange(false);
        }
    }

    void CrosshairChange(bool on)
    {
        if (on)
        {
            crosshair.color = Color.red;
        }
        else
        {
            crosshair.color = Color.white;
        }
    }
}
