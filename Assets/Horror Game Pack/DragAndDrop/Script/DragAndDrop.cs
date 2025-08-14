using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    [SerializeField] private InspectController inspectController;
    [SerializeField] private string ItemName;
    public Transform ObjectPlace;
    public Transform DefaultParent;
    public float weight = 1.0f;
    public float smoothTime = 0.1f;

    public bool isDragging = false;
    private Vector3 initialPosition;
    private bool isMousePressed = false;
    [HideInInspector]
    public Vector3 initialScale;
    private PlayerMovement playerMovement;
    private Vector3 currentVelocity = Vector3.zero;

    private void Start()
    {
        initialPosition = transform.position;
        initialScale = transform.localScale;
        playerMovement = FindObjectOfType<PlayerMovement>();
    }

    public void DragObject()
    {
        if (isMousePressed)
        {
            float dragForce = 1.0f / weight;

            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Camera.main.WorldToScreenPoint(transform.position).z;

            Vector3 dragVector = (Camera.main.ScreenToWorldPoint(mousePosition) - transform.position) * dragForce;

            Vector3 moveDirection = (playerMovement.transform.position - transform.position).normalized;

            if (playerMovement != null)
            {
                Vector3 targetPosition = playerMovement.transform.position + playerMovement.transform.forward * 2f;

                transform.position = Vector3.MoveTowards(transform.position, targetPosition, playerMovement.speed * Time.deltaTime);

            }
            RaycastHit hit;

            if (!Physics.Raycast(transform.position, moveDirection, out hit, dragVector.magnitude))
            {
                transform.position = Vector3.SmoothDamp(transform.position, transform.position + dragVector, ref currentVelocity, smoothTime);
                GetComponent<Rigidbody>().isKinematic = true;
            }
        }
    }

    private void UpdateMouseState()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isMousePressed = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isMousePressed = false;
        }
    }

    private void Update()
    {
        UpdateMouseState();
    }

    public void StopDragging()
    {
        isDragging = false;
        GetComponent<Rigidbody>().isKinematic = false;
    }

    public void ShowObjectName()
    {
        inspectController.ShowName(ItemName);
    }

    public void HideObjectName()
    {
        inspectController.HideName();
    }
}