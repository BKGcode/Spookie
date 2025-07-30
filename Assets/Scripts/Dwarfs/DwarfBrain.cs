using UnityEngine;
using System.Collections.Generic;
using World;
using Pathfinding;
using Dwarfs.States;
using System.Linq;
using Core;

namespace Dwarfs
{
    [RequireComponent(typeof(DwarfData), typeof(DwarfMovement))]
    public class DwarfBrain : MonoBehaviour
    {
        private DwarfData _dwarfData;
        private DwarfMovement _dwarfMovement;

        public DwarfBaseState CurrentState { get; private set; }
        public IdleState IdleState { get; private set; }
        public WalkingState WalkingState { get; private set; }
        public WorkingState WorkingState { get; private set; }
        public SleepingState SleepingState { get; private set; }
        public LivingState LivingState { get; private set; }
        
        public Targetable CurrentTargetable { get; set; }
        public Bed ReservedBed { get; set; }
        public Queue<PlayerDirective> DirectiveQueue { get; private set; } = new Queue<PlayerDirective>();
        
        [Header("AI Settings")]
        [SerializeField, Tooltip("How many seconds a target remains blacklisted if unreachable.")]
        private float _blacklistCooldown = 10f;
        private Dictionary<Targetable, float> _blacklistedTargets = new Dictionary<Targetable, float>();

        private void Awake()
        {
            _dwarfData = GetComponent<DwarfData>();
            _dwarfMovement = GetComponent<DwarfMovement>();

            IdleState = new IdleState(this, _dwarfData, _dwarfMovement);
            WalkingState = new WalkingState(this, _dwarfData, _dwarfMovement);
            WorkingState = new WorkingState(this, _dwarfData, _dwarfMovement);
            SleepingState = new SleepingState(this, _dwarfData, _dwarfMovement);
            LivingState = new LivingState(this, _dwarfData, _dwarfMovement);
            
            Debug.Log($"DwarfBrain initialized for {name}.");
        }
        
        private void Start()
        {
            TransitionToState(SleepingState);
        }

        private void OnEnable()
        {
            TimeManager.OnNightStart += HandleNightStart; // This might be deprecated for direct time check
            TimeManager.OnDayStart += HandleDayStart;
            Targetable.OnTargetOccupied += HandleTargetOccupied;
        }

        private void OnDisable()
        {
            TimeManager.OnNightStart -= HandleNightStart;
            TimeManager.OnDayStart -= HandleDayStart;
            Targetable.OnTargetOccupied -= HandleTargetOccupied;
        }

        private void Update()
        {
            // The core decision-making loop based on strict priorities.
            var highestPriorityAction = DecideNextAction();
            
            if (CurrentState.GetType() != highestPriorityAction.GetType())
            {
                TransitionToState(highestPriorityAction);
            }
            
            CurrentState?.UpdateState();
        }
        
        private DwarfBaseState DecideNextAction()
        {
            // Priority 1: Biological needs (Sleep)
            if (TimeManager.Instance.IsNight)
            {
                // If we are at the reserved bed, sleep.
                if (ReservedBed != null && IsAdjacentToTarget(ReservedBed.transform.position))
                {
                    return SleepingState;
                }
                
                // If it's night and we don't have a bed, find one.
                if (ReservedBed == null)
                {
                    ReservedBed = Bed.ReserveClosestBed(transform.position);
                }

                // If we found a bed, go to it.
                if (ReservedBed != null)
                {
                    Vector3? destination = FindWalkableAdjacentNode(ReservedBed.transform.position);
                    if (destination.HasValue)
                    {
                        WalkingState.SetDestination(destination.Value, WalkPurpose.GoToBed);
                        return WalkingState;
                    }
                }
                
                // If no beds are available or reachable, just be idle.
                return IdleState;
            }

            // Priority 2: Player Directives
            if (DirectiveQueue.Count > 0 && _dwarfData.WorkTimeBudget > 0)
            {
                var directive = DirectiveQueue.Peek();
                CurrentTargetable = directive.Target;
                return GetStateForCurrentTarget();
            }
            
            // Priority 3: Autonomous Work
            if (_dwarfData.WorkTimeBudget > 0)
            {
                if (CurrentTargetable == null)
                {
                    PruneBlacklist();
                    var currentBlacklist = new HashSet<Targetable>(_blacklistedTargets.Keys);
                    CurrentTargetable = Targetable.FindClosest(transform.position, currentBlacklist);
                }
                
                if (CurrentTargetable != null)
                {
                    return GetStateForCurrentTarget();
                }
            }

            // Priority 4: Leisure Time
            if (_dwarfData.LivingTimeBudget > 0)
            {
                CurrentTargetable = null; // No target when going to leisure
                return LivingState;
            }

            // Priority 5: No budget left for anything, just be idle.
            CurrentTargetable = null;
            return IdleState;
        }

        private DwarfBaseState GetStateForCurrentTarget()
        {
            if (CurrentTargetable == null) return IdleState;

            if (!IsAdjacentToTarget(CurrentTargetable.transform.position))
            {
                Vector3? destination = FindWalkableAdjacentNode(CurrentTargetable.transform.position);
                if (destination.HasValue)
                {
                    WalkingState.SetDestination(destination.Value, WalkPurpose.Work);
                    return WalkingState;
                }
                // If no walkable node, blacklist and go idle to force re-evaluation
                Debug.LogWarning($"{name} could not find a walkable path to {CurrentTargetable.name}. Blacklisting it for {_blacklistCooldown} seconds.");
                _blacklistedTargets[CurrentTargetable] = Time.time + _blacklistCooldown;
                CurrentTargetable = null;
                return IdleState;
            }
            
            // If adjacent, work.
            return WorkingState;
        }

        public void TransitionToState(DwarfBaseState newState)
        {
            // Avoids redundant state transitions
            if (CurrentState?.GetType() == newState.GetType()) return;

            CurrentState?.ExitState();
            CurrentState = newState;
            CurrentState.EnterState();
        }

        public void AssignDirective(PlayerDirective directive)
        {
            DirectiveQueue.Clear();
            DirectiveQueue.Enqueue(directive);
            ClearBlacklist();
            
            // The brain will now pick this up in the next Update cycle.
            Debug.Log($"New directive assigned for {name} to target {directive.Target.name}. Brain will re-evaluate.");
        }

        private void HandleDayStart()
        {
            // Clear directives from previous day.
            DirectiveQueue.Clear();
            ClearBlacklist();
            CurrentTargetable = null;
            ReservedBed = null; // Forget reserved bed
        }
        
        private void HandleNightStart()
        {
            // The DecideNextAction handles the transition, this could be a backup or removed.
            if(CurrentState != SleepingState)
            {
                 TransitionToState(SleepingState);
            }
        }
        
        private void HandleTargetOccupied(Targetable occupiedTarget)
        {
            // If the target we were heading to is now occupied by someone else, re-evaluate.
            if (CurrentState == WalkingState && CurrentTargetable == occupiedTarget)
            {
                Debug.Log($"{name} is aborting path to {occupiedTarget.name} because it was taken. Finding new job.");
                CurrentTargetable = null;
                TransitionToState(IdleState); // Go idle to immediately re-evaluate for a new task.
            }
        }
        
        public void ClearBlacklist()
        {
            _blacklistedTargets.Clear();
        }

        private void PruneBlacklist()
        {
            if (_blacklistedTargets.Count == 0) return;

            var keysToRemove = new List<Targetable>();
            foreach (var pair in _blacklistedTargets)
            {
                if (Time.time > pair.Value)
                {
                    keysToRemove.Add(pair.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _blacklistedTargets.Remove(key);
            }
        }

        private bool IsAdjacentToTarget(Vector3 targetPosition)
        {
            return Vector3.Distance(transform.position, targetPosition) < 1.5f; // Using a threshold for adjacency
        }

        public Vector3? FindWalkableAdjacentNode(Vector3 targetPosition)
        {
            PathNode targetNode = PathfindingGrid.Instance.WorldPointToNode(targetPosition);
            if (targetNode == null) return null;

            List<PathNode> neighbors;

            // If the target is a bed, its "neighbors" are the walkable nodes adjacent to *any* of its tiles.
            if (ReservedBed != null && targetPosition == ReservedBed.transform.position)
            {
                neighbors = new List<PathNode>();
                var bedTile2Pos = new Vector3(targetPosition.x, 0, targetPosition.z + 1);
                PathNode bedNode2 = PathfindingGrid.Instance.WorldPointToNode(bedTile2Pos);

                neighbors.AddRange(PathfindingGrid.Instance.GetNeighbours(targetNode));
                if (bedNode2 != null)
                {
                    neighbors.AddRange(PathfindingGrid.Instance.GetNeighbours(bedNode2));
                }
                // Remove duplicates
                neighbors = neighbors.Distinct().ToList();
            }
            else
            {
                neighbors = PathfindingGrid.Instance.GetNeighbours(targetNode);
            }
            
            PathNode bestNode = null;
            float closestDist = float.MaxValue;

            foreach (var neighbor in neighbors)
            {
                if (neighbor.isWalkable)
                {
                    float dist = Vector3.Distance(transform.position, neighbor.worldPosition);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        bestNode = neighbor;
                    }
                }
            }
            return bestNode?.worldPosition;
        }
    }
} 