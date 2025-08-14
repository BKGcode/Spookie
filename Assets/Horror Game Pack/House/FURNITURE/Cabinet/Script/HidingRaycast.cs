using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class HidingRaycast : MonoBehaviour
{
    public Camera playerCamera;
    public GameObject MainCameraObject;
    public float interactionDistance = 3f;
    public List<HideCamera> hideCamComponents = new List<HideCamera>(); 
    public List<Cabinet> cabinets = new List<Cabinet>();
    public AudioSource HideCabinet;
    [SerializeField] private Image crosshair;
    [SerializeField] private LayerMask LayerMaskInteract;

    private bool isHiding = false;
    private int currentHidingIndex = -1; 

    public PlayerMovement playerMovement;
    public MouseLook mouseLook;
    public InventoryController inventoryController;
    public Transform PlayerPosition;
    public PauseMenu pauseMenu;
    public CharacterController character;

    private Dictionary<int, Vector3> originalPositions = new Dictionary<int, Vector3>();
    private Dictionary<int, Quaternion> originalRotations = new Dictionary<int, Quaternion>();

    private float lastInteractionTime = 0f;
    private float interactionCooldown = 1f;
    private bool isCrosshairActive;

    void Start()
    {
        InitializeDictionaries();

        GameObject player = GameObject.FindGameObjectWithTag("Capsule");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
        }
    }

    void InitializeDictionaries()
    {
        foreach (var cabinet in cabinets)
        {
            int id = cabinet.GetID();
            originalPositions[id] = Vector3.zero;
            originalRotations[id] = Quaternion.identity;
        }
    }

    void Update()
    {
        if (Time.time - lastInteractionTime < interactionCooldown)
        {
            return;
        }

        RaycastHit hitObject;
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        if (Physics.Raycast(transform.position, fwd, out hitObject, interactionDistance, LayerMaskInteract.value))
        {
            CrosshairChange(true);
            isCrosshairActive = true;
        }
        else
        {
            if (isCrosshairActive)
            {
                CrosshairChange(false);
                isCrosshairActive = false;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (isHiding)
            {
                ExitHiding(currentHidingIndex);
                return;
            }



            if (Physics.Raycast(transform.position, fwd, out hitObject, interactionDistance, LayerMaskInteract.value))
            {
                Cabinet hitCabinet = hitObject.collider.GetComponent<Cabinet>();
                if (hitCabinet != null && hitCabinet.CanInteract())
                {
                    CrosshairChange(true);
                    Debug.Log("Clicked on cabinet with ID: " + hitCabinet.GetID());

                    lastInteractionTime = Time.time;

                    int id = hitCabinet.GetID();
                    if (id != -1)
                    {
                        if (!isHiding)
                        {
                            playerMovement.enabled = false;
                            StartHiding(id);
                        }
                    }
                    else
                    {
                        Debug.LogError("Cabinet ID not found");
                    }
                }
            }
            else
            {
                CrosshairChange(false);
            }
        }
    }

    void StartHiding(int id)
    {
        HideCabinet.Play();
        isHiding = true;
        currentHidingIndex = id;

        pauseMenu.enabled = false;
        character.enabled = false;
        AIController.isPlayerHiding = true; 
        mouseLook.canMove = false;

        var hideCam = hideCamComponents.Find(cam => cam.GetID() == id);

        if (hideCam != null)
        {
            // Elmentjük a célpont pozícióját és forgását
            var targetPosition = hideCam.GetChildCameraPosition();
            var targetRotation = hideCam.GetChildCameraRotation();

            inventoryController.PutItemBackInInventory();
            StartCoroutine(MoveMainCamera(hideCam, targetPosition, targetRotation));

            originalPositions[id] = hideCam.GetMainCameraPosition();
            originalRotations[id] = hideCam.GetMainCameraRotation();
        }
        else
        {
            Debug.LogError("HideCamera not found for ID: " + id);
        }
    }

    IEnumerator MoveMainCamera(HideCamera hideCam, Vector3 targetPosition, Quaternion targetRotation)
    {
        float startTime = Time.time;
        Vector3 startPosition = playerCamera.transform.position;
        Quaternion startRotation = playerCamera.transform.rotation;

        while (Time.time < startTime + 0.3f)
        {
            float t = (Time.time - startTime) / 0.3f;

            hideCam.SetMainCameraPosition(Vector3.Lerp(startPosition, targetPosition, t));
            hideCam.SetMainCameraRotation(Quaternion.Lerp(startRotation, targetRotation, t));

            yield return null;
        }

        hideCam.SetMainCameraEnabled(false);
        hideCam.SetChildCameraEnabled(true);

        hideCam.StartAnimation();
        var cabinet = cabinets.Find(cab => cab.GetID() == hideCam.GetID());
        if (cabinet != null)
        {
            cabinet.StartAnimation();
        }
    }

    void ExitHiding(int id)
    {
        HideCabinet.Play();
        MouseLook mouseLook = MainCameraObject.GetComponent<MouseLook>();
        if (mouseLook != null)
        {
            mouseLook.ResetRotation();
        }

        StartCoroutine(ExitHide(id));

        var hideCam = hideCamComponents.Find(cam => cam.GetID() == id);
        if (hideCam != null)
        {
            hideCam.SetMainCameraPosition(originalPositions[id]);
            hideCam.SetMainCameraRotation(originalRotations[id]);
        }

        var cabinet = cabinets.Find(cab => cab.GetID() == id);
        if (cabinet != null)
        {
            PlayerPosition.position = cabinet.GetTransformHolder().position;
            PlayerPosition.rotation = cabinet.GetTransformHolder().rotation;
        }
    }


    IEnumerator ExitHide(int id)
    {
        var hideCam = hideCamComponents.Find(cam => cam.GetID() == id);
        var cabinet = cabinets.Find(cab => cab.GetID() == id);
        if (hideCam != null && cabinet != null)
        {
            hideCam.ExitAnimation();
            cabinet.ExitAnimation();
        }

        yield return new WaitForSeconds(0.7f);

        isHiding = false;
        currentHidingIndex = -1;

        pauseMenu.enabled = true;
        character.enabled = true;
        AIController.isPlayerHiding = false;

        if (hideCam != null)
        {
            hideCam.SetChildCameraEnabled(false);
            hideCam.SetMainCameraEnabled(true);

            hideCam.IdleMode();
        }

        playerMovement.enabled = true;
        mouseLook.canMove = true;
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