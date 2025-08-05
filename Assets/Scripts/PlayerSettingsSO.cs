using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Spookie/Player Settings")]
public class PlayerSettingsSO : ScriptableObject
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Mouse Look Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;
    [SerializeField] private float crouchMouseSensitivity = 1f;
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 3f;
    
    [Header("Audio Settings")]
    [SerializeField] private float footstepVolume = 0.5f;
    [SerializeField] private float jumpVolume = 0.7f;
    
    // Properties for easy access
    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
    public float CrouchSpeed => crouchSpeed;
    public float JumpHeight => jumpHeight;
    public float Gravity => gravity;
    public float MouseSensitivity => mouseSensitivity;
    public float MaxLookAngle => maxLookAngle;
    public float CrouchMouseSensitivity => crouchMouseSensitivity;
    public float InteractionRange => interactionRange;
    public float FootstepVolume => footstepVolume;
    public float JumpVolume => jumpVolume;
    
    // Validation
    private void OnValidate()
    {
        // Ensure positive values
        walkSpeed = Mathf.Max(0.1f, walkSpeed);
        runSpeed = Mathf.Max(walkSpeed, runSpeed);
        crouchSpeed = Mathf.Max(0.1f, crouchSpeed);
        jumpHeight = Mathf.Max(0f, jumpHeight);
        mouseSensitivity = Mathf.Max(0.1f, mouseSensitivity);
        crouchMouseSensitivity = Mathf.Max(0.1f, crouchMouseSensitivity);
        maxLookAngle = Mathf.Clamp(maxLookAngle, 10f, 90f);
        interactionRange = Mathf.Max(0.5f, interactionRange);
        footstepVolume = Mathf.Clamp01(footstepVolume);
        jumpVolume = Mathf.Clamp01(jumpVolume);
    }
}

// ScriptRole: ScriptableObject for player movement and interaction settings
// UsesSO: None (this is a SO)
// NeedsSetup: Configure values in Inspector 