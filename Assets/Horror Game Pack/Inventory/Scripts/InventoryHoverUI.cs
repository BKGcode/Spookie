using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// UI ligera para mostrar el nombre del item bajo el cursor dentro del inventario.
/// Separada de InventoryController para reducir ruido en Update.
/// KISS: sin eventos; se consulta InventoryController cada frame solo cuando inventario está abierto.
/// </summary>
[AddComponentMenu("Spookie/Inventory/Inventory Hover UI")] 
public class InventoryHoverUI : MonoBehaviour
{
    [Header("Refs")]
    [Tooltip("Referencia al InventoryController.")]
    [SerializeField] private InventoryController inventoryController;
    [Tooltip("Texto TMP para mostrar el nombre del item.")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [Tooltip("Offset en píxeles respecto a la posición del cursor.")]
    [SerializeField] private Vector2 cursorOffset = new Vector2(5f, 20f);

    [Header("Config")] 
    [Tooltip("Actualizar aunque no haya cambios de slot (útil si añadirás animaciones después).")]
    [SerializeField] private bool alwaysUpdatePosition = true;

    private Camera uiCamera; // opcional si usas Screen Space - Camera

    private void Awake()
    {
        if (itemNameText != null) itemNameText.gameObject.SetActive(false);
    }

    private void Reset()
    {
        if (inventoryController == null)
        {
            inventoryController = FindObjectOfType<InventoryController>();
        }
    }

    private void LateUpdate()
    {
        if (inventoryController == null || itemNameText == null) return;
        // Consideramos inventario abierto si el canvas está activo
        if (!inventoryController.gameObject.activeInHierarchy) return; // controller activo
        if (!inventoryController.enabled) return;

        // Canvas abierto?
        if (inventoryController.inventoryCanvas == null || !inventoryController.inventoryCanvas.activeSelf)
        {
            if (itemNameText.gameObject.activeSelf) itemNameText.gameObject.SetActive(false);
            return;
        }

        Vector2 mousePos = Input.mousePosition;
        if (alwaysUpdatePosition || itemNameText.gameObject.activeSelf)
        {
            itemNameText.transform.position = mousePos + cursorOffset;
        }

        // Detectar slot
        RawImage[] slots = inventoryController.inventorySlots;
        bool found = false;
        for (int i = 0; i < slots.Length; i++)
        {
            RawImage ri = slots[i];
            if (ri == null) continue;
            if (RectTransformUtility.RectangleContainsScreenPoint(ri.rectTransform, mousePos))
            {
                if (ri.texture != null && inventoryController.TryGetItemAtSlot(i, out var item))
                {
                    itemNameText.text = item.itemName;
                    if (!itemNameText.gameObject.activeSelf) itemNameText.gameObject.SetActive(true);
                }
                else
                {
                    if (itemNameText.gameObject.activeSelf) itemNameText.gameObject.SetActive(false);
                }
                found = true;
                break;
            }
        }
        if (!found && itemNameText.gameObject.activeSelf)
        {
            itemNameText.gameObject.SetActive(false);
        }
    }
}

// ScriptRole: Mostrar tooltip simple de item en inventario
// RelatedScripts: InventoryController
// UsesSO: No
// ReceivesFrom: Input (posición del cursor)
// SendsTo: TMP_Text (itemNameText)
// Adjuntar a: Mismo GameObject del InventoryController o un hijo UI.
// Referencias a asignar: inventoryController (si no autodescubierto), itemNameText (TMP).
