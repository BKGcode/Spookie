using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Baterias : MonoBehaviour
{
	public Flashlight flashlight;
    public AudioSource Battery;
    public GameObject Player;

    [Tooltip("Here you can specify how many batteries you want to add to the flashlight.")]
    [SerializeField] public int addbattery = 2;

    [SerializeField] private InspectController inspectController;
    [SerializeField] private string ItemName;
    [Tooltip("For example if you want to add 2 battery for your flashlight and you can have only 6 battery than set this 5.")]
    public int maxbattery;
    [SerializeField] private Animator text = null;
    [SerializeField] private string ErrorText = "ErrorText";
    public float animationLength;
    public bool CanPlay = true;
    public GameObject Canvas;

    public void AddBattery()
    {
        if (flashlight.batteries < maxbattery)
        {
            Battery.Play();
            Player.GetComponentInChildren<Flashlight>().batteries += addbattery;
            //flashlight.batteries += 1;
            Destroy(this.gameObject);
        }
        else if (CanPlay == true)
        {
            Canvas.SetActive(true);
            text.Play(ErrorText, 0, 0.0f);
            StartCoroutine(TextAnimEnd());
            CanPlay = false;
        }
    }
    IEnumerator TextAnimEnd()
    {
        yield return new WaitForSecondsRealtime(animationLength);
        CanPlay = true;
    }
    public void ShowObjectName()
    {
        inspectController.ShowName(ItemName);
    }

    public void HideObjectName()
    {
        inspectController.HideName();
    }
}
