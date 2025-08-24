using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // New Input System
using Game.Core; // CursorStateController

public class InventoryController : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("The canvas representing the inventory UI.")]
    public GameObject inventoryCanvas;
    [Tooltip("The array of RawImage slots representing the inventory.")]
    public RawImage[] inventorySlots;
    [Tooltip("Reference to the TextMeshProUGUI element for displaying item names.")]
    public TextMeshProUGUI itemNameText;
    [Tooltip("Reference to the RawImage used for item dragging.")]
    public RawImage duplicatedRawImage;
    [Tooltip("(LEGACY) The key to toggle the inventory UI if no InputAction is assigned.")]
    public KeyCode inventoryKey = KeyCode.I;
    [Header("Input (New Input System)")]
    [Tooltip("InputAction (Button) para abrir/cerrar inventario. Si está asignada se ignora la tecla legacy.")]
    [SerializeField] private InputActionReference inventoryToggleAction;
    [Tooltip("Mostrar logs de depuración al intentar abrir/cerrar inventario.")]
    [SerializeField] private bool debugToggleLogs = false;

    [Header("Player elements")]
    [Tooltip("The main camera's mouselook script in the scene.")]
    public MouseLook mouseLook;
    [Tooltip("The main camera used in the scene.")]
    public GameObject mainCamera;
    public PlayerMovement playerMovement;
    public PauseMenu pauseMenu;
    public Transform Hand;
    public GameObject HandObject;
    public Transform DefaultParent;
    public Flashlight flashlight;
    public Candle candle;
    public Lantern lantern;
    public Key key;
    [Tooltip("The key to put back the item into the inventory.")]
    public KeyCode PutBackInventoryKey = KeyCode.K;

    [Header("Default Items")]
    [Tooltip("List of default items that appear in the inventory at the start.")]
    public List<DefaultInventoryItem> defaultItems;
    public List<DoorManager> KeyDoors = new List<DoorManager>();
    public List<Keycard> keycards = new List<Keycard>();

    [System.Serializable]
    public class DefaultInventoryItem
    {
        public string itemName;
        public Sprite itemImage;
        public GameObject itemPrefab;
        public float XRotation;
        public float YRotation;
        public float ZRotation;
    }
    private bool dragging = false;
    private Dictionary<int, InventoryItem> inventoryItems = new Dictionary<int, InventoryItem>();
    private Dictionary<int, Vector2> rotationValues = new Dictionary<int, Vector2>();
    private bool isInventoryOpen = false;
    private bool isMouseVisible = true;
    private const float throwSpeed = 10f;
    private Color[] originalSlotColors;
    private int uniqueItemIdCounter = 0;
    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.3f;
    private int slotIndexBeingDragged = -1;
    private RawImage originalRawImage;
    private bool isHoldingItem = false;
    private InventoryItem holdingItem;
    private List<Bolt> bolts = new List<Bolt>();
    [HideInInspector]public bool iskeypadinuse = false;
    [HideInInspector]
    public bool CantOpenInventory = false;

    public void SaveRotationValues(int itemId, float xRotation, float yRotation, float zRotation)
    {
        if (!rotationValues.ContainsKey(itemId))
        {
            rotationValues[itemId] = new Vector3(xRotation, yRotation, zRotation); 
        }
    }


    private void Start()
    {
        inventoryCanvas.SetActive(false);
        originalSlotColors = new Color[inventorySlots.Length];
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            originalSlotColors[i] = inventorySlots[i].color;
        }
        duplicatedRawImage.enabled = false;
        foreach (DefaultInventoryItem defaultItem in defaultItems)
        {
            AddDefaultItemToInventory(defaultItem);
        }
        // Cambio A: eliminar dependencia de Tag "Bolt" inexistente -> usar FindObjectsOfType<Bolt>()
        bolts.Clear();
        var foundBolts = FindObjectsOfType<Bolt>(true); // incluye inactivos por si se activan luego
        for (int i = 0; i < foundBolts.Length; i++)
        {
            if (foundBolts[i] != null) bolts.Add(foundBolts[i]);
        }
    }

    private void OnEnable()
    {
        if (inventoryToggleAction != null)
        {
            try
            {
                inventoryToggleAction.action.performed += OnInventoryTogglePerformed;
                inventoryToggleAction.action.Enable();
            }
            catch { /* ignore double enable */ }
        }
    }

    private void OnDisable()
    {
        if (inventoryToggleAction != null)
        {
            try
            {
                inventoryToggleAction.action.performed -= OnInventoryTogglePerformed;
                inventoryToggleAction.action.Disable();
            }
            catch { /* ignore */ }
        }
    }

    private void OnInventoryTogglePerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (debugToggleLogs) Debug.Log("[InventoryController] InputAction performed -> toggle");
            TryToggleInventory("InputAction");
        }
    }

    private void Update()
    {
        // Fallback legacy key SIEMPRE permitido (evita quedarse sin toggle si la acción no dispara)
        if (Input.GetKeyDown(inventoryKey))
        {
            if (debugToggleLogs) Debug.Log("[InventoryController] Legacy key pressed -> toggle");
            TryToggleInventory("LegacyKey");
        }
        // Lógica de hover + tooltip movida a InventoryHoverUI (se mantiene drag/drop aquí)
        if (isInventoryOpen)
        {
            HandleDragAndDrop();
        }
    if (Input.GetKeyDown(PutBackInventoryKey))
        {
            PutItemBackInInventory();
        }
        for (int i = 1; i <= 4; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                PickUpItem(i - 1);
            }
        }
    }

    private void TryToggleInventory(string reason)
    {
        if (CantOpenInventory) return;
        isInventoryOpen = !isInventoryOpen;
        inventoryCanvas.SetActive(isInventoryOpen);

        // Activar/desactivar control de jugador
        if (mouseLook != null) mouseLook.enabled = !isInventoryOpen;
        if (playerMovement != null) playerMovement.enabled = !isInventoryOpen;
        if (pauseMenu != null) pauseMenu.enabled = !isInventoryOpen;

        // Cursor via controlador central
        if (isInventoryOpen)
        {
            CursorStateController.Instance?.SetFree("InventoryOpen:" + reason);
        }
        else
        {
            CursorStateController.Instance?.SetLocked("InventoryClose:" + reason);
        }
        isMouseVisible = isInventoryOpen; // mantenido por compatibilidad con lógica existente
    }

    public bool TryGetItemAtSlot(int slotIndex, out InventoryItem item)
    {
        return inventoryItems.TryGetValue(slotIndex, out item);
    }

    private void HandleDragAndDrop()
    {
        // Sólo ejecutar si tenemos el texto (dep está en hover script) y slots válidos
        Vector2 mousePosition = Input.mousePosition;

        if (Input.GetMouseButtonDown(1))
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                RawImage rawImage = inventorySlots[i];
                if (rawImage != null && RectTransformUtility.RectangleContainsScreenPoint(rawImage.rectTransform, mousePosition))
                {
                    if (inventoryItems.ContainsKey(i)) PickUpItem(i);
                    break;
                }
            }
        }

        // Double click detection + start drag
        if (Input.GetMouseButtonDown(0))
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                RawImage rawImage = inventorySlots[i];
                if (rawImage != null && RectTransformUtility.RectangleContainsScreenPoint(rawImage.rectTransform, mousePosition))
                {
                    if (Time.time - lastClickTime < doubleClickThreshold)
                    {
                        ThrowItem(i);
                        return;
                    }
                    lastClickTime = Time.time;

                    // Start drag
                    slotIndexBeingDragged = i;
                    originalRawImage = rawImage;
                    if (inventoryItems.ContainsKey(slotIndexBeingDragged))
                    {
                        InventoryItem item = inventoryItems[slotIndexBeingDragged];
                        duplicatedRawImage.enabled = true;
                        duplicatedRawImage.texture = item.itemImage.texture;
                        duplicatedRawImage.rectTransform.sizeDelta = rawImage.rectTransform.sizeDelta;
                        float imageAspect = (float)item.itemImage.texture.width / item.itemImage.texture.height;
                        if (imageAspect > 1f)
                        {
                            float xOffset = (1f - 1f / imageAspect) * 0.5f;
                            duplicatedRawImage.uvRect = new Rect(xOffset, 0f, 1f / imageAspect, 1f);
                        }
                        else
                        {
                            float yOffset = (1f - imageAspect) * 0.5f;
                            duplicatedRawImage.uvRect = new Rect(0f, yOffset, 1f, imageAspect);
                        }
                        duplicatedRawImage.color = Color.white;
                        dragging = true;
                        originalRawImage.enabled = false;
                    }
                    break;
                }
            }
        }

        if (dragging)
        {
            duplicatedRawImage.rectTransform.position = mousePosition;
        }

        if (Input.GetMouseButtonUp(0) && slotIndexBeingDragged != -1)
        {
            // Detect drop target
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                RawImage rawImage = inventorySlots[i];
                if (rawImage != null && RectTransformUtility.RectangleContainsScreenPoint(rawImage.rectTransform, mousePosition))
                {
                    dragging = false;
                    if (i != slotIndexBeingDragged)
                    {
                        SwapItems(slotIndexBeingDragged, i, i);
                        ApplyAppearanceSettingsToItem(slotIndexBeingDragged);
                        ApplyAppearanceSettingsToItem(i);
                    }
                    else
                    {
                        // restore original
                        duplicatedRawImage.enabled = false;
                        originalRawImage.enabled = true;
                        if (inventoryItems.ContainsKey(i))
                        {
                            InventoryItem targetItem = inventoryItems[i];
                            RawImage ri = inventorySlots[i];
                            float imageAspect = (float)targetItem.itemImage.texture.width / targetItem.itemImage.texture.height;
                            if (imageAspect > 1f)
                            {
                                float xOffset = (1f - 1f / imageAspect) * 0.5f;
                                ri.uvRect = new Rect(xOffset, 0f, 1f / imageAspect, 1f);
                            }
                            else
                            {
                                float yOffset = (1f - imageAspect) * 0.5f;
                                ri.uvRect = new Rect(0f, yOffset, 1f, imageAspect);
                            }
                            ri.color = Color.white;
                        }
                    }
                    slotIndexBeingDragged = -1;
                    return;
                }
            }

            // No slot: throw
            ThrowItem(slotIndexBeingDragged);
            slotIndexBeingDragged = -1;
            dragging = false;
            originalRawImage.enabled = true;
            duplicatedRawImage.enabled = false;
        }
    }

    private void PickUpItem(int slotIndex)
    {
        if (inventoryItems.ContainsKey(slotIndex) && iskeypadinuse == false)
        {
            if(!HasEmptySlot() && isHoldingItem) 
            { 
                return; 
            }

            if (isHoldingItem)
            {
                PutItemBackInInventory();
            }
            

            // Get the item from the slot
            InventoryItem item = inventoryItems[slotIndex];

            // Deactivate the associated object
            item.associatedObject.SetActive(true);

            // Set the RawImage color of the slot to default
            RawImage rawImage = inventorySlots[slotIndex].GetComponent<RawImage>();
            rawImage.color = originalSlotColors[slotIndex];

            // Remove the item from the inventory
            inventoryItems.Remove(slotIndex);
            inventorySlots[slotIndex].texture = null;

            // Update the uniqueItemIdCounter to reflect the next available unique item ID
            if (slotIndex < uniqueItemIdCounter)
            {
                uniqueItemIdCounter = slotIndex;
            }

            // Set the position and rotation of the object
            item.associatedObject.transform.position = Hand.transform.position + Hand.transform.forward * 0.5f;

            Vector3 cameraForward = Camera.main.transform.forward;

            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
            targetRotation.eulerAngles = new Vector3(item.XRotation, targetRotation.eulerAngles.y + item.YRotation, item.ZRotation);

            item.associatedObject.transform.rotation = targetRotation;

            item.associatedObject.transform.parent = Hand;
            Rigidbody rb = item.associatedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false; 
            }
            BoxCollider bc = item.associatedObject.GetComponent<BoxCollider>();
            if (bc != null)
            {
                bc.enabled = false;
            }

            // Set the player to hold the item
            isHoldingItem = true;

            // Assign the picked up item to holdingItem
            holdingItem = item;

            if (item.associatedObject.tag == "Flashlight")
            {
                flashlight.ToggleFlashlight(!flashlight.usingFlashlight, true);
                HandObject.SetActive(false);
            }
            if(item.associatedObject.tag == "Candle")
            {
                candle.ToggleCandle(!candle.usingCandle, true);
                HandObject.SetActive(false);
            }
            if (item.associatedObject.tag == "Lantern")
            {
                lantern.ToggleCandle(!lantern.usingCandle, true);
                HandObject.SetActive(false);
            }
            if (holdingItem.associatedObject.tag.Contains("Key"))
            {
                foreach (var door in KeyDoors)
                {
                    if (door.keyTag == holdingItem.associatedObject.tag)
                    {
                        door.hasKey = true;
                        key.KeyTakeOutSound();
                    }
                }
            }
            if (holdingItem.associatedObject.tag == "Crowbar")
            {
                foreach (var Bolt in bolts)
                {
                    Bolt.CanScrew = true;
                }
            }
            if (holdingItem.associatedObject.tag == "Keycard")
            {
                Keycard heldKeycard = holdingItem.associatedObject.GetComponent<Keycard>();
                if (heldKeycard != null)
                {
                    heldKeycard.EnableKeypadCard();
                }
            }
        }
    }
    public void PutItemBackInInventory()
    {
        if (isHoldingItem && HasEmptySlot() && iskeypadinuse == false)
        {
            // Add the holdingItem back to the inventory
            AddItemToInventory(holdingItem);
            holdingItem.associatedObject.SetActive(false);
            holdingItem.associatedObject.transform.parent = DefaultParent.transform.parent;
            Rigidbody rb = holdingItem.associatedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = true; 
            }
            BoxCollider bc = holdingItem.associatedObject.GetComponent<BoxCollider>();
            if (bc != null)
            {
                bc.enabled = true;
            }
            if (holdingItem.associatedObject.tag == "Flashlight")
            {
                flashlight.ToggleFlashlight(!flashlight.usingFlashlight, true);
                HandObject.SetActive(true);
            }
            if (holdingItem.associatedObject.tag == "Candle")
            {
                candle.ToggleCandle(!candle.usingCandle, true);
                HandObject.SetActive(true);
            }
            if (holdingItem.associatedObject.tag == "Lantern")
            {
                lantern.ToggleCandle(!lantern.usingCandle, true);
                HandObject.SetActive(true);
            }
            if (holdingItem.associatedObject.tag.Contains("Key"))
            {
                foreach (var door in KeyDoors)
                {
                    if (door.keyTag == holdingItem.associatedObject.tag)
                    {
                        door.hasKey = false;
                        key.KeyTakeBackSound();
                    }
                }
            }
            if (holdingItem.associatedObject.tag == "Crowbar")
            {
                foreach(var Bolt in bolts)
                {
                    Bolt.CanScrew = false;
                }
            }
            if (holdingItem.associatedObject.tag == "Keycard")
            {
                Keycard heldKeycard = holdingItem.associatedObject.GetComponent<Keycard>();
                if (heldKeycard != null)
                {
                    heldKeycard.DisableKeypadCard();
                }
            }

            // Reset the holdingItem variable and isHoldingItem flag
            holdingItem = null;
            isHoldingItem = false;
        }
    }
    private void SwapItems(int indexA, int indexB, int slotIndex)
    {
        if (indexA == indexB)
        {
            // Same slot, do nothing
            return;
        }
        int itemId = slotIndex;
        // Check if the slots have items
        bool hasItemA = inventoryItems.ContainsKey(indexA);
        bool hasItemB = inventoryItems.ContainsKey(indexB);

        if (hasItemA && hasItemB)
        {
            // Swap items in non-empty slots
            InventoryItem itemA = inventoryItems[indexA];
            InventoryItem itemB = inventoryItems[indexB];

            inventoryItems[indexA] = itemB;
            inventoryItems[indexB] = itemA;

            Texture tempTexture = inventorySlots[indexA].texture;
            inventorySlots[indexA].texture = inventorySlots[indexB].texture;
            inventorySlots[indexB].texture = tempTexture;

            int tempItemId = itemA.itemId;
            itemA.itemId = itemB.itemId;
            itemB.itemId = tempItemId;
        }
        else if (hasItemA && !hasItemB)
        {
            InventoryItem itemA = inventoryItems[indexA];

            inventoryItems.Remove(indexA);
            inventoryItems.Remove(slotIndex);
            inventoryItems[indexB] = itemA;

            // Reset the RawImage color of the source slot
            RawImage rawImageA = inventorySlots[indexA].GetComponent<RawImage>();
            rawImageA.color = originalSlotColors[indexA];
            inventorySlots[indexA].texture = null;

            // Set the RawImage color of the target slot to default
            RawImage rawImageB = inventorySlots[indexB].GetComponent<RawImage>();
            rawImageB.color = originalSlotColors[indexB];
            inventorySlots[indexB].texture = itemA.itemImage.texture;
            RawImage rawImage = inventorySlots[itemId].GetComponent<RawImage>();
            rawImage.color = Color.white;
            itemA.itemId = indexB;
        }
        else if (!hasItemA && hasItemB)
        {
            // Move item from indexB to an empty indexA
            InventoryItem itemB = inventoryItems[indexB];

            inventoryItems.Remove(indexB);

            inventoryItems[indexA] = itemB;

            // Reset the RawImage color of the source slot
            RawImage rawImageB = inventorySlots[indexB].GetComponent<RawImage>();
            rawImageB.color = originalSlotColors[indexB];
            inventorySlots[indexB].texture = null;

            // Set the RawImage color of the target slot to default
            RawImage rawImageA = inventorySlots[indexA].GetComponent<RawImage>();
            rawImageA.color = originalSlotColors[indexA];
            inventorySlots[indexA].texture = itemB.itemImage.texture;
            itemB.itemId = indexA;
        }
    }

    private void ApplyAppearanceSettingsToItem(int itemIndex)
    {
        if (itemIndex >= 0 && itemIndex < inventorySlots.Length)
        {
            RawImage rawImage = inventorySlots[itemIndex].GetComponent<RawImage>();
            if (rawImage != null && inventoryItems.ContainsKey(itemIndex))
            {
                InventoryItem item = inventoryItems[itemIndex];

                // Set the texture property to the item's image
                rawImage.texture = item.itemImage.texture;

                // Calculate the aspect ratio of the item's image
                float imageAspect = (float)item.itemImage.texture.width / item.itemImage.texture.height;

                // Calculate the UV rect for displaying the entire image without squashing
                if (imageAspect > 1f) 
                {
                    float xOffset = (1f - 1f / imageAspect) * 0.5f;
                    rawImage.uvRect = new Rect(xOffset, 0f, 1f / imageAspect, 1f);
                }
                else
                {
                    float yOffset = (1f - imageAspect) * 0.5f;
                    rawImage.uvRect = new Rect(0f, yOffset, 1f, imageAspect);
                }

                // Set the color to white
                rawImage.color = Color.white;
            }
        }
    }
    private void ThrowItem(int slotIndex)
    {
        if (itemNameText != null)
        {
            // Use the slot index as the item ID
            int itemId = slotIndex;

            if (inventoryItems.ContainsKey(itemId))
            {
                InventoryItem item = inventoryItems[itemId];

                if (item != null)
                {
                    // Deactivate the associated object
                    item.associatedObject.SetActive(true);

                    Rigidbody rb = item.associatedObject.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        // Calculate the throw direction in front of the camera
                        Vector3 throwDirection = mainCamera.transform.forward;

                        // Set the position of the dropped object in front of the camera
                        Vector3 dropPosition = mainCamera.transform.position + mainCamera.transform.forward * 2f + Vector3.up * 0.5f;
                        item.associatedObject.transform.position = dropPosition;

                        
                        rb.isKinematic = false;
                        rb.linearVelocity = throwDirection * throwSpeed;
                        rb.useGravity = true;
                    }

                    // Reset the RawImage color to default
                    RawImage rawImage = inventorySlots[slotIndex].GetComponent<RawImage>();
                    rawImage.color = originalSlotColors[slotIndex];

                    // Remove the item from the inventory
                    inventoryItems.Remove(itemId);
                    inventorySlots[slotIndex].texture = null;

                    // Update the uniqueItemIdCounter to reflect the next available unique item ID
                    if (itemId < uniqueItemIdCounter)
                    {
                        uniqueItemIdCounter = itemId;
                    }
                }
            }
        }
    }
    public void AddItemToInventory(InventoryItem item)
    {
        // Find an empty slot to place the item
        int emptySlotIndex = -1;
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (!inventoryItems.ContainsKey(i))
            {
                emptySlotIndex = i;
                break;
            }
        }

        if (emptySlotIndex != -1)
        {
            // Assign a unique item ID
            int itemId = uniqueItemIdCounter;
            uniqueItemIdCounter++;

            // Store the item in the inventory using the unique ID
            inventoryItems[emptySlotIndex] = item;

            // Assign the item's image to the corresponding slot
            inventorySlots[emptySlotIndex].texture = item.itemImage.texture;

            SaveRotationValues(itemId, item.XRotation, item.YRotation, item.ZRotation);

            // Get the RawImage component of the slot
            RawImage rawImage = inventorySlots[emptySlotIndex].GetComponent<RawImage>();
            if (rawImage != null)
            {
                // Set the texture property to the item's image
                rawImage.texture = item.itemImage.texture;

                // Calculate the aspect ratio of the item's image
                float imageAspect = (float)item.itemImage.texture.width / item.itemImage.texture.height;

                // Calculate the UV rect for displaying the entire image without squashing
                if (imageAspect > 1f) 
                {
                    float xOffset = (1f - 1f / imageAspect) * 0.5f;
                    rawImage.uvRect = new Rect(xOffset, 0f, 1f / imageAspect, 1f);
                }
                else 
                {
                    float yOffset = (1f - imageAspect) * 0.5f;
                    rawImage.uvRect = new Rect(0f, yOffset, 1f, imageAspect);
                }

                // Set the color to white
                rawImage.color = Color.white;
            }

            // Set the item's unique ID for reference
            item.itemId = emptySlotIndex;
        }
        else
        {
            // Handle the case when there are no empty slots (inventory full)
            Debug.Log("Inventory is full. Cannot add item.");
        }
    }
    private void AddDefaultItemToInventory(DefaultInventoryItem defaultItem)
    {
        // Find an empty slot to place the item
        int emptySlotIndex = -1;
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (!inventoryItems.ContainsKey(i))
            {
                emptySlotIndex = i;
                break;
            }
        }

        if (emptySlotIndex != -1)
        {
            // Assign a unique item ID
            int itemId = uniqueItemIdCounter;
            uniqueItemIdCounter++;

            // Create a new InventoryItem from the default item settings
            InventoryItem newItem = new InventoryItem();
            newItem.itemId = emptySlotIndex;
            newItem.itemName = defaultItem.itemName;
            newItem.itemImage = defaultItem.itemImage;
            newItem.associatedObject = defaultItem.itemPrefab;

            newItem.XRotation = defaultItem.XRotation;
            newItem.ZRotation = defaultItem.ZRotation;
            newItem.YRotation = defaultItem.YRotation;

            SaveRotationValues(itemId, newItem.XRotation, newItem.YRotation,  newItem.ZRotation);

            // Store the item in the inventory using the unique ID
            inventoryItems[emptySlotIndex] = newItem;

            // Assign the item's image to the corresponding slot
            inventorySlots[emptySlotIndex].texture = newItem.itemImage.texture;

            // Get the RawImage component of the slot
            RawImage rawImage = inventorySlots[emptySlotIndex].GetComponent<RawImage>();
            if (rawImage != null)
            {
                // Set the texture property to the item's image
                rawImage.texture = newItem.itemImage.texture;

                // Calculate the aspect ratio of the item's image
                float imageAspect = (float)newItem.itemImage.texture.width / newItem.itemImage.texture.height;

                // Calculate the UV rect for displaying the entire image without squashing
                if (imageAspect > 1f) 
                {
                    float xOffset = (1f - 1f / imageAspect) * 0.5f;
                    rawImage.uvRect = new Rect(xOffset, 0f, 1f / imageAspect, 1f);
                }
                else // Image is taller
                {
                    float yOffset = (1f - imageAspect) * 0.5f;
                    rawImage.uvRect = new Rect(0f, yOffset, 1f, imageAspect);
                }

                // Set the color to white
                rawImage.color = Color.white;
            }

            // Set the item's unique ID for reference
            newItem.itemId = emptySlotIndex;
        }
        else
        {
            // Handle the case when there are no empty slots (inventory full)
            Debug.Log("Inventory is full. Cannot add default item: " + defaultItem.itemName);
        }
    }
    public bool HasEmptySlot()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (!inventoryItems.ContainsKey(i))
            {
                return true; // Found an empty slot
            }
        }
        return false; // No empty slots found
    }
}

