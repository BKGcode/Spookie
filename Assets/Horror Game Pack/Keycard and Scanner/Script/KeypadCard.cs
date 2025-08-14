using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeypadCard : MonoBehaviour
{
    public GameObject keyCard;
    public GameObject PlayerHand;
    public DoorManager cabinetDoor;
    public AudioSource keycardAccepted;
    public Light keypadLight;
    public InventoryController inventoryController;
    public bool canUseKeyCard = false;
    public float animLenght = 1f;

    public void KeyCardClick()
    {
        if (canUseKeyCard)
        {
            inventoryController.iskeypadinuse = true;
            keyCard.SetActive(true);
            Animator animator = GetComponent<Animator>();
            animator.Play("KeyCardAnim");
            canUseKeyCard = false;
            MeshRenderer meshRenderer = PlayerHand.GetComponentInChildren<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }
            StartCoroutine(DeleteCard());
        }
    }

    private IEnumerator DeleteCard()
    {
        yield return new WaitForSecondsRealtime(animLenght);

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = renderer.material;
            if (mat != null && mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", new Color(0, 170 / 255f, 0));
                mat.EnableKeyword("_EMISSION");
            }
        }

        if (keypadLight != null)
        {
            keypadLight.color = new Color(0, 1, 0); 
        }
        cabinetDoor.isKeyCardUnlocked = true;
        Destroy(keyCard);
        MeshRenderer meshRenderer = PlayerHand.GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
        }
        inventoryController.iskeypadinuse = false;
        keycardAccepted.Play();
    }
}
