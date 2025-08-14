using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keycard : MonoBehaviour
{
    public KeypadCard keypadCard;

    public void EnableKeypadCard()
    {
        keypadCard.canUseKeyCard = true;
    }
    public void DisableKeypadCard()
    {
        keypadCard.canUseKeyCard = false;
    }
}

