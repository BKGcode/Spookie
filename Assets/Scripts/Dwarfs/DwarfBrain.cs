using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using World;
using Core;
using Pathfinding;

namespace Dwarfs
{
    [RequireComponent(typeof(DwarfData), typeof(DwarfMovement))]
    public class DwarfBrain : MonoBehaviour
    {
        // Component Dependencies
        private DwarfData _dwarfData;
        private DwarfMovement _dwarfMovement;
        
        // State
        private Targetable _currentTargetable;
        private Coroutine _searchCoroutine;
        private Queue<PlayerDirective> _directiveQueue = new Queue<PlayerDirective>();
        private HashSet<Targetable> _blacklistedTargets = new HashSet<Targetable>();

        // Timers and Constants
        private float _workTimer;
        private const float WorkTimeToDestroyTarget = 5f;
        private const float NoJobSearchInterval = 5f;
        private const float FailedPathPenalty = 2f;

        #region Unity Lifecycle
        private void Awake()
        {
            _dwarfData = GetComponent<DwarfData>();
            _dwarfMovement = GetComponent<DwarfMovement>();
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

            // Priority 1: Player commands (either active or in queue)
            if (_dwarfData.currentDirective != null)
            {
                HandleDirectiveState();
                return;
            }

            if (_directiveQueue.Count > 0)
            {
                ProcessNextInQueue();
                return;
            }
            
            // Priority 2: Autonomous states
            switch (_dwarfData.currentState)
            {
                case DwarfData.DwarfState.Idle:
                    HandleIdleState();
                    break;
                case DwarfData.DwarfState.Working:
                    HandleAutonomousWorkState();
                    break;
                case DwarfData.DwarfState.Living:
                    HandleLivingState();
                    break;
            }
        }
        #endregion

        #region Public API
        public void AssignDirective(PlayerDirective directive)
        {
            CleanupCurrentTasks(true);

            var path = Pathfinder.FindPath(transform.position, directive.Target.transform.position);

            if (path == null)
            {
                Debug.Log($"Target for {name} is blocked. Generating tunnel path.");
                GenerateTunnelQueue(directive);
            }
            else
            {
                _directiveQueue.Enqueue(directive);
            }
            
            ProcessNextInQueue();
        }
        #endregion

        #region State Handlers
        private void HandleDirectiveState()
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
                Debug.LogWarning($"{name} ran out of work budget. Aborting directive queue.");
                AbortDirective();
            }
        }

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
            }
        }

        private void HandleAutonomousWorkState()
        {
            if (_dwarfData.WorkTimeBudget > 0)
            {
                _dwarfData.ConsumeWorkTime(Time.deltaTime);
            }
            else
            {
                ReleaseCurrentTarget();
                _dwarfData.currentState = DwarfData.DwarfState.Idle;
            }
        }

        private void HandleLivingState()
        {
            if (_dwarfData.LivingTimeBudget > 0)
            {
                 _dwarfData.ConsumeLivingTime(Time.deltaTime);
                 if(_searchCoroutine == null) 
                 {
                    _searchCoroutine = StartCoroutine(SearchForTargetsRoutine());
                 }
            }
            else // Should not happen often, but as a fallback
            {
                 _dwarfData.currentState = DwarfData.DwarfState.Idle;
            }
        }

        private void HandleArrival()
        {
            // Pathfinding failed completely
            if (_currentTargetable == null)
            {
                 Debug.LogWarning($"{name} failed to find a path to the designated target.");
                 _dwarfData.ConsumeWorkTime(FailedPathPenalty);
                 if(_dwarfData.currentDirective != null)
                 {
                    _blacklistedTargets.Add(_dwarfData.currentDirective.Target);
                    AbortDirective();
                 }
                 _dwarfData.currentState = DwarfData.DwarfState.Idle;
                 return;
            }
            
            // Arrived for a player directive
            if (_dwarfData.currentDirective != null && _dwarfData.currentDirective.Target == _currentTargetable)
            {
                 if (_currentTargetable.IsOccupied)
                 {
                    Debug.LogWarning($"{name} arrived at directive target, but it's occupied. Aborting.");
                    AbortDirective();
                 }
                 else
                 {
                    _currentTargetable.SetOccupancy(true);
                    _dwarfData.currentState = DwarfData.DwarfState.Working;
                    _workTimer = 0f;
                    Debug.Log($"{name} started working on player directive: {_currentTargetable.name}");
                 }
                 return;
            }
            
            // Arrived for an autonomous task
            if (!_currentTargetable.IsOccupied)
            {
                _currentTargetable.SetOccupancy(true);
                _dwarfData.currentState = DwarfData.DwarfState.Working;
                Debug.Log($"{name} claimed autonomous target {_currentTargetable.name}.");
            }
            else
            {
                Debug.LogWarning($"{name} arrived at autonomous target, but it's occupied. Finding new job.");
                ReleaseCurrentTarget();
                _dwarfData.currentState = DwarfData.DwarfState.Idle;
            }
        }

        private void HandleNightStart()
        {
             Debug.Log($"{name} is going to sleep, forgetting all tasks.");
             _blacklistedTargets.Clear();
             CleanupCurrentTasks(false);
        }
        #endregion

        #region Task & Queue Management
        private void ProcessNextInQueue()
        {
            if (_dwarfData.currentDirective != null || _directiveQueue.Count == 0) return;

            _dwarfData.currentDirective = _directiveQueue.Dequeue();
            SetCurrentTarget(_dwarfData.currentDirective.Target);
            
            _dwarfData.currentState = DwarfData.DwarfState.Walking;
            _dwarfMovement.GoToTarget(_currentTargetable.transform);
            Debug.Log($"{name} starting next directive in queue: {_currentTargetable.name}");
        }

        private void GenerateTunnelQueue(PlayerDirective finalDirective)
        {
            PathNode startNode = PathfindingGrid.Instance.WorldPointToNode(transform.position);
            PathNode endNode = PathfindingGrid.Instance.WorldPointToNode(finalDirective.Target.transform.position);
            
            List<PathNode> line = GetLineOfSight(startNode, endNode);
            var obstacles = line
                .Where(node => !node.isWalkable)
                .Select(node => Physics.OverlapSphere(node.worldPosition, 0.4f)
                                       .Select(c => c.GetComponent<Targetable>())
                                       .FirstOrDefault(t => t != null))
                .Where(t => t != null && t != finalDirective.Target)
                .Distinct()
                .ToList();

            Debug.Log($"Found {obstacles.Count} obstacles to tunnel.");

            obstacles.ForEach(obs => _directiveQueue.Enqueue(new PlayerDirective(obs)));
            _directiveQueue.Enqueue(finalDirective);
        }
        
        private void CompleteCurrentDirective()
        {
            Debug.Log($"{name} completed work on {_currentTargetable.name}.");
            PathfindingGrid.Instance.UpdateNodeWalkability(_currentTargetable.transform.position, true);
            Destroy(_currentTargetable.gameObject);
            ReleaseCurrentTarget();
            _dwarfData.currentDirective = null;
        }

        private void AbortDirective()
        {
            CleanupCurrentTasks(false);
            _dwarfData.currentState = DwarfData.DwarfState.Living;
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
            
            if (!forNewPlayerDirective)
            {
                _dwarfData.currentState = DwarfData.DwarfState.Sleeping;
            }
        }
        #endregion

        #region Target Management
        private IEnumerator SearchForTargetsRoutine()
        {
            while (true) // The loop is controlled by the coroutine being stopped.
            {
                if (_dwarfData.currentState == DwarfData.DwarfState.Idle || _dwarfData.currentState == DwarfData.DwarfState.Living)
                {
                    Debug.Log($"{name} is searching for a target...");
                    Transform foundTransform = Targetable.FindClosest(transform.position, _blacklistedTargets);

                    if (foundTransform != null)
                    {
                        _dwarfData.currentState = DwarfData.DwarfState.Idle;
                        SetCurrentTarget(foundTransform.GetComponent<Targetable>());
                        _dwarfMovement.GoToTarget(_currentTargetable.transform);
                        _searchCoroutine = null; 
                        yield break;
                    }
                    else
                    {
                        Debug.Log($"{name} found no available jobs. Will check again later.");
                        _dwarfData.currentState = DwarfData.DwarfState.Living;
                        yield return new WaitForSeconds(NoJobSearchInterval);
                    }
                }
                else
                {
                     yield return null; // Wait a frame if in another state
                }
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

        private void ReleaseCurrentTarget()
        {
            if (_currentTargetable != null)
            {
                if (_currentTargetable.IsOccupied)
                {
                    _currentTargetable.SetOccupancy(false);
                }
                SetCurrentTarget(null);
            }
        }

        private void HandleTargetOccupancyChanged(bool isOccupied)
        {
            if (!isOccupied || _currentTargetable == null) return;
            
            if (_dwarfData.currentState == DwarfData.DwarfState.Walking)
            {
                Debug.LogWarning($"{name}'s target at {_currentTargetable.name} was stolen! Finding new job.");
                if (_dwarfData.currentDirective != null) AbortDirective();
                else
                {
                    _dwarfMovement.StopMoving();
                    ReleaseCurrentTarget();
                    _dwarfData.currentState = DwarfData.DwarfState.Idle;
                }
            }
        }
        #endregion

        #region Utility
        private List<PathNode> GetLineOfSight(PathNode start, PathNode end)
        {
            List<PathNode> line = new List<PathNode>();
            int x = start.gridX, y = start.gridY, z = start.gridZ;
            int x2 = end.gridX, y2 = end.gridY, z2 = end.gridZ;
            
            int w = x2 - x, h = y2 - y, d = z2 - z;
            int dx1 = 0, dy1 = 0, dz1 = 0, dx2 = 0, dy2 = 0, dz2 = 0;
            if (w<0) dx1 = -1; else if (w>0) dx1 = 1;
            if (h<0) dy1 = -1; else if (h>0) dy1 = 1;
            if (d<0) dz1 = -1; else if (d>0) dz1 = 1;
            if (w<0) dx2 = -1; else if (w>0) dx2 = 1;
            int longest = Mathf.Abs(w), shortest = Mathf.Abs(h);
            if (longest < Mathf.Abs(d))
            {
                longest = Mathf.Abs(d);
                shortest = Mathf.Abs(w);
                if (shortest < Mathf.Abs(h)) shortest = Mathf.Abs(h);
                dy2 = 0;
            }
            else if (shortest < Mathf.Abs(d))
            {
                shortest = Mathf.Abs(d);
            }

            int numerator = longest >> 1;
            for (int i=0; i<=longest; i++)
            {
                line.Add(PathfindingGrid.Instance.WorldPointToNode(new Vector3(x, y, z)));
                numerator += shortest;
                if (numerator >= longest)
                {
                    numerator -= longest;
                    x += dx1; y += dy1; z += dz1;
                } else {
                    x += dx2; y += dy2; z += dz2;
                }
            }
            return line;
        }
        #endregion
    }
} 