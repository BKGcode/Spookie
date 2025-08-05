using UnityEngine;
using UnityEngine.InputSystem;

public static class InputSetupHelper
{
    public static void SetupDefaultInputActions()
    {
        Debug.Log("Setting up default input actions...");
        
        // This script helps with Input System setup
        // You need to create an Input Actions Asset in Unity:
        // 1. Right-click in Project > Create > Input Actions
        // 2. Name it "PlayerInput"
        // 3. Add the following actions:
        //    - Move (2D Vector) - WASD/Arrow Keys
        //    - Look (2D Vector) - Mouse Delta
        //    - Jump (Button) - Space
        //    - Run (Button) - Left Shift
        //    - Interact (Button) - E
        //    - ToggleCursor (Button) - Tab
        
        Debug.Log("Input setup instructions logged. Check console for details.");
    }
    
    public static string GetInputSetupInstructions()
    {
        return @"
=== INPUT SYSTEM SETUP INSTRUCTIONS ===

1. Create Input Actions Asset:
   - Right-click in Project > Create > Input Actions
   - Name it 'PlayerInput'

2. Add Action Map 'Player':
   - Click '+' next to Action Maps
   - Name it 'Player'

3. Add Actions:
   - Move (Action Type: Value, Control Type: 2D Vector)
     Binding: WASD or Arrow Keys
   
   - Look (Action Type: Value, Control Type: 2D Vector)
     Binding: Mouse Delta
   
   - Jump (Action Type: Button)
     Binding: Space
   
   - Run (Action Type: Button)
     Binding: Left Shift
   
   - Interact (Action Type: Button)
     Binding: E
   
   - ToggleCursor (Action Type: Button)
     Binding: Tab

4. Generate C# Class:
   - Select the Input Actions Asset
   - In Inspector, check 'Generate C# Class'
   - Click 'Apply'

5. Assign to PlayerInput component:
   - Add PlayerInput component to player GameObject
   - Assign the Input Actions Asset
   - Set Behavior to 'Invoke Unity Events'

6. Connect events in Inspector:
   - Move -> PlayerInput.OnMove
   - Look -> PlayerInput.OnLook
   - Jump -> PlayerInput.OnJump
   - Run -> PlayerInput.OnRun
   - Interact -> PlayerInput.OnInteract
   - ToggleCursor -> PlayerInput.OnToggleCursor
";
    }
}

// ScriptRole: Utility class for Input System setup instructions
// Dependencies: None
// UsesSO: None 