using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Character specifications")]
    public float NormalSpeed = 6f;
    public float RunSpeed = 10f;
    public float walkSpeed = 3f;
    public float gravity = -9.18f;
    public float jumpHeight = 3f;
    public float standHeight = 2.0f;
    public float crouchHeight = 1.0f;
    public float RechargeDuration;
    public float maxEnergy = 100f;
    public float energy = 100f;
    public CharacterController controller;
    [Tooltip("Enable this if you want to stand up automatically when releasing the C button")]
    public bool HoldToCrouch;

    [Header("Energy specifications")]
    public Slider energySlider;
    [Tooltip("The energy canvas.")]
    public GameObject Canvas;
    public bool isRecharging = false;

    [Header("Ground specifications")]
    public float groundDistance = 0.4f;
    public Transform groundCheck;
    public LayerMask groundMask;
    bool isGrounded;

    Vector3 velocity;

    [System.Serializable]
    public class SurfaceSound
    {
        public string tag;
        public AudioClip clip;
    }

    [Header("Audio specifications")]
    public List<SurfaceSound> surfaceSounds;
    string surface = "";

    Dictionary<string, AudioClip> surfaceSoundDict = new Dictionary<string, AudioClip>();

    public AudioSource footstepSound;
    public AudioClip sighSound;
    public AudioSource SighAudioSource;

    private bool isWalking = false;
    private bool isCrouching = false;
    private float crouchTime = 0.5f; 
    private float standRadius = 0.5f; 
    private float crouchRadius = 0.25f; 
    private float currentCrouchTime = 0.0f;
    [HideInInspector]
    public float speed = 6f;

    void Start()
    {
        energySlider.maxValue = maxEnergy;
        energySlider.value = energy;
        Canvas.SetActive(false);
    }

    private void Awake()
    {
        foreach (SurfaceSound ss in surfaceSounds)
        {
            surfaceSoundDict[ss.tag] = ss.clip;
        }
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleCrouch();
        }

        if (isCrouching && currentCrouchTime < crouchTime)
        {
            currentCrouchTime += Time.deltaTime;
        }
        else if (!isCrouching && currentCrouchTime > 0)
        {
            currentCrouchTime -= Time.deltaTime;
        }

        float currentRadius = Mathf.Lerp(standRadius, crouchRadius, currentCrouchTime / crouchTime);
        float currentHeight = Mathf.Lerp(standHeight, crouchHeight, currentCrouchTime / crouchTime);

        controller.radius = currentRadius;
        controller.height = currentHeight;

        if (!Input.GetKey(KeyCode.C) && isCrouching && HoldToCrouch)
        {
            ToggleCrouch(); 
        }

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (Input.GetKey("left shift") && isGrounded && (Input.GetAxis("Horizontal") != 0f || Input.GetAxis("Vertical") != 0f) && !isRecharging)
        {
            speed = RunSpeed;
            energy -= 10f * Time.deltaTime; // Decrease energy when running
            energy = Mathf.Clamp(energy, 0f, maxEnergy);
            Canvas.SetActive(true);
        }
        else
        {
            speed = NormalSpeed;
            if (energy < maxEnergy && !isRecharging)
            {
                energy += 10f * Time.deltaTime; // Increase energy when not running
                energy = Mathf.Clamp(energy, 0f, maxEnergy); // Clamp energy within the range of 0 to maxEnergy
            }
        }

        if (energy <= 0f)
        {
            speed = walkSpeed;
            isWalking = true;
            StartCoroutine(RechargeEnergy()); // Start coroutine to recharge energy
        }

        // Check if the "Ctrl" key is pressed and toggle isWalking accordingly
        if (Input.GetKey(KeyCode.LeftControl))
        {
            speed = walkSpeed;
            isWalking = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            speed = NormalSpeed;
            isWalking = false;
        }

        // Use the appropriate speed depending on whether the player is walking or not
        float currentSpeed = isWalking ? walkSpeed : speed;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);


        if (controller.isGrounded && controller.velocity.magnitude > 2f && footstepSound.isPlaying == false)
        {
            if (isWalking)
            {
                footstepSound.volume = 0f; // Mute the footstep sound if walking
            }
            else
            {
                footstepSound.volume = Random.Range(.1f, .5f);
                footstepSound.pitch = Random.Range(.5f, 1.1f);
                if (surfaceSoundDict.TryGetValue(surface, out AudioClip clip))
                {
                    footstepSound.clip = clip;
                }
                footstepSound.Play();
            }
        }

        energySlider.value = energy; // Update the UI slider with the current energy value

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        if(energy == maxEnergy)
        {
            Canvas.SetActive(false);
        }
    }
    
     void ToggleCrouch()
    {
        isCrouching = !isCrouching;

        // Az állapot változásakor alaphelyzetbe állítjuk a guggolási idõt
        currentCrouchTime = isCrouching ? 0 : crouchTime;
    }

    IEnumerator RechargeEnergy()
    {
        if (!SighAudioSource.isPlaying) // check if the audio source is not already playing a sound
        {
            SighAudioSource.clip = sighSound; 
            SighAudioSource.Play(); 
            SighAudioSource.volume = Random.Range(0.5f, 1f);
            SighAudioSource.pitch = Random.Range(0.95f, 1.05f);
        }
        isRecharging = true;
        yield return new WaitForSecondsRealtime(5f); 
        float currentTime = 0.0f;
        float rechargeDuration = RechargeDuration;

        while (currentTime < rechargeDuration && isRecharging == true)
        {
            currentTime += Time.deltaTime;

            float currentEnergy = Mathf.Lerp(0f, maxEnergy, currentTime / rechargeDuration);

            // Energy recharge rate per second
            float rechargeRate = 20f;

            energy = Mathf.Clamp(currentEnergy, 0f, maxEnergy + rechargeRate * Time.deltaTime);

            yield return null;
        }


        if (energy == maxEnergy)
        {
            isRecharging = false;
            isWalking = false;
        }
    }

    public void OnControllerColliderHit(ControllerColliderHit hit)
    {
        surface = hit.gameObject.tag;
    }
    public void DisableTheCanvas()
    {
        Canvas.SetActive(false);
    }
}