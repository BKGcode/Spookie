using UnityEngine;
using UnityEngine.UI;

public class DragAndDropRaycast : MonoBehaviour
{
    [SerializeField] private int rayLength = 5;
    [SerializeField] private LayerMask LayerMaskInteract;
    private DragAndDrop currentRaycastedObj;

    [SerializeField] private Image crosshair;
    private bool isCrosshairActive;
    private bool doOnce;
    [SerializeField] public KeyCode Holdkey = KeyCode.Mouse0;

    [SerializeField] private float maxDistance = 5f;
    public float MaximumWeightPlayerCanHoldInHands;
    private float initialDistance;
    private PlayerMovement playerMovement;

    private void Start()
    {
        initialDistance = Vector3.Distance(transform.position, Camera.main.transform.position);
    }

    private void Update()
    {
        RaycastHit hit;
        Vector3 fwd = transform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(transform.position, fwd, out hit, rayLength, LayerMaskInteract.value))
        {
            if (hit.collider.CompareTag("Drag"))
            {
                DragAndDrop hitObject = hit.collider.gameObject.GetComponent<DragAndDrop>();

                if (currentRaycastedObj != hitObject)
                {
                    if (currentRaycastedObj != null)
                    {
                        currentRaycastedObj.HideObjectName();
                        CrosshairChange(false);
                        currentRaycastedObj.StopDragging();
                    }

                    currentRaycastedObj = hitObject;
                    currentRaycastedObj.ShowObjectName();
                    CrosshairChange(true);
                    doOnce = true;
                }

                if (Input.GetKey(Holdkey))
                {
                    currentRaycastedObj.DragObject();
                }
                else
                {
                    currentRaycastedObj.StopDragging();
                }

                isCrosshairActive = true;
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
            currentRaycastedObj.StopDragging();
            currentRaycastedObj = null;
            doOnce = false;
            isCrosshairActive = false;
        }
        else if (isCrosshairActive)
        {
            CrosshairChange(false);
            isCrosshairActive = false;
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