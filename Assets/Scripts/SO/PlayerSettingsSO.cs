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
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float maxLookAngle = 80f;
        
        [Header("Crouch Settings")]
        [SerializeField] private float crouchHeight = 1f;
        [SerializeField] private float standHeight = 2f;
        [SerializeField] private float crouchSpeed = 2.5f;
        
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
        public float CrouchHeight => crouchHeight;
        public float StandHeight => standHeight;
        public float CrouchSpeed => crouchSpeed;
        public float InteractionRange => interactionRange;
        public LayerMask InteractableLayers => interactableLayers;
    }
}
