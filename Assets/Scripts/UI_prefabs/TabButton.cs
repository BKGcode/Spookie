
using UnityEngine;

namespace Brainamics.UI.Prototypes
{
    [RequireComponent(typeof(CustomButton))]
    public class TabButton : UIElement
    {
        [Header("Configuración de Pestaña")]
        [Tooltip("El panel de contenido que este botón debe activar.")]
        [SerializeField] public GameObject contentPanel;

        private CustomButton _button;
        private TabView _parentTabView;

        public void Initialize(TabView parentView)
        {
            _parentTabView = parentView;
            _button = GetComponent<CustomButton>();
            _button.OnClick.AddListener(OnTabSelected);
        }

        private void OnTabSelected()
        {
            if (showLogs) Debug.Log($"[TabButton] Pestaña seleccionada: {gameObject.name}");
            _parentTabView.OnTabSelected(this);
        }

        public void SetSelected(bool isSelected)
        {
            // Aquí se podría cambiar el estado visual del botón (color, sprite, etc.)
            // Por simplicidad, lo dejamos así para el prototipo.
            if (contentPanel != null) 
            {
                contentPanel.SetActive(isSelected);
            }
        }
    }

    // ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
    // Role: Controla un botón de pestaña individual.
    // Related: TabView, CustomButton
    // UsesSO: N/A
    // ReceivesFrom: Clic del usuario.
    // SendsTo: TabView (notifica selección).
}
