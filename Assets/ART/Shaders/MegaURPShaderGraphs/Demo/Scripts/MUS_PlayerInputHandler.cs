using UnityEngine;

public class MUS_PlayerInputHandler : MonoBehaviour
{
    public const string k_AxisNameVertical = "Vertical";
    public const string k_AxisNameHorizontal = "Horizontal";
    public const string k_MouseAxisNameVertical = "Mouse Y";
    public const string k_MouseAxisNameHorizontal = "Mouse X";

    public const string k_ButtonNameJump = "Jump";
    public const string k_ButtonNameAim = "Fire2";

    public const string k_ButtonNameSprint = "left shift";
    public const string k_ButtonNameCrouch = "left ctrl";
   
    public const string k_ButtonNameSubmit = "Submit";
    public const string k_ButtonNameCancel = "Cancel";

    [Tooltip("Sensitivity multiplier for moving the camera around")]
    public float lookSensitivity = 1f;
    [Tooltip("Additional sensitivity multiplier for WebGL")]
    public float webglLookSensitivityMultiplier = 0.25f;
    [Tooltip("Limit to consider an input when using a trigger on a controller")]
    public float triggerAxisThreshold = 0.4f;
    [Tooltip("Used to flip the vertical input axis")]
    public bool invertYAxis = true;
    [Tooltip("Used to flip the horizontal input axis")]
    public bool invertXAxis = true;
    
   
    MUS_CharController m_PlayerCharacterController;
   

    private void Start()
    {
        m_PlayerCharacterController = GetComponent<MUS_CharController>();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
      
    }
   


    public void LockMouseLook()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
       
    }

    public bool CanProcessInput()
    {
       //INSERT TEST
        return true;
        
    }

    float GetLookAxis(string mouseInputName,bool invertAxis)
    {
        if (CanProcessInput())
        {
            // Check if this look input is coming from the mouse
          
            float i =  Input.GetAxisRaw(mouseInputName);

            // handle inverting vertical input
            if (invertAxis)
                i *= -1f;

            // apply sensitivity multiplier
            i *= lookSensitivity;

           
                // reduce mouse input amount to be equivalent to stick movement
                i *= 0.01f;
#if UNITY_WEBGL
                // Mouse tends to be even more sensitive in WebGL due to mouse acceleration, so reduce it even more
                i *= webglLookSensitivityMultiplier;
#endif
            
            return i;
        }

        return 0f;
    }

    public Vector3 GetMoveInput()
    {
        if (CanProcessInput())
        {
            Vector3 move = new Vector3(Input.GetAxisRaw(k_AxisNameHorizontal), 0f, Input.GetAxisRaw(k_AxisNameVertical));

            // constrain move input to a maximum magnitude of 1, otherwise diagonal movement might exceed the max move speed defined
            move = Vector3.ClampMagnitude(move, 1);

            return move;
        }

        return Vector3.zero;
    }

    public float GetLookInputsHorizontal()
    {
        return GetLookAxis(k_MouseAxisNameHorizontal,invertXAxis);
    }

    public float GetLookInputsVertical()
    {
        return GetLookAxis(k_MouseAxisNameVertical,invertYAxis);
    }

    public bool GetJumpInputDown()
    {
        if (CanProcessInput())
        {
            return Input.GetButtonDown(k_ButtonNameJump);
        }

        return false;
    }

    public bool GetJumpInputHeld()
    {
        if (CanProcessInput())
        {
            return Input.GetButton(k_ButtonNameJump);
        }

        return false;
    }

    public bool GetAimInputHeld()
    {
        if (CanProcessInput())
        {

            return Input.GetButton(k_ButtonNameAim);
            
        }

        return false;
    }



    public bool GetSprintInputHeld()
    {
        if (CanProcessInput())
        {
            return Input.GetKey(k_ButtonNameSprint);
        }

        return false;
    }

    public bool GetCrouchInputDown()
    {
        if (CanProcessInput())
        {
            return Input.GetKey(k_ButtonNameCrouch);
        }

        return false;
    }

    public bool GetCrouchInputReleased()
    {
        if (CanProcessInput())
        {
            return Input.GetButtonUp(k_ButtonNameCrouch);
        }

        return false;
    }


}
