using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using World;
using Core;
using UnityEngine.AI;

namespace Dwarfs
{
    [RequireComponent(typeof(DwarfData), typeof(DwarfMovement), typeof(NavMeshAgent))]
    public class DwarfBrain : MonoBehaviour
    {
        private DwarfData _dwarfData;
        private DwarfMovement _dwarfMovement;
        private NavMeshAgent _agent;

        private Targetable _currentTargetable;
        private Coroutine _searchCoroutine;
        
        private Queue<PlayerDirective> _directiveQueue = new Queue<PlayerDirective>();
        private float _workTimer;
        private const float WorkTimeToDestroyTarget = 5f; // Time in seconds to "mine" a target.

        private void Awake()
        {
            _dwarfData = GetComponent<DwarfData>();
            _dwarfMovement = GetComponent<DwarfMovement>();
            _agent = GetComponent<NavMeshAgent>();
            Debug.Log($"DwarfBrain initialized for {name}.");
        }

        private void OnEnable()
        {
            _dwarfMovement.OnArrival += HandleArrival;
            TimeManager.OnNightStart += HandleNightStart;
        }

        private void OnDisable()
        {
            _dwarfMovement.OnArrival -= HandleArrival;
            TimeManager.OnNightStart -= HandleNightStart;
        }

        private void Update()
        {
            if (_dwarfData.currentState == DwarfData.DwarfState.Sleeping) return;

            // Priority 1: Process the directive queue if we have one.
            if (_dwarfData.currentDirective != null)
            {
                HandleDirective();
                return;
            }

            // If the current directive is null, but the queue is not, process the next one.
            if (_directiveQueue.Count > 0)
            {
                ProcessNextInQueue();
                return;
            }
            
            // Priority 2: Autonomous behavior
            switch (_dwarfData.currentState)
            {
                case DwarfData.DwarfState.Idle:
                    HandleIdleState();
                    break;
                case DwarfData.DwarfState.Working:
                    HandleWorkingState();
                    break;
                case DwarfData.DwarfState.Living:
                    HandleLivingState();
                    break;
            }
        }
        
        public void AssignDirective(PlayerDirective directive)
        {
            CleanupCurrentTasks(true); // Hard reset for player command

            var path = new NavMeshPath();
            _agent.CalculatePath(directive.Target.transform.position, path);

            if (path.status != NavMeshPathStatus.PathComplete)
            {
                Debug.Log($"Target for {name} is blocked. Attempting to generate tunnel path.");
                GenerateTunnelQueue(directive);
            }
            else
            {
                _directiveQueue.Enqueue(directive);
            }
            
            ProcessNextInQueue();
        }

        private void HandleDirective()
        {
            if (_dwarfData.currentState != DwarfData.DwarfState.Working) return;
            
            if (_dwarfData.WorkTimeBudget > 0)
            {
                _dwarfData.ConsumeWorkTime(Time.deltaTime);
                _workTimer += Time.deltaTime;

                if (_workTimer >= WorkTimeToDestroyTarget)
                {
                    CompleteCurrentDirective();
                }
            }
            else
            {
                Debug.LogWarning($"{name} ran out of work budget on a directive. Aborting queue.");
                AbortDirective();
            }
        }

        private void CompleteCurrentDirective()
        {
            Debug.Log($"{name} completed directive on target {_currentTargetable.name}.");
            Destroy(_currentTargetable.gameObject); // The target is destroyed
            ReleaseCurrentTarget();
            _dwarfData.currentDirective = null;
            // The main loop will call ProcessNextInQueue automatically.
        }

        private void GenerateTunnelQueue(PlayerDirective finalDirective)
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = finalDirective.Target.transform.position;
            
            Vector3 center = (startPos + endPos) / 2f;
            float distance = Vector3.Distance(startPos, endPos);
            Vector3 halfExtents = new Vector3(2f, 5f, distance / 2f); // Box width/height is arbitrary
            Quaternion orientation = Quaternion.LookRotation(endPos - startPos);

            var colliders = Physics.OverlapBox(center, halfExtents, orientation);

            var obstacles = colliders
                .Select(c => c.GetComponent<Targetable>())
                .Where(t => t != null && t != finalDirective.Target && !t.IsOccupied)
                .OrderBy(t => Vector3.Distance(transform.position, t.transform.position))
                .ToList();
            
            Debug.Log($"Found {obstacles.Count} obstacles to tunnel through.");

            foreach (var obstacle in obstacles)
            {
                _directiveQueue.Enqueue(new PlayerDirective(obstacle));
            }
            _directiveQueue.Enqueue(finalDirective);
        }

        private void ProcessNextInQueue()
        {
            if (_dwarfData.currentDirective != null || _directiveQueue.Count == 0) return;

            var nextDirective = _directiveQueue.Dequeue();
            _dwarfData.currentDirective = nextDirective;
            SetCurrentTarget(nextDirective.Target);
            
            _dwarfData.currentState = DwarfData.DwarfState.Walking;
            _dwarfMovement.GoToTarget(_currentTargetable.transform);
            Debug.Log($"{name} is starting next directive in queue: {_currentTargetable.name}");
        }
        
        private void AbortDirective()
        {
            CleanupCurrentTasks(false);
            _dwarfData.currentState = DwarfData.DwarfState.Living;
        }
        
        private void HandleNightStart()
        {
             Debug.Log($"{name} is going to sleep, forgetting all tasks and directives.");
             CleanupCurrentTasks(false);
        }

        private void CleanupCurrentTasks(bool forNewPlayerDirective)
        {
            _dwarfMovement.StopMoving();
            ReleaseCurrentTarget();
            _dwarfData.currentDirective = null;
            _directiveQueue.Clear();

            if (_searchCoroutine != null)
            {
                StopCoroutine(_searchCoroutine);
                _searchCoroutine = null;
            }
            
            // If it's not for a new player directive, the state should change.
            if (!forNewPlayerDirective)
            {
                _dwarfData.currentState = DwarfData.DwarfState.Sleeping;
            }
        }

        private void SetCurrentTarget(Targetable newTarget)
        {
            if (_currentTargetable != null)
            {
                _currentTargetable.OnOccupancyChanged -= HandleTargetOccupancyChanged;
            }

            _currentTargetable = newTarget;

            if (_currentTargetable != null)
            {
                _currentTargetable.OnOccupancyChanged += HandleTargetOccupancyChanged;
            }
        }

        private void HandleTargetOccupancyChanged(bool isOccupied)
        {
            if (isOccupied && _currentTargetable != null && (_dwarfData.currentState == DwarfData.DwarfState.Walking || _dwarfData.currentState == DwarfData.DwarfState.Idle))
            {
                // If our current target gets stolen
                if (_dwarfData.currentDirective != null && _dwarfData.currentDirective.Target == _currentTargetable)
                {
                    Debug.LogWarning($"{name}'s directive target was stolen! Aborting directive queue.");
                    AbortDirective();
                }
                else
                {
                    Debug.LogWarning($"{name}'s autonomous target was stolen! Finding a new job.");
                    _dwarfMovement.StopMoving();
                    ReleaseCurrentTarget();
                    _dwarfData.currentState = DwarfData.DwarfState.Idle;
                }
            }
        }
        
        // --- Unchanged methods from before ---
        
        private void HandleIdleState()
        {
            if (_dwarfData.WorkTimeBudget > 0)
            {
                if (_searchCoroutine == null)
                {
                    _searchCoroutine = StartCoroutine(SearchForTargetsRoutine());
                }
            }
            else
            {
                _dwarfData.currentState = DwarfData.DwarfState.Living;
                Debug.Log($"{name} has no more work budget, switching to Living state.");
            }
        }
        
        private IEnumerator SearchForTargetsRoutine()
        {
            while (_dwarfData.currentState == DwarfData.DwarfState.Idle)
            {
                Debug.Log($"{name} is searching for a target...");
                Transform foundTransform = Targetable.FindClosest(transform.position);

                if (foundTransform != null)
                {
                    SetCurrentTarget(foundTransform.GetComponent<Targetable>());
                    _dwarfData.currentState = DwarfData.DwarfState.Walking;
                    _dwarfMovement.GoToTarget(_currentTargetable.transform);
                    _searchCoroutine = null; 
                    yield break;
                }

                yield return new WaitForSeconds(SearchInterval);
            }
            _searchCoroutine = null;
        }
        
        private void HandleWorkingState()
        {
            if (_dwarfData.WorkTimeBudget > 0)
            {
                _dwarfData.ConsumeWorkTime(Time.deltaTime);
            }
            else
            {
                ReleaseCurrentTarget();
                _dwarfData.currentState = DwarfData.DwarfState.Idle;
                Debug.Log($"{name} finished working and is now Idle.");
            }
        }
        
        private void HandleLivingState()
        {
            if (_dwarfData.LivingTimeBudget > 0)
            {
                _dwarfData.ConsumeLivingTime(Time.deltaTime);
            }
            else
            {
                _dwarfData.currentState = DwarfData.DwarfState.Idle;
                 Debug.Log($"{name} ran out of living budget, going Idle.");
            }
        }
        
        private void HandleArrival()
        {
            if (_currentTargetable == null) return;
            
            if (_dwarfData.currentDirective != null && _dwarfData.currentDirective.Target == _currentTargetable)
            {
                 if (_currentTargetable.IsOccupied)
                 {
                    Debug.LogWarning($"{name} arrived at directive target, but it's occupied. Aborting directive queue.");
                    AbortDirective();
                    return;
                 }
                 _currentTargetable.SetOccupancy(true);
                 _dwarfData.currentState = DwarfData.DwarfState.Working;
                 _workTimer = 0f;
                 Debug.Log($"{name} started working on player directive: {_currentTargetable.name}");
                 return;
            }
            
            if (!_currentTargetable.IsOccupied)
            {
                _currentTargetable.SetOccupancy(true);
                _dwarfData.currentState = DwarfData.DwarfState.Working;
                Debug.Log($"{name} successfully claimed autonomous target {_currentTargetable.name} and starts working.");
            }
            else
            {
                Debug.LogWarning($"{name} arrived at autonomous target, but it's already occupied. Returning to Idle.");
                ReleaseCurrentTarget();
                _dwarfData.currentState = DwarfData.DwarfState.Idle;
            }
        }

        private void ReleaseCurrentTarget()
        {
            if (_currentTargetable != null)
            {
                _currentTargetable.OnOccupancyChanged -= HandleTargetOccupancyChanged;
                
                if (_currentTargetable.IsOccupied)
                {
                    _currentTargetable.SetOccupancy(false);
                }
                
                SetCurrentTarget(null);
            }
        }
    }
} 