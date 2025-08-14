using UnityEngine;
using UnityEngine.UI;

public class KeyCardRaycast : MonoBehaviour
{
    [SerializeField] private int rayLength = 5;
    [SerializeField] private LayerMask LayerMaskInteract;
    [SerializeField] private string KeypadButtonTag = "KeyPad";
    private KeypadCard currentRaycastedObj;

    [SerializeField] private Image crosshair;
    private bool isCrosshairActive;
    [SerializeField] public KeyCode UseKey = KeyCode.Mouse0;

    private void Update()
    {
        RaycastHit hit;
        Vector3 fwd = transform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(transform.position, fwd, out hit, rayLength, LayerMaskInteract.value))
        {
            if (hit.collider.CompareTag(KeypadButtonTag))
            {
                KeypadCard hitKeyCard = hit.collider.gameObject.GetComponent<KeypadCard>();

                if (currentRaycastedObj != hitKeyCard)
                {
                    if (currentRaycastedObj != null)
                    {
                        CrosshairChange(false);
                    }

                    currentRaycastedObj = hitKeyCard;
                    CrosshairChange(true);
                    isCrosshairActive = true;
                }

                if (Input.GetKeyDown(UseKey))
                {
                    currentRaycastedObj.KeyCardClick();
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
            CrosshairChange(false);
            isCrosshairActive = false;
            currentRaycastedObj = null;
        }
        else if (isCrosshairActive)
        {
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