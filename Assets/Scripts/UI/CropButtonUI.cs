using UnityEngine;
using UnityEngine.UI; // Para Button y Image

[RequireComponent(typeof(Button), typeof(Image))]
public class CropButtonUI : MonoBehaviour
{
    [Header("Visualización Selección")]
    [Tooltip("Elemento visual que indica si este botón está seleccionado (ej. un borde resaltado). Opcional.")]
    [SerializeField] private GameObject selectionHighlight;

    // Referencias internas
    private Button button;
    private Image buttonImage; // Para mostrar el icono del cultivo
    private CropSO representedCrop;
    private FarmingManager farmingManager;

    public CropSO RepresentedCrop => representedCrop; // Propiedad para que ToolbarUI sepa qué crop representa

    void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>(); // Asumimos que la imagen principal del botón muestra el icono

        if (button == null) Debug.LogError("CropButtonUI: No se encontró componente Button.");
        if (buttonImage == null) Debug.LogError("CropButtonUI: No se encontró componente Image.");
        if (selectionHighlight != null) selectionHighlight.SetActive(false); // Ocultar al inicio

        button.onClick.AddListener(HandleClick);
    }

    // Llamado por CropToolbarUI al instanciar el botón
    public void Initialize(CropSO crop, FarmingManager manager)
    {
        if (crop == null || manager == null)
        {
            Debug.LogError("CropButtonUI: Intento de inicializar con CropSO o FarmingManager nulo.");
            gameObject.SetActive(false); // Desactivar botón si no se puede inicializar bien
            return;
        }

        representedCrop = crop;
        farmingManager = manager;

        if (buttonImage != null && crop.Icon != null)
        {
            buttonImage.sprite = crop.Icon;
        }
        else if(buttonImage != null)
        {
             Debug.LogWarning($"CropButtonUI: CropSO {crop.name} no tiene un icono asignado.");
        }

        gameObject.name = $"CropButton_{crop.name}"; // Cambiar nombre para debug
    }

    private void HandleClick()
    {
        if (farmingManager != null && representedCrop != null)
        {
            // Si ya estaba seleccionado, hacer clic de nuevo lo deselecciona (sale del modo plantar)
            if (farmingManager.GetSelectedCropForPlanting() == representedCrop)
            {
                farmingManager.ExitPlantingMode();
                 Debug.Log($"CropButtonUI ({representedCrop.name}): Clic para deseleccionar.");
            }
            else // Si no, selecciona este cultivo
            {
                farmingManager.SelectCropForPlanting(representedCrop);
                 Debug.Log($"CropButtonUI ({representedCrop.name}): Clic para seleccionar.");
            }
        }
    }

    // Llamado por CropToolbarUI para actualizar el estado visual
    public void SetSelected(bool isSelected)
    {
        if (selectionHighlight != null)
        {
            selectionHighlight.SetActive(isSelected);
        }
        // Podrías añadir más feedback visual aquí (cambiar color del botón, etc.)
    }

    void OnDestroy()
    {
        if(button != null) button.onClick.RemoveListener(HandleClick);
    }
}

// ScriptRole: Controla un botón individual en la barra de cultivos, representa un CropSO y notifica al FarmingManager al ser pulsado.
// RelatedScripts: CropToolbarUI, FarmingManager, CropSO
// UsesSO: CropSO
// ReceivesFrom: CropToolbarUI (Initialize, SetSelected), User Input (clicks)
// SendsTo: FarmingManager (SelectCropForPlanting, ExitPlantingMode) 