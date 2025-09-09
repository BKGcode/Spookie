
using UnityEngine;
using TMPro;

namespace Brainamics.UI.Prototypes
{
    public class ModalWindow : UIElement
    {
        [Header("Componentes")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private CustomButton confirmButton;
        [SerializeField] private CustomButton cancelButton;
        [SerializeField] private CustomButton closeButton; // Para popups de solo info

        public void ShowInfoWindow(string title, string message)
        {
            titleText.text = title;
            messageText.text = message;
            confirmButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);
            closeButton.gameObject.SetActive(true);
            gameObject.SetActive(true);
        }

        public void ShowConfirmationWindow(string title, string message, UnityEngine.Events.UnityAction onConfirm, UnityEngine.Events.UnityAction onCancel)
        {
            titleText.text = title;
            messageText.text = message;
            confirmButton.gameObject.SetActive(true);
            cancelButton.gameObject.SetActive(true);
            closeButton.gameObject.SetActive(false);

            confirmButton.OnClick.RemoveAllListeners();
            confirmButton.OnClick.AddListener(onConfirm);
            confirmButton.OnClick.AddListener(Hide);

            cancelButton.OnClick.RemoveAllListeners();
            cancelButton.OnClick.AddListener(onCancel);
            cancelButton.OnClick.AddListener(Hide);

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }

    // ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
    // Role: Controla la lógica de una ventana modal para diferentes casos de uso.
    // Related: CustomButton, UIElement
    // UsesSO: N/A
    // ReceivesFrom: Sistemas que la invocan con ShowInfoWindow o ShowConfirmationWindow.
    // SendsTo: N/A (acciones se pasan por delegados).
}
