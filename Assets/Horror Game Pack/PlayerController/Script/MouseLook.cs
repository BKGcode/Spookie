using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Transform playerTransform;
    public float bobbingSpeed = 0.1f; // How fast the head should bob
    public float bobbingAmount = 0.05f; // How far the head should bob
    public float midpointY = 0.5f; // The Y coordinate where the head is at its midpoint

    private float timer = 0f; // A timer for the head bobbing
    private float waveSlice = 0f; // The current position on the head bobbing wave

    private float xRotation = 0f;
    public bool canMove = true; 
    private bool resetRotationRequested = false; 

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -50f, 80f);

            // Update the rotation of the camera
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerTransform.Rotate(Vector3.up, mouseX);

            // Update the head bobbing
            float waveslice = 0.0f;
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            if (Mathf.Abs(horizontal) == 0f && Mathf.Abs(vertical) == 0f)
            {
                timer = 0.0f;
            }
            else
            {
                waveslice = Mathf.Sin(timer);
                timer += bobbingSpeed;

                if (timer > Mathf.PI * 2)
                {
                    timer = timer - (Mathf.PI * 2);
                }
            }

            if (waveslice != 0)
            {
                float translateChange = waveslice * bobbingAmount;
                float totalAxes = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
                totalAxes = Mathf.Clamp(totalAxes, 0.0f, 1.0f);
                translateChange = totalAxes * translateChange;
                transform.localPosition = new Vector3(transform.localPosition.x, midpointY + translateChange, transform.localPosition.z);
            }
            else
            {
                transform.localPosition = new Vector3(transform.localPosition.x, midpointY, transform.localPosition.z);
            }
        }


        if (resetRotationRequested)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            xRotation = 0f;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -50f, 80f);

            // Update the rotation of the camera
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerTransform.Rotate(Vector3.up, mouseX);
            resetRotationRequested = false; 
        }
    }

    public void SetCanMove(bool move)
    {
        canMove = move;
    }

    public void ResetRotation()
    {
        resetRotationRequested = true; 
    }
}
