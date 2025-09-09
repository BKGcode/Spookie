
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace Brainamics.UI.Prototypes
{
    [RequireComponent(typeof(Button))]
    public class CustomButton : UIElement
    {
        [Header("Configuración")]
        [Tooltip("Referencia al SO de mensajes para obtener el texto del botón.")]
        [SerializeField] private FeedbackMessagesSO messagesSO;
        [Tooltip("Clave del texto a mostrar en el botón.")]
        [SerializeField] private string textKey;

        [Header("Componentes (Opcional)")]
        [Tooltip("Referencia al TextMeshPro para el texto. Si es nulo, lo busca en hijos.")]
        [SerializeField] private TextMeshProUGUI buttonText;
        [Tooltip("Referencia a la imagen para un icono. Opcional.")]
        [SerializeField] private Image iconImage;

        [Header("Eventos")]
        [Tooltip("Canal de evento a invocar al hacer clic. Opcional.")]
        [SerializeField] private VoidEventChannelSO onClickChannel;
        [Tooltip("Evento de Unity para cablear acciones en el Inspector.")]
        public UnityEvent OnClick;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (buttonText == null) buttonText = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(HandleClick);
            UpdateText();
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(HandleClick);
        }

        private void HandleClick()
        {
            if (showLogs) Debug.Log($"[CustomButton] Botón '{gameObject.name}' presionado.");
            onClickChannel?.RaiseEvent();
            OnClick?.Invoke();
        }

        public void UpdateText()
        {
            if (messagesSO != null && buttonText != null && !string.IsNullOrEmpty(textKey))
            {
                buttonText.text = messagesSO.GetMessage(textKey, buttonText.text);
            }
        }

        private void OnValidate()
        {
            if (buttonText == null) buttonText = GetComponentInChildren<TextMeshProUGUI>();
            UpdateText();
        }
    }

    // ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
    // Role: Gestiona la lógica de un botón, incluyendo texto y eventos.
    // Related: UIElement, FeedbackMessagesSO, VoidEventChannelSO
    // UsesSO: FeedbackMessagesSO (para leer textos)
    // ReceivesFrom: N/A (recibe clic del usuario)
    // SendsTo: VoidEventChannelSO (al hacer clic), UnityEvent OnClick
}
