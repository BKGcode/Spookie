
using UnityEngine;
using System.Collections.Generic;

namespace Brainamics.UI.Prototypes
{
    public class TabView : UIElement
    {
        [Header("Configuración de Pestañas")]
        [Tooltip("Lista de los botones de pestaña que controla este grupo.")]
        [SerializeField] private List<TabButton> tabButtons = new List<TabButton>();
        [Tooltip("Índice de la pestaña que se mostrará por defecto al iniciar.")]
        [SerializeField] private int defaultTabIndex = 0;

        private void Start()
        {
            foreach (var button in tabButtons)
            {
                button.Initialize(this);
            }

            if (tabButtons.Count > 0 && defaultTabIndex < tabButtons.Count)
            {
                OnTabSelected(tabButtons[defaultTabIndex]);
            }
        }

        public void OnTabSelected(TabButton selectedButton)
        {
            foreach (var button in tabButtons)
            {
                button.SetSelected(button == selectedButton);
            }
        }

        private void OnValidate()
        {
            if (tabButtons.Count == 0)
            {
                GetComponentsInChildren<TabButton>(true, tabButtons);
            }
        }
    }

    // ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
    // Role: Gestiona un grupo de pestañas, mostrando un solo contenido a la vez.
    // Related: TabButton
    // UsesSO: N/A
    // ReceivesFrom: TabButton (al ser seleccionado)
    // SendsTo: TabButton (para activar/desactivar su estado y panel)
}
