using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DoorManager : MonoBehaviour
{
    public enum DoorType
    {
        Regular,
        KeyDoor,
        KeypadDoor,
        KeyCardDoor,
        Drawer
    }

    public DoorType doorType;
    public AudioSource CabinetClose;
    public AudioSource CabinetOpen;
    public bool doorOpened = false;

    [HideInInspector]
    public bool CabinetOpened = true;

    [SerializeField] public float targetAngleX = 0f;
    [SerializeField] public float targetAngleY = 0f;
    [SerializeField] public float targetAngleZ = 0f;

    [SerializeField] public Vector3 targetPosition;

    [SerializeField] public float openSpeed = 2f;
    [SerializeField] public float closeSpeed = 2f;

    private Quaternion initialRotation;
    private Vector3 initialPosition;
    [HideInInspector]
    public Quaternion openRotation;
    public List<NavMeshObstacle> navMeshObstacle = new List<NavMeshObstacle>();
    private bool isMoving = false;

    [SerializeField] public InspectController inspectController;
    [SerializeField] public string ItemName;

    private bool isInitiallyOpened;

    [HideInInspector] public bool isLocked = true;
    [HideInInspector] public bool hasKey = false;
    [HideInInspector] public bool hasKeyCard = false;
    [HideInInspector] public string keyTag = "DefaultKey";
    [HideInInspector] public AudioSource LockedDoor;
    [HideInInspector] public AudioSource LockDoorOpen;

    [HideInInspector] public GameObject ErrorCanvas;
    [HideInInspector] public Animator text = null;
    [HideInInspector] public string ErrorTextAnimationName = "ErrorText";
    [HideInInspector] private bool CanPlay = true;
    [HideInInspector] public bool isKeypadUnlocked = false;
    [HideInInspector] public bool isKeyCardUnlocked = false;

    private bool isReadyToOpen = false;

    private void Start()
    {
        initialRotation = transform.localRotation;
        initialPosition = transform.localPosition;
        SetOpenRotation();
        isInitiallyOpened = doorOpened;
    }

    private void SetOpenRotation()
    {
        openRotation = Quaternion.Euler(targetAngleX, targetAngleY, targetAngleZ);
    }

    public void ToggleCabinet()
    {
        if (isMoving) return;

        if (doorType == DoorType.KeyDoor && isLocked)
        {
            if (!hasKey)
            {
                Debug.Log("Door is locked. You need the key to open it.");
                LockedDoor.Play();
                if(CanPlay)
                {
                    ErrorCanvas.SetActive(true);
                    text.Play(ErrorTextAnimationName, 0, 0.0f);
                    StartCoroutine(TextAnimEnd());
                    CanPlay = false;
                }
                return;
            }
            else if (!isReadyToOpen)
            {
                StartCoroutine(UnlockDoor());
                return;
            }
        }

        if(doorType == DoorType.KeyCardDoor && !isKeyCardUnlocked)
        {
            Debug.Log("Door is locked.");
            return;
        }

        if (doorType == DoorType.KeypadDoor && !isKeypadUnlocked)
        {
            Debug.Log("Door is locked. Enter the correct code to unlock.");
            return;
        }

        if (doorType == DoorType.Drawer)
        {
            if (CabinetOpened)
            {
                StartCoroutine(MoveDrawer(targetPosition, openSpeed));
                CabinetOpen.Play();
            }
            else
            {
                StartCoroutine(MoveDrawer(initialPosition, closeSpeed));
                CabinetClose.Play();
            }
        }
        else
        {
            if (CabinetOpened)
            {
                StartCoroutine(RotateDoor(openRotation, openSpeed));
                if (!doorOpened)
                {
                    CabinetOpen.Play();
                }
                else if (doorOpened)
                {
                    CabinetClose.Play();
                }
            }
            else
            {
                StartCoroutine(RotateDoor(initialRotation, closeSpeed));

                if (!doorOpened)
                {
                    CabinetOpen.Play();
                }
                else if (doorOpened)
                {
                    CabinetClose.Play();
                }
            }

            foreach (var obstacle in navMeshObstacle)
            {
                if(obstacle.enabled == false)
                {
                    obstacle.enabled = true;
                }
                else
                {
                    obstacle.enabled = false;
                }
            }
        }
        CabinetOpened = !CabinetOpened;
        doorOpened = !doorOpened;
        isReadyToOpen = false;
    }

    IEnumerator TextAnimEnd()
    {
        AnimatorStateInfo animationState = text.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(animationState.length);
        CanPlay = true;
        ErrorCanvas.SetActive(false);
    }

    private IEnumerator UnlockDoor()
    {
        LockDoorOpen.Play();
        yield return new WaitForSeconds(LockDoorOpen.clip.length);
        isLocked = false;
        isReadyToOpen = true;
    }

    public IEnumerator RotateDoor(Quaternion targetRotation, float speed)
    {
        isMoving = true;
        Quaternion startRotation = transform.localRotation;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * speed;

            float fractionOfJourney = Mathf.Clamp01(elapsedTime);

            transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, fractionOfJourney);

            yield return null;
        }

        transform.localRotation = targetRotation;
        isMoving = false;
    }

    public IEnumerator MoveDrawer(Vector3 targetPosition, float speed)
    {
        isMoving = true;
        Vector3 startPosition = transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * speed;

            float fractionOfJourney = Mathf.Clamp01(elapsedTime);

            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

            yield return null;
        }

        transform.localPosition = targetPosition;
        isMoving = false;
    }

    public void ShowObjectName()
    {
        if (!isMoving)
        {
            inspectController.ShowName(ItemName);
        }
    }

    public void HideObjectName()
    {      
       inspectController.HideName();
    }
}
