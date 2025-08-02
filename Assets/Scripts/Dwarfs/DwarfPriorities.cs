using UnityEngine;
using Core;
using Core.Shared;
using Dwarfs.States;
using World;

namespace Dwarfs
{
    /// <summary>
    /// Encapsula la lógica de prioridades para la toma de decisiones de los enanos.
    /// </summary>
    public class DwarfPriorities
    {
        private readonly DwarfBrain _brain;
        private readonly DwarfData _data;
        private readonly TimeManager _timeManager;

        public DwarfPriorities(DwarfBrain brain, DwarfData data)
        {
            _brain = brain;
            _data = data;
            _timeManager = TimeManager.Instance;
        }

        /// <summary>
        /// Evalúa las prioridades del enano y determina su próximo estado.
        /// </summary>
        public DwarfBaseState DecideNextState()
        {
            // Prioridad 1: Necesidades biológicas (dormir)
            var sleepState = EvaluateSleepPriority();
            if (sleepState != null) 
            {
                Debug.Log($"{_brain.name} DecideNextState: Sleep priority selected");
                return sleepState;
            }

            // Prioridad 2: Órdenes directas del jugador
            var playerDirectiveState = EvaluatePlayerDirectives();
            if (playerDirectiveState != null) 
            {
                Debug.Log($"{_brain.name} DecideNextState: Player directive priority selected");
                return playerDirectiveState;
            }

            // Prioridad 3: Trabajo autónomo
            var workState = EvaluateWorkPriority();
            if (workState != null) 
            {
                Debug.Log($"{_brain.name} DecideNextState: Work priority selected");
                return workState;
            }

            // Prioridad 4: Actividades de ocio
            var livingState = EvaluateLivingPriority();
            if (livingState != null) 
            {
                Debug.Log($"{_brain.name} DecideNextState: Living priority selected");
                return livingState;
            }

            // Estado por defecto: Idle
            Debug.Log($"{_brain.name} DecideNextState: Defaulting to Idle state");
            return _brain.IdleState;
        }

        private DwarfBaseState EvaluateSleepPriority()
        {
            if (!_timeManager.IsNight && !_brain.IsSleepWindow) return null;

            // Si es hora de dormir o ya estamos durmiendo
            if (_timeManager.IsSleepWindow || _brain.CurrentState is SleepingState)
            {
                if (_brain.TargetBed != null && _brain.IsAdjacentToTarget(_brain.TargetBed.transform.position))
                    return _brain.SleepingState;

                if (_brain.TargetBed == null)
                {
                    if (World.BedManager.Instance != null)
                    {
                        _brain.TargetBed = World.BedManager.Instance.RequestBed(_brain.gameObject);
                    }
                }

                if (_brain.TargetBed != null)
                {
                    Vector3? destination = _brain.FindWalkableAdjacentNode(_brain.TargetBed.transform.position);
                    if (destination.HasValue)
                    {
                        _brain.WalkingState.SetDestination(destination.Value, WalkPurpose.GoToBed);
                        return _brain.WalkingState;
                    }
                    _brain.TargetBed.Vacate();
                    _brain.TargetBed = null;
                }
            }

            return null;
        }

        private DwarfBaseState EvaluatePlayerDirectives()
        {
            if (_timeManager.IsSleepWindow) return null; // No aceptar órdenes durante la ventana de sueño
            if (_data.WorkTimeBudget <= 0) return null; // No trabajar si no hay presupuesto

            if (_brain.DirectiveQueue.Count > 0)
            {
                _brain.CurrentTargetable = _brain.DirectiveQueue.Peek().Target as Targetable;
                return _brain.GetStateForCurrentTarget();
            }

            return null;
        }

        private DwarfBaseState EvaluateWorkPriority()
        {
            if (_timeManager.IsSleepWindow) 
            {
                Debug.Log($"{_brain.name} EvaluateWorkPriority: Skipping work - sleep window active");
                return null;
            }
            if (_data.WorkTimeBudget <= 0) 
            {
                Debug.Log($"{_brain.name} EvaluateWorkPriority: Skipping work - no work time budget ({_data.WorkTimeBudget})");
                return null;
            }

            if (_brain.CurrentTargetable == null)
            {
                Debug.Log($"{_brain.name} EvaluateWorkPriority: No current targetable, searching for one");
                _brain.PruneBlacklist();
                _brain.CurrentTargetable = World.Targetable.FindClosest(
                    _brain.transform.position, 
                    _brain.GetBlacklistedTargets()
                );
                
                if (_brain.CurrentTargetable != null)
                {
                    Debug.Log($"{_brain.name} EvaluateWorkPriority: Found targetable {_brain.CurrentTargetable.name}");
                }
                else
                {
                    Debug.Log($"{_brain.name} EvaluateWorkPriority: No targetable found");
                }
            }

            if (_brain.CurrentTargetable != null)
            {
                Debug.Log($"{_brain.name} EvaluateWorkPriority: Has targetable {_brain.CurrentTargetable.name}, getting state");
                return _brain.GetStateForCurrentTarget();
            }

            Debug.Log($"{_brain.name} EvaluateWorkPriority: No work available");
            return null;
        }

        private DwarfBaseState EvaluateLivingPriority()
        {
            if (_timeManager.IsSleepWindow) return null;
            if (_data.LivingTimeBudget <= 0) return null;

            _brain.CurrentTargetable = null;
            return _brain.LivingState;
        }
    }
}

// ScriptRole: Encapsula la lógica de prioridades para la toma de decisiones de los enanos
// Dependencies: DwarfBrain, DwarfData, TimeManager
// RelatedScripts: DwarfBrain, DwarfData
// NeedsSetup: None - Used internally by DwarfBrain