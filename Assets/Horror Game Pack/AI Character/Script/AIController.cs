using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    [System.Serializable]
    public class FootstepSound
    {
        public string tag;
        public AudioClip sound;
    }

    public float viewAngle = 90f;
    public float viewDistance = 10f;
    public float wanderRadius = 10f;
    public float wanderInterval = 5f;
    public AudioSource audioSource;
    public AudioClip actionMusic;
    public AudioClip BehindSpawn;
    public Animator animator;

    private NavMeshAgent navAgent;
    private GameObject player;
    private Vector3 wanderPoint;
    private bool isChasing = false;
    private bool isCaught = false;

    public float hearingRange = 5f;
    public LayerMask audioLayer;
    public float volumeTreshHold;
    public float acceleration = 0.1f;
    public bool isWalking;

    [Tooltip("Turn this on if you want Action music when the AI is chasing you.")]
    public bool PlayTheActionMusic = true;
    public bool useRandomTeleportToPlayer = true;

    public AudioSource footstepAudioSource;
    public List<FootstepSound> footstepSounds = new List<FootstepSound>();

    [Tooltip("Put here what should be turned on when the AI catches you. For example, your jumpscare object.")]
    public List<GameObject> ThingsThatShouldBeTurnOn = new List<GameObject>();
    [Tooltip("Put here what should be turned off when the AI catches you. For example, your main camera.")]
    public List<GameObject> ThingsThatShouldBeTurnOff = new List<GameObject>();
    [Tooltip("The gameobject that contains the script that drops you back to the previous scene when you die.")]
    public GameObject PreviousScene;

    [Space(10)]
    [Header("Teleport Settings")]
    public Vector2 timeUntilNextTeleportRange = new Vector2(10f, 30f);
    public Vector2 spawnNearPlayerDistanceRange = new Vector2(8f, 13f);
    public Vector2 CloseSpawnNearPlayerDistanceRange = new Vector2(4f, 7.5f);
    [Tooltip("The initial teleport delay is the number of seconds after the game starts before the AI can teleport to the player.")]
    public float initialTeleportDelay = 10f;
    [Range(0, 1)]
    public float CloseTeleportChance = 0.2f;

    private Vector3? lastKnownPosition = null;
    private float timeSinceLastSawPlayer = Mathf.Infinity;
    private float timeUntilNextTeleport = 0f;
    private float timeSinceStart = 0f;
    private HidingRaycast hidingRaycast;

    public static bool isPlayerHiding = false;

    private void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Capsule");
        animator = GetComponent<Animator>();
        StartCoroutine(Wander());
    }

    private void Update()
    {
        if (isPlayerHiding)
        {
            isChasing = false;
            lastKnownPosition = null;
            audioSource.Stop();
            return;
        }

        if (isChasing)
        {
            timeSinceLastSawPlayer = 0f;
            navAgent.SetDestination(player.transform.position);
            lastKnownPosition = player.transform.position;
            isWalking = true;

            if (!CanSeePlayer() && !CanHearPlayer())
            {
                isChasing = false;
            }
            else if (!audioSource.isPlaying && PlayTheActionMusic == true)
            {
                audioSource.PlayOneShot(actionMusic);
            }
        }
        else if (CanSeePlayer() || CanHearPlayer())
        {
            isChasing = true;
            isWalking = true;
            lastKnownPosition = player.transform.position;
        }
        else if (lastKnownPosition.HasValue)
        {
            navAgent.SetDestination(lastKnownPosition.Value);
            isWalking = true;

            // If the AI reaches the last known position and still cannot see the player, start wandering again
            if (navAgent.remainingDistance <= navAgent.stoppingDistance && !navAgent.pathPending)
            {
                lastKnownPosition = null;
                StartCoroutine(Wander());
                audioSource.Stop();
            }
        }
        else
        {
            isWalking = navAgent.velocity.magnitude > 0.1f; // Check if the AI is moving
        }

        if (!CanSeePlayer() && !CanHearPlayer())
        {
            timeSinceLastSawPlayer += Time.deltaTime;
            if (timeSinceLastSawPlayer > 2f)
            {
                audioSource.Stop();
            }
        }

        animator.SetFloat("Blend", isWalking ? 1f : 0f);

        if (isCaught)
        {
            navAgent.isStopped = true;
            audioSource.Stop();
            for (int i = 0; i < ThingsThatShouldBeTurnOn.Count; i++)
            {
                ThingsThatShouldBeTurnOn[i].SetActive(true);
            }
            for (int i = 0; i < ThingsThatShouldBeTurnOff.Count; i++)
            {
                ThingsThatShouldBeTurnOff[i].SetActive(false);
            }
        }
        if (navAgent.velocity.magnitude > 2f && footstepAudioSource.isPlaying == false)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit))
            {
                PlayFootstepSound(hit.collider.tag);
            }
        }

        timeUntilNextTeleport -= Time.deltaTime;
        timeSinceStart += Time.deltaTime;

        if (timeUntilNextTeleport <= 0 && useRandomTeleportToPlayer && !isPlayerHiding && !isChasing)
        {
            if (Random.value < CloseTeleportChance && timeSinceStart >= initialTeleportDelay)
            {
                isChasing = true;
                isCaught = false;
                audioSource.PlayOneShot(BehindSpawn);
                SpawnNearPlayer(CloseSpawnNearPlayerDistanceRange.x, CloseSpawnNearPlayerDistanceRange.y);
            }
            else if (timeSinceStart >= initialTeleportDelay)
            {
                SpawnNearPlayer(spawnNearPlayerDistanceRange.x, spawnNearPlayerDistanceRange.y);
            }

            timeUntilNextTeleport = Random.Range(timeUntilNextTeleportRange.x, timeUntilNextTeleportRange.y);
        }
    }

    private void PlayFootstepSound(string surfaceTag)
    {
        foreach (var footstepSound in footstepSounds)
        {
            if (footstepSound.tag == surfaceTag)
            {
                footstepAudioSource.PlayOneShot(footstepSound.sound);
                break;
            }
        }
    }

    private bool CanSeePlayer()
    {
        Vector3 direction = player.transform.position - transform.position;
        float angle = Vector3.Angle(direction, transform.forward);
        if (angle < viewAngle / 2f)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, viewDistance))
            {
                if (hit.collider.gameObject == player)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool CanHearPlayer()
    {
        Collider[] audioSources = Physics.OverlapSphere(transform.position, hearingRange, audioLayer);
        foreach (Collider audioSource in audioSources)
        {
            AudioSource source = audioSource.GetComponent<AudioSource>();
            if (source != null && source.clip != null && !source.isPlaying)
            {
                float distance = Vector3.Distance(transform.position, audioSource.transform.position);
                float volume = source.volume / Mathf.Max(distance, 1f);
                //Debug.Log("Volume: " + volume);
                if (volume >= volumeTreshHold)
                {
                    return true;
                }
            }
        }
        return false;
    }


    private IEnumerator Wander()
    {
        while (true)
        {
            if (!isChasing)
            {
                wanderPoint = RandomNavSphere(transform.position, wanderRadius, -1);
                navAgent.SetDestination(wanderPoint);
            }
            yield return new WaitForSecondsRealtime(wanderInterval);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            isChasing = true;
            isCaught = true;
            PreviousScene.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            isChasing = false;
        }
    }

    private Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }

    private Vector3 GetRandomPointBehindPlayer(float minDistance, float maxDistance)
    {
        Vector3 playerDirection = -player.transform.forward;
        float distance = Random.Range(minDistance, maxDistance);

        Vector3 spawnPoint = player.transform.position + playerDirection * distance;

        return spawnPoint;
    }

    private void SpawnNearPlayer(float minDistance, float maxDistance)
    {
        Vector3 spawnPosition = GetRandomPointBehindPlayer(minDistance, maxDistance);

        transform.position = spawnPosition;

        navAgent.isStopped = false;
        lastKnownPosition = null;

        StartCoroutine(Wander());
    }
}