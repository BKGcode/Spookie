using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using Dwarfs.States;
using Core;
using Core.Shared;
using World;

namespace Dwarfs
{
    [RequireComponent(typeof(DwarfData), typeof(DwarfMovement))]
    public class DwarfBrain : MonoBehaviour
    {
        private DwarfData _dwarfData;
        private DwarfMovement _dwarfMovement;
        private DwarfPriorities _priorities;

        // Estado actual y estados posibles
        private DwarfBaseState _currentState;
        public DwarfBaseState CurrentState => _currentState;

        private IdleState _idleState;
        private WalkingState _walkingState;
        private WorkingState _workingState;
        private SleepingState _sleepingState;
        private LivingState _livingState;

        // Propiedades de estado interno protegidas
        internal IdleState IdleState => _idleState;
        internal WalkingState WalkingState => _walkingState;
        internal WorkingState WorkingState => _workingState;
        internal SleepingState SleepingState => _sleepingState;
        internal LivingState LivingState => _livingState;

        // Objetivos y directivas
        private World.Targetable _currentTargetable;
        private World.Bed _targetBed;
        public Queue<PlayerDirective> DirectiveQueue => _directiveQueue;
        private readonly Queue<PlayerDirective> _directiveQueue = new Queue<PlayerDirective>();

        public World.Targetable CurrentTargetable 
        { 
            get => _currentTargetable;
            protected internal set
            {
                if (_currentTargetable != value)
                {
                    var oldTarget = _currentTargetable;
                    _currentTargetable = value;
                    if (oldTarget != null && oldTarget.IsOccupiedBy(gameObject))
                    {
                        oldTarget.SetOccupancy(false);
                    }
                }
            }
        }

        public World.Bed TargetBed
        {
            get => _targetBed;
            protected internal set
            {
                if (_targetBed != value)
                {
                    var oldBed = _targetBed;
                    _targetBed = value;
                    if (oldBed != null && oldBed.OccupiedBy == gameObject)
                    {
                        oldBed.Vacate();
                    }
                }
            }
        }

        // Configuración de IA
        [Header("AI Settings")]
        [SerializeField, Tooltip("How many seconds a target remains blacklisted if unreachable.")]
        [Range(5f, 30f)]
        private float _blacklistCooldown = 10f;

        [SerializeField, Tooltip("How many times a dwarf can fail to find a path before blacklisting.")]
        [Range(1, 5)]
        private int _maxPathAttempts = 3;

        private DwarfBlacklist _blacklist;
        private bool _isNight;
        public bool IsSleepWindow => _isSleepWindow;
        private bool _isSleepWindow;
        private bool _isWakeUpTime;
        private bool _isInitialized;

        // Propiedades de estado para DwarfPriorities
        internal bool IsNight => _isNight;

        internal bool IsWakeUpTime => _isWakeUpTime;
        internal bool HasPendingDirectives => _directiveQueue.Count > 0;

        private void Awake()
        {
            _dwarfData = GetComponent<DwarfData>();
            _dwarfMovement = GetComponent<DwarfMovement>();

            InitializeStates();
            _priorities = new DwarfPriorities(this, _dwarfData);
            _blacklist = new DwarfBlacklist(_blacklistCooldown, _maxPathAttempts, name);
        }

        private void InitializeStates()
        {
            try
            {
                if (_dwarfData == null || _dwarfMovement == null)
                {
                    throw new System.InvalidOperationException("Required components not found!");
                }

                _idleState = new IdleState(this, _dwarfData, _dwarfMovement);
                _walkingState = new WalkingState(this, _dwarfData, _dwarfMovement);
                _workingState = new WorkingState(this, _dwarfData, _dwarfMovement);
                _sleepingState = new SleepingState(this, _dwarfData, _dwarfMovement);
                _livingState = new LivingState(this, _dwarfData, _dwarfMovement);

                Debug.Log($"States initialized for {name}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize states for {name}: {ex.Message}");
                enabled = false;
                throw;
            }
        }

        private void OnEnable()
        {
            try
            {
                SubscribeToEvents();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to subscribe to events for {name}: {ex.Message}");
                enabled = false;
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            World.Targetable.OnTargetOccupied += HandleTargetOccupied;
            World.Targetable.OnTargetFreed += HandleTargetFreed;
            World.Targetable.OnTargetTransferred += HandleTargetTransferred;
            TimeManager.OnDayStart += HandleDayStart;
            TimeManager.OnNightStart += HandleNightStart;
            TimeManager.OnSleepWindowStart += HandleSleepWindowStart;
            TimeManager.OnWakeUpTime += HandleWakeUpTime;
        }

        private void UnsubscribeFromEvents()
        {
            World.Targetable.OnTargetOccupied -= HandleTargetOccupied;
            World.Targetable.OnTargetFreed -= HandleTargetFreed;
            World.Targetable.OnTargetTransferred -= HandleTargetTransferred;
            TimeManager.OnDayStart -= HandleDayStart;
            TimeManager.OnNightStart -= HandleNightStart;
            TimeManager.OnSleepWindowStart -= HandleSleepWindowStart;
            TimeManager.OnWakeUpTime -= HandleWakeUpTime;
        }
        
        private void Start()
        {
            try
            {
                ValidateComponents();

                // Si TimeManager ya está corriendo y es de día, inicializar manualmente
                if (TimeManager.Instance != null)
                {
                    if (!TimeManager.Instance.IsNight)
                    {
                        HandleDayStart();
                    }
                    TransitionToState(_idleState);
                }
                else
                {
                    throw new System.InvalidOperationException("TimeManager not found in scene!");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize {name}: {ex.Message}");
                enabled = false;
            }
        }

        private void ValidateComponents()
        {
            if (_dwarfData == null)
                throw new System.InvalidOperationException("DwarfData component not found!");
            if (_dwarfMovement == null)
                throw new System.InvalidOperationException("DwarfMovement component not found!");
            if (_blacklist == null)
                throw new System.InvalidOperationException("DwarfBlacklist not initialized!");
        }

        private void OnDestroy()
        {
            Debug.Log($"{name} is being destroyed. Releasing occupied resources.");
            
            try
            {
                // Las propiedades CurrentTargetable y TargetBed manejan la liberación
                // de recursos automáticamente al asignarles null
                CurrentTargetable = null;
                TargetBed = null;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error releasing resources for {name}: {ex.Message}");
            }
        }

        private void Update()
        {
            if (!_isInitialized) return;
            
            var nextState = _priorities.DecideNextState();
            
            // Log para debug del flujo de decisiones
            if (nextState != _currentState)
            {
                Debug.Log($"{name} Update: Transitioning from {_currentState?.GetType().Name ?? "null"} to {nextState.GetType().Name}");
            }
            
            TransitionToState(nextState);
            CurrentState?.UpdateState();
        }

        public HashSet<World.Targetable> GetBlacklistedTargets()
        {
            return _blacklist.GetBlacklistedTargets();
        }

        private Dictionary<World.Targetable, int> _failedPathAttempts = new();

        public DwarfBaseState GetStateForCurrentTarget()
        {
            if (CurrentTargetable == null) 
            {
                Debug.Log($"{name} GetStateForCurrentTarget: No current targetable, returning IdleState");
                return IdleState;
            }

            Debug.Log($"{name} GetStateForCurrentTarget: Checking distance to {CurrentTargetable.name}");
            
            if (!IsAdjacentToTarget(CurrentTargetable.transform.position))
            {
                Debug.Log($"{name} GetStateForCurrentTarget: Not adjacent to {CurrentTargetable.name}, finding walkable node");
                Vector3? destination = FindWalkableAdjacentNode(CurrentTargetable.transform.position);
                if (destination.HasValue)
                {
                    if (_failedPathAttempts.ContainsKey(CurrentTargetable))
                        _failedPathAttempts.Remove(CurrentTargetable);
                    
                    WalkingState.SetDestination(destination.Value, WalkPurpose.Work);
                    Debug.Log($"{name} GetStateForCurrentTarget: Found walkable node, returning WalkingState");
                    return WalkingState;
                }

                Debug.LogWarning($"{name} GetStateForCurrentTarget: No walkable node found for {CurrentTargetable.name}");
                HandlePathfindingFailure();
                return IdleState;
            }
            
            Debug.Log($"{name} GetStateForCurrentTarget: Adjacent to {CurrentTargetable.name}, returning WorkingState");
            return WorkingState;
        }

        private DwarfBaseState DecideNextState()
        {
            return _priorities.DecideNextState();
        }

        private void HandlePathfindingFailure()
        {
            if (CurrentTargetable == null) return;

            _blacklist.RegisterPathfindingFailure(CurrentTargetable);
            CurrentTargetable = null;
        }

        public void TransitionToState(DwarfBaseState newState)
        {
            if (newState == null)
                throw new System.ArgumentNullException(nameof(newState));

            if (_currentState?.GetType() == newState.GetType()) 
                return;

            try
            {
                _currentState?.ExitState();
                _currentState = newState;
                _currentState.EnterState();
                Debug.Log($"{name} transitioned to {newState.GetType().Name} state");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during state transition for {name}: {ex.Message}");
                // En caso de error, intentar volver a estado idle
                if (newState.GetType() != typeof(IdleState))
                {
                    Debug.Log($"Attempting to fallback to idle state for {name}");
                    TransitionToState(_idleState);
                }
            }
        }

        public void AssignDirective(PlayerDirective directive)
        {
            if (directive == null)
                throw new System.ArgumentNullException(nameof(directive));

            if (directive.Target == null)
            {
                Debug.LogError($"Invalid directive for {name}: target is null");
                return;
            }

            try
            {
                Debug.Log($"New directive received for {name} to target {directive.Target.name}");
                _directiveQueue.Clear();
                _directiveQueue.Enqueue(directive);
                ClearBlacklist();
                CurrentTargetable = null;
                
                var nextState = DecideNextState();
                TransitionToState(nextState);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to assign directive to {name}: {ex.Message}");
                _directiveQueue.Clear();
            }
        }

        private void HandleDayStart()
        {
            if (!_isInitialized)
            {
                _dwarfData.AgeOneDay();
            }
           
            try
            {
                // Verificar si el enano debe morir
                if (_dwarfData.AgeInDays >= _dwarfData.LifespanInDays)
                {
                    Debug.Log($"{name} has died of old age at {_dwarfData.AgeInDays} days old");
                    Destroy(gameObject);
                    return;
                }
                
                Debug.Log($"{name} acknowledges it's a new day");
                
                // Resetear estados
                _isNight = false;
                _isSleepWindow = false;
                _isWakeUpTime = false;
                
                // Inicializar presupuestos y limpiar estado
                _dwarfData.InitializeBudgets(TimeManager.Instance.TotalDayDuration);
                _dwarfData.ResetDay();
                ClearBlacklist();
                
                // Limpiar objetivos
                CurrentTargetable = null;
                
                // Manejar despertar si está durmiendo
                if (_currentState is SleepingState)
                {
                    TargetBed = null; // La propiedad maneja la liberación
                    var nextState = DecideNextState();
                    TransitionToState(nextState);
                }
                
                _isInitialized = true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during day start for {name}: {ex.Message}");
                enabled = false;
            }
        }

        private void HandleNightStart()
        {
            if (!_isInitialized) return;
            Debug.Log($"{name} acknowledges night has fallen.");
            _isNight = true;
            _isSleepWindow = false;
            _isWakeUpTime = false;
            
            // No cancelamos directivas inmediatamente, permitimos terminar tareas hasta la ventana de sueño
            var nextState = DecideNextState();
            TransitionToState(nextState);
        }

        private void HandleSleepWindowStart()
        {
            if (!_isInitialized) return;
            Debug.Log($"{name} acknowledges it's time to sleep, cancelling all tasks.");
            _isSleepWindow = true;
            _isWakeUpTime = false;
            _dwarfData.CurrentDirective = null;
            CurrentTargetable = null;
            DirectiveQueue.Clear();

            var nextState = DecideNextState();
            TransitionToState(nextState);
        }

        private void HandleWakeUpTime()
        {
            if (!_isInitialized) return;
            Debug.Log($"{name} is preparing to wake up for the new day.");
            _isWakeUpTime = true;

            if (CurrentState is SleepingState)
            {
                // Despertar anticipadamente si es necesario
                var nextState = DecideNextState();
                TransitionToState(nextState);
            }
        }

        private void HandleTargetOccupied(World.Targetable occupiedTarget, GameObject occupier)
        {
            if (occupier == gameObject) return; // Ignorar si somos nosotros mismos los que ocupamos el objetivo

            bool isMyWorkTarget = CurrentTargetable == occupiedTarget;
            bool isMyBedTarget = TargetBed != null && occupiedTarget.TryGetComponent(out World.Bed bed) && bed == TargetBed;

            if (isMyWorkTarget || isMyBedTarget)
            {
                HandleTargetLost(occupiedTarget, occupier?.name ?? "unknown");
            }
        }

        private void HandleTargetFreed(World.Targetable freedTarget)
        {
            // Si estamos en estado Living o Idle, podemos considerar este nuevo objetivo
            if ((CurrentState is LivingState || CurrentState is IdleState) && 
                _dwarfData.WorkTimeBudget > 0 && !_isNight)
            {
                Debug.Log($"{name} noticed {freedTarget.name} is now available.");
                CurrentTargetable = freedTarget;
                var nextState = DecideNextState();
                TransitionToState(nextState);
            }
        }

        private void HandleTargetTransferred(World.Targetable target, GameObject oldOccupier, GameObject newOccupier)
        {
            if (oldOccupier == gameObject)
            {
                Debug.Log($"{name} lost control of {target.name} to {newOccupier.name}.");
                HandleTargetLost(target, newOccupier.name);
            }
        }

        private void HandleTargetLost(World.Targetable target, string newOwner)
        {
            Debug.Log($"{name} can no longer use {target.name} (taken by {newOwner}).");
            
            if (CurrentTargetable == target)
            {
                CurrentTargetable = null;
                if (DirectiveQueue.Count > 0 && DirectiveQueue.Peek().Target == target)
                {
                    DirectiveQueue.Dequeue(); // Eliminar la directiva si era para este objetivo
                }
            }

            World.Bed bed;
            if (target.TryGetComponent(out bed) && System.Object.ReferenceEquals(bed, TargetBed))
            {
                TargetBed = null;
            }

            // Si estábamos caminando hacia el objetivo, buscar uno nuevo
            if (CurrentState is WalkingState)
            {
                var nextState = DecideNextState();
                TransitionToState(nextState);
            }
        }
        
        public void ClearBlacklist()
        {
            _blacklist.Clear();
        }

        public void PruneBlacklist()
        {
            // La limpieza se maneja internamente en DwarfBlacklist
        }

        public bool IsAdjacentToTarget(Vector3 targetPosition) 
        {
            float distance = Vector3.Distance(transform.position, targetPosition);
            bool isAdjacent = distance < 2.5f; // Increased from 1.8f to 2.5f for better detection
            
            if (CurrentTargetable != null)
            {
                Debug.Log($"{name} distance to {CurrentTargetable.name}: {distance:F2} (adjacent: {isAdjacent})");
            }
            
            return isAdjacent;
        }

        public Vector3? FindWalkableAdjacentNode(Vector3 targetPosition)
        {
            PathNode targetNode = PathfindingGrid.Instance.WorldPointToNode(targetPosition);
            if (targetNode == null) return null;

            var neighbors = PathfindingGrid.Instance.GetNeighbours(targetNode);
            if (TargetBed != null && targetPosition == TargetBed.transform.position)
            {
                var bedTile2Pos = new Vector3(targetPosition.x, 0, targetPosition.z + 1);
                PathNode bedNode2 = PathfindingGrid.Instance.WorldPointToNode(bedTile2Pos);
                if (bedNode2 != null) neighbors.AddRange(PathfindingGrid.Instance.GetNeighbours(bedNode2));
            }
            
            return neighbors.Where(n => n.isWalkable)
                            .OrderBy(n => Vector3.Distance(transform.position, n.worldPosition))
                            .FirstOrDefault()?.worldPosition;
        }
    }
}

// ScriptRole: Core AI brain for a dwarf entity, managing state machine, decision making, and resource targeting.
// Dependencies: 
//   - Required Components: DwarfData, DwarfMovement
//   - Required Systems: TimeManager, PathfindingGrid
// HandlesEvents: 
//   - World.Targetable: OnTargetOccupied, OnTargetFreed, OnTargetTransferred
//   - TimeManager: OnDayStart, OnNightStart, OnSleepWindowStart, OnWakeUpTime
// States:
//   - Idle: Default state when no action is needed
//   - Walking: Moving towards a target
//   - Working: Interacting with a work target
//   - Sleeping: Resting in a bed
//   - Living: Performing non-work activities
// Error Handling:
//   - Validates all component dependencies on startup
//   - Handles state transition failures with fallback to Idle
//   - Manages resource cleanup on target changes
// NeedsSetup:
//   1. Attach to Dwarf prefab
//   2. Configure AI Settings in Inspector:
//      - Blacklist Cooldown: 5-30 seconds
//      - Max Path Attempts: 1-5 tries
//   3. Ensure TimeManager exists in scene