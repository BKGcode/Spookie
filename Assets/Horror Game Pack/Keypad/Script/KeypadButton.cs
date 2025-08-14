using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeypadButton : MonoBehaviour
{
    public string KeypadNumber;
    public Keypad keypad;

    public void OnNumberClick()
    {
        if (keypad != null)
        {
            keypad.AddNumber(KeypadNumber);
        }
    }
}
