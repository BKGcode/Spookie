using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BookCollector : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    private int numSpawnedItems = 0;

    [SerializeField] private int rayLength = 5;
    [SerializeField] private LayerMask LayerMaskInteract;
    [SerializeField] private Image crosshair;
    private bool isCrosshairActive;
    private bool doOnce;
    [SerializeField] KeyCode PickUp = KeyCode.Mouse0;
    private ItemSpawner raycastedObj;

    [TextArea] [SerializeField] private string YourText;

    [Tooltip("Put here your player audiolistener.")]
    public AudioListener audiolistener;
    public GameObject Wincanvas;
    public MouseLook mouseLook;
    public PauseMenu pauseMenu;

    private void Start()
    {
        textMeshPro.enabled = false;
    }

    private void Update()
    {
        RaycastHit hit;
        Vector3 fwd = transform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(transform.position, fwd, out hit, rayLength, LayerMaskInteract.value))
        {
            if (hit.collider.gameObject.CompareTag("Book"))
            {
                if (!doOnce)
                {
                    raycastedObj = hit.collider.gameObject.GetComponent<ItemSpawner>();
                    CrosshairChange(true);
                }

                isCrosshairActive = true;
                doOnce = true;

                if (Input.GetKeyDown(PickUp))
                {
                    Destroy(hit.collider.gameObject);
                    numSpawnedItems++;
                    UpdateTextDisplay();
                    StartCoroutine(Text());
                }
            }
        }
        else
        {
            if (isCrosshairActive)
            {
                CrosshairChange(false);
                doOnce = false;
            }
        }
        // Check if all five books have been collected
        /*if (numSpawnedItems >= raycastedObj.numberOfItems)
        {
            Debug.Log("You have collected all five books!");
        }
        */
    }

    

    void CrosshairChange(bool on)
    {
        if (on && !doOnce)
        {
            crosshair.color = Color.red;
        }
        else
        {
            crosshair.color = Color.white;
            isCrosshairActive = false;
        }
    }
    
    IEnumerator Text()
    {
        textMeshPro.enabled = true;
        yield return new WaitForSecondsRealtime(2f);
        textMeshPro.enabled = false;
    }

    public void UpdateTextDisplay()
    {
        ItemSpawner itemSpawner = FindObjectOfType<ItemSpawner>();
        if (numSpawnedItems >= itemSpawner.numberOfItems)
        {
            Time.timeScale = 0f;
            Destroy(textMeshPro);
            audiolistener.enabled = false;
            Wincanvas.SetActive(true);
            mouseLook.enabled = false;
            pauseMenu.enabled = false;
            Cursor.lockState = CursorLockMode.None;
        }
        else
            textMeshPro.text = YourText + numSpawnedItems;
    }
}
