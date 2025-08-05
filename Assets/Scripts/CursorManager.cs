using UnityEngine;
using UnityEngine.InputSystem;

public class CursorManager : MonoBehaviour
{
    [Header("Cursor Settings")]
    [SerializeField] private bool startWithLockedCursor = true;
    [SerializeField] private CursorLockMode defaultLockMode = CursorLockMode.Locked;
    
    [Header("UI References")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameUI;
    
    [Header("Auto Setup")]
    [SerializeField] private bool autoFindUIElements = true;
    
    private bool isCursorLocked = true;
    private bool wasInitialized = false;
    
    private void Start()
    {
        if (autoFindUIElements)
        {
            AutoFindUIElements();
        }
        
        if (startWithLockedCursor)
        {
            LockCursor();
        }
        else
        {
            UnlockCursor();
        }
        
        wasInitialized = true;
        Debug.Log("CursorManager initialized");
    }
    
    private void AutoFindUIElements()
    {
        // Try to find pause menu
        if (pauseMenu == null)
        {
            pauseMenu = GameObject.Find("PauseMenu") ?? GameObject.Find("Pause Panel") ?? GameObject.Find("Menu");
        }
        
        // Try to find game UI
        if (gameUI == null)
        {
            gameUI = GameObject.Find("GameUI") ?? GameObject.Find("HUD") ?? GameObject.Find("UI");
        }
        
        if (pauseMenu != null)
        {
            Debug.Log($"Found pause menu: {pauseMenu.name}");
        }
        
        if (gameUI != null)
        {
            Debug.Log($"Found game UI: {gameUI.name}");
        }
    }
    
    public void LockCursor()
    {
        Cursor.lockState = defaultLockMode;
        Cursor.visible = false;
        isCursorLocked = true;
        
        if (gameUI != null)
            gameUI.SetActive(true);
            
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
            
        if (wasInitialized)
        {
            Debug.Log("Cursor locked");
        }
    }
    
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isCursorLocked = false;
        
        if (gameUI != null)
            gameUI.SetActive(false);
            
        if (pauseMenu != null)
            pauseMenu.SetActive(true);
            
        if (wasInitialized)
        {
            Debug.Log("Cursor unlocked");
        }
    }
    
    public void ToggleCursor()
    {
        if (isCursorLocked)
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }
    }
    
    public bool IsCursorLocked => isCursorLocked;
    
    // Input System callback
    public void OnToggleCursor(InputValue value)
    {
        if (value.isPressed)
        {
            ToggleCursor();
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && startWithLockedCursor && wasInitialized)
        {
            LockCursor();
        }
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && isCursorLocked && wasInitialized)
        {
            UnlockCursor();
        }
    }
}

// ScriptRole: Manages cursor state and related UI visibility
// Dependencies: GameObject (UI elements)
// UsesSO: None
// NeedsSetup: pauseMenu, gameUI (auto-found if available) 