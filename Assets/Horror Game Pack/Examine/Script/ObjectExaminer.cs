using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    [HideInInspector]
    [SerializeField]
    public string itemName;
    public Sprite itemImage;
    public GameObject associatedObject;
    [HideInInspector]
    public int itemId;
    public float XRotation;
    public float YRotation;
    public float ZRotation;
}

public class EmissionControl : MonoBehaviour
{
    private Renderer objectRenderer;
    private MaterialPropertyBlock materialPropertyBlock;

    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        materialPropertyBlock = new MaterialPropertyBlock();
    }

    public void EnableEmission(bool enable)
    {
        int enableValue = enable ? 1 : 0;
        objectRenderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetInt("_EnableEmission", enableValue);
        objectRenderer.SetPropertyBlock(materialPropertyBlock);
    }
}


public class ObjectExaminer : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("The main camera used in the scene.")]
    public Camera mainCamera;
    [Tooltip("An array of cameras used for examining the object.")]
    public Camera[] examineCameras;
    [Tooltip("The speed at which the object rotates during examination.")]
    public float rotationSpeed = 5f;
    [Tooltip("The maximum distance from the camera to the object when picking it up.")]
    public float examineDistance = 5f;
    [Tooltip("The distance from the camera to the object during examination.")]
    public float examineDistanceObject = 5f;
    [Tooltip("The field of view of the cameras during examination.")]
    public float examineFOV = 30f;

    [Header("UI Settings")]
    [Tooltip("The tag of the UI canvas used in the scene. The UI elements with this tag will be disabled during examination.")]
    public string CanvasTag = "UI";
    public GameObject ExamineCanvas;
    public GameObject TakeButton;
    public GameObject DescriptionButton;

    [Header("Controller Settings")]
    [Tooltip("The inspect controller used for object examination.")]
    [SerializeField] private InspectController inspectController;
    public PlayerMovement playerMovement;
    public bool examining;
    public bool Description;

    [SerializeField] private KeyCode DescriptionKey = KeyCode.Q;
    [SerializeField] public KeyCode addToInventoryKey = KeyCode.E;
    [SerializeField] KeyCode PickUp = KeyCode.Mouse0;
    [SerializeField] KeyCode PutDown = KeyCode.Mouse0;

    [Header("Layer Settings")]
    [Tooltip("The layer used to determine which objects can be examined.")]
    public LayerMask examineLayer;

    [Header("Item Customization")]
    [Tooltip("If enabled, the object can be taken into the inventory.")]
    public bool takeable = true;
    [Tooltip("If enabled, the object has additional description information.")]
    public bool HaveDescription = true;
    [Tooltip("If enabled, the object can only be taken from the ground and cannot be examined.")]
    public bool OnlyTakeNoExamine = false;
    [Tooltip("If your scene is dark, enable this to make the examined object more visible.")]
    public bool wantEmission;
    public bool wantOutline;

    [TextArea] [SerializeField] private string itemextraInfo;
    [SerializeField] private string ItemName;
    public InventoryItem inventoryItem;

    private bool isExamining = false;
    private GameObject currentExaminedObject;
    private Vector3 previousMousePosition;
    private Rigidbody examinedObjectRigidbody;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3[] initialPositions;
    private Quaternion[] initialRotations;
    private bool canAddToInventory;
    private GameObject canvas;
    private Material[] materials;
    private GameObject thisgameobject;
    private EmissionControl emissionControl;

    private void Start()
    {
        inventoryItem.itemName = ItemName;
        thisgameobject = this.gameObject;
        foreach (Camera examineCamera in examineCameras)
        {
            examineCamera.enabled = false;
        }
        examining = false;
        Description = false;
        canAddToInventory = true;
        ExamineCanvas.SetActive(false);
        TakeButton.SetActive(false);
        DescriptionButton.SetActive(false);
        canvas = GameObject.FindWithTag(CanvasTag);
    }
    private void Update()
    {
        if (!isExamining)
        {
            if (Input.GetKeyDown(PickUp) && !OnlyTakeNoExamine)
            {
                RaycastHit hit;
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, examineDistance, examineLayer))
                {
                    currentExaminedObject = hit.collider.gameObject;
                    if (playerMovement != null && playerMovement.Canvas != null)
                    {
                        playerMovement.DisableTheCanvas();
                    }
                    StartExamination(this.gameObject);
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(PutDown))
            {
                StopExamination();
            }
            else
            {
                float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
                float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

                currentExaminedObject.transform.Rotate(Vector3.up, -mouseX, Space.World);
                currentExaminedObject.transform.Rotate(Vector3.right, mouseY, Space.World);
            }
        }

        if (Input.GetKeyDown(DescriptionKey) && examining == true && Description == false && HaveDescription)
        {
            ShowDescription();
        }
        else if (Input.GetKeyDown(DescriptionKey) && examining == true && Description == true && HaveDescription)
        {
            DontShowDescription();
        }

        if (Input.GetKeyDown(addToInventoryKey) && examining && Description == false && canAddToInventory && takeable == true)
        {
            AddToInventory();
        }
    }

    public void StartExamination(GameObject gameObject)
    {
        if (isExamining || gameObject == currentExaminedObject)
        {
            inventoryItem.associatedObject = currentExaminedObject;
            isExamining = true;
            previousMousePosition = Input.mousePosition;

            mainCamera.enabled = false;

            canvas.SetActive(false);

            var inventoryController = FindObjectOfType<InventoryController>();
            inventoryController.CantOpenInventory = true;

            // Enable the examine cameras
            foreach (Camera examineCamera in examineCameras)
            {
                examineCamera.enabled = true;
                examineCamera.fieldOfView = examineFOV;
            }

            // Disable gravity and freeze position/rotation on the examined object's Rigidbody
            examinedObjectRigidbody = currentExaminedObject.GetComponent<Rigidbody>();
            if (examinedObjectRigidbody != null)
            {
                examinedObjectRigidbody.useGravity = false;
                examinedObjectRigidbody.isKinematic = true;
            }

            // Store initial positions and rotations for each examine camera
            initialPositions = new Vector3[examineCameras.Length];
            initialRotations = new Quaternion[examineCameras.Length];
            for (int i = 0; i < examineCameras.Length; i++)
            {
                initialPositions[i] = currentExaminedObject.transform.position;
                initialRotations[i] = currentExaminedObject.transform.rotation;
            }

            // Position the examined object in front of each examine camera
            for (int i = 0; i < examineCameras.Length; i++)
            {
                Camera examineCamera = examineCameras[i];
                Vector3 cameraPosition = examineCamera.transform.position;
                Vector3 cameraForward = examineCamera.transform.forward;
                currentExaminedObject.transform.position = cameraPosition + cameraForward * examineDistanceObject;
            }

            // Disable player movement while examining
            FindObjectOfType<MouseLook>().enabled = false;
            FindObjectOfType<PlayerMovement>().enabled = false;
            FindObjectOfType<PauseMenu>().enabled = false;

            if (wantEmission)
            {
                // Add EmissionControl script if it doesn't exist
                emissionControl = currentExaminedObject.GetComponent<EmissionControl>();
                if (emissionControl == null)
                {
                    emissionControl = currentExaminedObject.AddComponent<EmissionControl>();
                }

                emissionControl.EnableEmission(true);
            }

            examining = true;
            ExamineCanvas.SetActive(true);
            if (takeable == true)
            {
                TakeButton.SetActive(true);
            }
            if (HaveDescription)
            {
                DescriptionButton.SetActive(true);
            }
        }
    }

    private void StopExamination()
    {
        if (!Description)
        {
            isExamining = false;

            mainCamera.enabled = true;

            var inventoryController = FindObjectOfType<InventoryController>();
            inventoryController.CantOpenInventory = false;

            // Disable the examine cameras
            foreach (Camera examineCamera in examineCameras)
            {
                examineCamera.enabled = false;
                examineCamera.fieldOfView = mainCamera.fieldOfView;
            }

            // Re-enable gravity and unfreeze position/rotation on the examined object's Rigidbody
            if (examinedObjectRigidbody != null)
            {
                examinedObjectRigidbody.useGravity = true;
                examinedObjectRigidbody.isKinematic = false;
            }

            // Restore initial positions and rotations for each examine camera
            for (int i = 0; i < examineCameras.Length; i++)
            {
                Camera examineCamera = examineCameras[i];
                currentExaminedObject.transform.position = initialPositions[i];
                currentExaminedObject.transform.rotation = initialRotations[i];
            }

            // Enable player movement
            FindObjectOfType<MouseLook>().enabled = true;
            FindObjectOfType<PlayerMovement>().enabled = true;
            FindObjectOfType<PauseMenu>().enabled = true;

            if (wantEmission)
            {
                // Add EmissionControl script if it doesn't exist
                emissionControl = currentExaminedObject.GetComponent<EmissionControl>();
                if (emissionControl == null)
                {
                    emissionControl = currentExaminedObject.AddComponent<EmissionControl>();
                }

                emissionControl.EnableEmission(false);
            }

            examining = false;
            ExamineCanvas.SetActive(false);
            canvas.SetActive(true);
            playerMovement.Canvas.SetActive(true);
            if (takeable == true)
            {
                TakeButton.SetActive(false);
            }
            if (HaveDescription)
            {
                DescriptionButton.SetActive(false);
            }
        }
    }

    public void ShowObjectName()
    {
        inspectController.ShowName(ItemName);
    }

    public void HideObjectName()
    {
        inspectController.HideName();
    }

    public void ShowDescription()
    {
        Description = true;
        ExamineCanvas.SetActive(false);
    }

    public void DontShowDescription()
    {
        Description = false;
        ExamineCanvas.SetActive(true);
    }
    public void ShowDescription2()
    {
        if (HaveDescription)
            inspectController.ShowAdditionalInfoExamine(itemextraInfo);
    }
    public void DontShowDescription2()
    {
        if (HaveDescription)
            inspectController.DontShowAdditionalInfoExamine();
    }
    public void AddToInventory()
    {
        if (inventoryItem != null)
        {
            var inventoryController = FindObjectOfType<InventoryController>();
            if (inventoryController != null)
            {
               // Debug.Log("2: currentExaminedObject ID: " + currentExaminedObject.GetInstanceID());
               // Debug.Log("2: associatedObject ID: " + inventoryItem.associatedObject);
                if (currentExaminedObject != null && currentExaminedObject == inventoryItem.associatedObject)
                {
                    if (inventoryController.HasEmptySlot())
                    {
                        inventoryController.AddItemToInventory(inventoryItem);
                        StopExamination();
                        currentExaminedObject.SetActive(false);

                    }
                    else
                    {
                        Debug.Log("Inventory is full. Cannot add item.");
                    }
                }
            }
        }
    }
    public void AddToInventoryWithoutExamine()
    {
        if (inventoryItem != null)
        {
            var inventoryController = FindObjectOfType<InventoryController>();
            if (inventoryController != null)
            {
                if (this.gameObject != null && this.gameObject == inventoryItem.associatedObject)
                {
                    if (inventoryController.HasEmptySlot())
                    {
                        inventoryController.AddItemToInventory(inventoryItem);
                        this.gameObject.SetActive(false);

                    }
                    else
                    {
                        Debug.Log("Inventory is full. Cannot add item.");
                    }
                }
            }
        }
    }
}






