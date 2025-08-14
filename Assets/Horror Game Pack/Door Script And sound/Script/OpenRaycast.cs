using UnityEngine;
using UnityEngine.UI;

public class OpenRaycast : MonoBehaviour
{
    [SerializeField] private int rayLength = 5;
    [SerializeField] private LayerMask LayerMaskInteract;
    public string DoorTag = "CabinetDoor";
    private DoorManager currentRaycastedObj;

    [SerializeField] private Image crosshair;
    private bool isCrosshairActive;
    [SerializeField] public KeyCode UseKey = KeyCode.Mouse0;

    private void Update()
    {
        RaycastHit hit;
        Vector3 fwd = transform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(transform.position, fwd, out hit, rayLength, LayerMaskInteract.value))
        {
            if (hit.collider.CompareTag(DoorTag))
            {
                DoorManager hitCabinetDoor = hit.collider.gameObject.GetComponent<DoorManager>();

                if (currentRaycastedObj != hitCabinetDoor)
                {
                    if (currentRaycastedObj != null)
                    {
                        currentRaycastedObj.HideObjectName();
                        CrosshairChange(false);
                    }

                    currentRaycastedObj = hitCabinetDoor;
                    currentRaycastedObj.ShowObjectName();
                    CrosshairChange(true);
                    isCrosshairActive = true;
                }

                if (Input.GetKeyDown(UseKey))
                {
                    currentRaycastedObj.ToggleCabinet();
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
