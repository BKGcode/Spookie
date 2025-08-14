using System.Collections;
using UnityEngine;
using TMPro;

public class Keypad : MonoBehaviour
{
    public TextMeshPro displayText;
    public string correctCode = "12345";
    public string initialDisplayText = "Need Code";
    private string enteredCode = "";
    private bool canEnterCode = true;
    public Light keypadLight;
    public DoorManager cabinetDoor;

    public AudioSource buttonClickSound;
    public AudioSource acceptedSound;
    public AudioSource deniedSound;

    private Color initialTextColor;
    private Color initialLightColor;

    private void Start()
    {
        initialTextColor = displayText.color;
        initialLightColor = keypadLight.color;

        displayText.text = initialDisplayText;
    }

    public void AddNumber(string number)
    {
        if (!canEnterCode) return;

        buttonClickSound.Play();

        enteredCode += number;
        UpdateDisplay();

        if (enteredCode.Length >= correctCode.Length)
        {
            CheckCode();
        }
    }

    private void UpdateDisplay()
    {
        displayText.text = enteredCode;
    }

    private void CheckCode()
    {
        canEnterCode = false;
        if (enteredCode == correctCode)
        {
            displayText.color = initialTextColor;
            displayText.text = "Accepted";
            cabinetDoor.isKeypadUnlocked = true;

            acceptedSound.Play();
        }
        else
        {
            displayText.color = Color.red;
            displayText.text = "Denied";
            keypadLight.color = Color.red;

            deniedSound.Play();

            StartCoroutine(ResetAfterDelay());
        }
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSecondsRealtime(4);
        ResetKeypad();
    }

    private void ResetKeypad()
    {
        enteredCode = "";
        displayText.color = initialTextColor;
        displayText.text = initialDisplayText;
        keypadLight.color = initialLightColor;
        canEnterCode = true;
    }

    public void ClearCode()
    {
        enteredCode = "";
        UpdateDisplay();
    }
}