using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InspectController : MonoBehaviour
{
    [SerializeField] private GameObject objectNameBG;
    [SerializeField] private TextMeshProUGUI objectNameUI;

    [SerializeField] private GameObject InteractText;
    [SerializeField]
    [Tooltip("Enter the text for interaction here.")]
    [TextArea] private string interactTextContent;
    [SerializeField] private float OnScreenTimer;
    [SerializeField] private TextMeshProUGUI extraInfoUI;
    [SerializeField] private GameObject extraInfoBG;
    [SerializeField] private GameObject examineDescription;
    [HideInInspector] public bool startTimer;
    private float timer;

    [SerializeField] private KeyCode descriptionKey = KeyCode.Q;

    private bool isExamining = false;
    private string objectDescription = "";

    [SerializeField] private TextMeshProUGUI extraInfoExamine;

    private ObjectExaminer objectExaminer;

    private void Start()
    {
        objectNameBG.SetActive(false);
        extraInfoBG.SetActive(false);
        InteractText.SetActive(false);
    }

    private void Update()
    {
        if (startTimer)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                timer = 0;
                ClearAdditionalInfo();
                startTimer = false;
            }
        }
        if (isExamining && Input.GetKeyDown(descriptionKey))
        {
            ShowDescription(objectDescription);
        }
    }

    public void ShowName(string objectName)
    {
        objectNameBG.SetActive(true);
        objectNameUI.text = objectName;
        InteractText.SetActive(true);
        InteractText.GetComponent<TextMeshProUGUI>().text = interactTextContent;
    }

    public void HideName()
    {
        objectNameBG.SetActive(false);
        objectNameUI.text = "";
        InteractText.SetActive(false);
    }

    public void ShowAdditionalInfo(string newInfo)
    {
        timer = OnScreenTimer;
        startTimer = true;
        extraInfoBG.SetActive(true);
        extraInfoUI.text = newInfo;
    }

    public void ShowAdditionalInfoExamine(string newInfo)
    {
        examineDescription.SetActive(true);
        extraInfoExamine.enabled = true;
        extraInfoExamine.text = newInfo;
    }

    public void DontShowAdditionalInfoExamine()
    {
        examineDescription.SetActive(false);
        extraInfoExamine.enabled = false;
    }

    void ClearAdditionalInfo()
    {
        extraInfoBG.SetActive(false);
        extraInfoUI.text = "";
    }

    void ShowDescription(string description)
    {
        ShowAdditionalInfo(description);
    }
}
