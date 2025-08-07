using UnityEngine;

namespace PlayerController
{
    public class MouseLook : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerSettingsSO playerSettings;
        [SerializeField] private Transform playerBody;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Rotation variables
        private float xRotation = 0f;
        private float yRotation = 0f;
        
        // Input variables
        private Vector2 mouseInput;
        
        private void Start()
        {
            ValidateReferences();
            
            if (showDebugLogs)
                Debug.Log($"[MouseLook] Initialized on {gameObject.name}");
        }
        
        private void Update()
        {
            HandleMouseInput();
            ApplyRotation();
        }
        
        private void HandleMouseInput()
        {
            // Get mouse input
            mouseInput.x = Input.GetAxis("Mouse X");
            mouseInput.y = Input.GetAxis("Mouse Y");
            
            // Apply sensitivity
            mouseInput *= playerSettings.MouseSensitivity;
        }
        
        private void ApplyRotation()
        {
            // Horizontal rotation (player body)
            if (playerBody != null)
            {
                yRotation += mouseInput.x;
                playerBody.localRotation = Quaternion.Euler(0f, yRotation, 0f);
            }
            
            // Vertical rotation (camera only)
            xRotation -= mouseInput.y;
            xRotation = Mathf.Clamp(xRotation, -playerSettings.MaxLookAngle, playerSettings.MaxLookAngle);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
        
        private void ValidateReferences()
        {
            if (playerSettings == null)
            {
                Debug.LogError("[MouseLook] PlayerSettingsSO reference is missing!");
            }
            
            if (playerBody == null)
            {
                Debug.LogWarning("[MouseLook] Player Body Transform reference is missing - only camera will rotate");
            }
        }
        
        // Public methods for external access
        public float GetXRotation() => xRotation;
        public float GetYRotation() => yRotation;
        public Vector2 GetMouseInput() => mouseInput;
        
        // Method to set rotation (useful for cutscenes or external control)
        public void SetRotation(float xRot, float yRot)
        {
            xRotation = Mathf.Clamp(xRot, -playerSettings.MaxLookAngle, playerSettings.MaxLookAngle);
            yRotation = yRot;
            
            if (playerBody != null)
                playerBody.localRotation = Quaternion.Euler(0f, yRotation, 0f);
            
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }
}

// ScriptRole: Handles mouse look for camera rotation and player body rotation
// RelatedScripts: PlayerMovement
// UsesSO: PlayerSettingsSO
// ReceivesFrom: Input System (Mouse)
// SendsTo: Camera Transform, Player Body Transform
