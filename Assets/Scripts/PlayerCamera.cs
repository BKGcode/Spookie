using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private PlayerSettingsSO playerSettings;
    
    [Header("References")]
    [SerializeField] private Transform playerBody;
    
    private float xRotation = 0f;
    private Vector2 lookInput;
    
    private void Awake()
    {
        if (playerBody == null)
        {
            playerBody = transform.parent;
            Debug.LogWarning("Player body not assigned to PlayerCamera, using parent transform");
        }
        
        Debug.Log("PlayerCamera initialized");
    }
    
    private void Start()
    {
        // Lock cursor for FPS experience
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void Update()
    {
        HandleMouseLook();
    }
    
    private void HandleMouseLook()
    {
        if (lookInput.sqrMagnitude < 0.01f) return;
        
        float mouseSensitivity = playerSettings != null ? playerSettings.MouseSensitivity : 2f;
        float maxLookAngle = playerSettings != null ? playerSettings.MaxLookAngle : 80f;
        
        // Get mouse input
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;
        
        // Rotate camera up/down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        // Rotate player body left/right
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
    
    public void SetLookInput(Vector2 input)
    {
        lookInput = input;
    }
    
    public void SetCursorLock(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
        
        Debug.Log($"Cursor lock: {locked}");
    }
    
    public void ToggleCursorLock()
    {
        bool isLocked = Cursor.lockState == CursorLockMode.Locked;
        SetCursorLock(!isLocked);
    }
}

// ScriptRole: Handles camera rotation and cursor lock
// Dependencies: Transform (player body)
// UsesSO: PlayerSettingsSO
// NeedsSetup: playerSettings, playerBody 