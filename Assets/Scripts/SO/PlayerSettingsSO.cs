using UnityEngine;

namespace PlayerController
{
    [CreateAssetMenu(fileName = "PlayerSettings", menuName = "Spookie/Player Settings")]
    public class PlayerSettingsSO : ScriptableObject
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private float gravity = -20f;
        
        [Header("Mouse Look Settings")]
		[Tooltip("Sensibilidad del ratón normalizada 0..1 para UI. 0.3 ≈ baja, 0.6 ≈ media, 1.0 ≈ alta.")]
        [Range(0f, 1f)]
        [SerializeField] private float mouseSensitivity = 0.3f;
        [Tooltip("Escala adicional para el eje vertical del ratón (0..1). Útil para bajar un poco la sensibilidad vertical.")]
        [Range(0f, 1f)]
        [SerializeField] private float mouseYScale = 0.85f;
        [SerializeField] private float maxLookAngle = 80f;
    [Tooltip("Si está activo, el eje vertical del ratón se invierte (arriba=mirar abajo). Por defecto desactivado para estilo FPS clásico.")]
    [SerializeField] private bool invertVerticalLook = false;
    [Tooltip("Easing simple para la cámara. 0 = sin easing, 1 = easing fuerte. Recomendado 0.1–0.3.")]
    [Range(0f, 1f)]
    [SerializeField] private float lookEasingAmount = 0.2f;
    [Tooltip("Activa el easing simple de la cámara.")]
    [SerializeField] private bool lookEasingEnabled = true;
    [Tooltip("Multiplicador extra global para la sensibilidad del look (aplicado encima de MouseSensitivity). 1 = sin cambio.")]
    [SerializeField] private float lookSensitivityMultiplier = 1f;
        
        [Header("Crouch Settings")]
        [SerializeField] private float crouchHeight = 1f;
        [SerializeField] private float standHeight = 2f;
        [SerializeField] private float crouchSpeed = 2.5f;

        [Header("Crouch Transition Settings")]
        [Tooltip("Duración de la transición agachado/de pie. 0 = instantáneo.")]
        [SerializeField] private float crouchTransitionDuration = 0.25f;
        [Tooltip("Curva de easing para mezclar entre alturas (0=de pie, 1=agachado).")]
        [SerializeField] private AnimationCurve crouchEaseCurve = null;

        [Header("Movement Easing Settings")]
        [Tooltip("Frenado extra de velocidad cerca del inicio y fin del movimiento de agachado (0..1).")]
        [Range(0f, 1f)]
        [SerializeField] private float speedEdgeSlowPotency = 0.3f;
        
        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private LayerMask interactableLayers = -1;
        
        // Public properties for easy access
        public float WalkSpeed => walkSpeed;
        public float SprintSpeed => sprintSpeed;
        public float JumpForce => jumpForce;
        public float Gravity => gravity;
        public float MouseSensitivity => mouseSensitivity;
        public float MaxLookAngle => maxLookAngle;
    public bool InvertVerticalLook => invertVerticalLook;
    public float LookEasingAmount => lookEasingAmount;
    public bool LookEasingEnabled => lookEasingEnabled;
    public float MouseYScale => mouseYScale;
    public float LookSensitivityMultiplier => lookSensitivityMultiplier;
        public float CrouchHeight => crouchHeight;
        public float StandHeight => standHeight;
        public float CrouchSpeed => crouchSpeed;
        public float CrouchTransitionDuration => crouchTransitionDuration;
    public AnimationCurve CrouchEaseCurve => crouchEaseCurve != null ? crouchEaseCurve : AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public float SpeedEdgeSlowPotency => speedEdgeSlowPotency;
        public float InteractionRange => interactionRange;
        public LayerMask InteractableLayers => interactableLayers;
    }
}
