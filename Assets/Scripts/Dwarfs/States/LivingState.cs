using UnityEngine;
using Pathfinding;

namespace Dwarfs.States
{
    public class LivingState : DwarfBaseState
    {
        private float _livingTimer;
        private const float LivingDuration = 10f; // Time to wait before re-evaluating
        private const float WanderRadius = 2f; // Radio muy reducido para evitar targets no walkables
        private Vector3 _wanderCenter; // Centro de deambulación
        private bool _isWandering = false;
        private int _wanderAttempts = 0;
        private const int MaxWanderAttempts = 5; // Reducido para evitar bucles largos

        public LivingState(DwarfBrain brain, DwarfData data, DwarfMovement movement) : base(brain, data, movement) { }

        public override void EnterState()
        {
            Debug.Log($"{_data.name} is now in Living state. Wandering or resting.");
            _livingTimer = 0f;
            _wanderCenter = _brain.transform.position; // Establecer centro de deambulación
            _isWandering = false;
            _wanderAttempts = 0;
        }

        public override void UpdateState()
        {
            // CRÍTICO: Validar LivingTimeBudget antes de consumir
            if (_data.LivingTimeBudget <= 0)
            {
                Debug.Log($"{_data.name} LivingState: LivingTimeBudget exhausted, transitioning to IdleState");
                _brain.TransitionToState(_brain.IdleState);
                return;
            }
            
            // Consumir LivingTimeBudget según el GDD
            _data.ConsumeLivingTime(Time.deltaTime);
            
            // Implementar comportamiento de ocio según GDD
            if (!_isWandering)
            {
                StartWandering();
            }
            
            _livingTimer += Time.deltaTime;
            if (_livingTimer >= LivingDuration)
            {
                // After some time, go back to idle to check for new day or new opportunities
                Debug.Log($"{_data.name} LivingState: Living duration completed, transitioning to IdleState");
                _brain.TransitionToState(_brain.IdleState);
            }
        }

        private void StartWandering()
        {
            if (_wanderAttempts >= MaxWanderAttempts)
            {
                Debug.Log($"{_data.name} LivingState: Max wander attempts reached, staying in place");
                _isWandering = true; // Evitar bucle infinito
                return;
            }

            _wanderAttempts++;
            
            // Generar punto aleatorio dentro del radio de deambulación
            Vector2 randomOffset = Random.insideUnitCircle * WanderRadius;
            Vector3 wanderTarget = _wanderCenter + new Vector3(randomOffset.x, 0, randomOffset.y);
            
            // VERIFICAR SI EL TARGET ES WALKABLE ANTES DE INTENTAR MOVERSE
            PathNode targetNode = PathfindingGrid.Instance.WorldPointToNode(wanderTarget);
            if (targetNode != null && targetNode.walkable)
            {
                Debug.Log($"{_data.name} LivingState: Starting to wander to walkable target {wanderTarget}");
                
                // Usar WalkingState para moverse al punto de deambulación
                _brain.WalkingState.SetDestination(wanderTarget, WalkPurpose.Wander);
                _brain.TransitionToState(_brain.WalkingState);
                _isWandering = true;
            }
            else
            {
                Debug.Log($"{_data.name} LivingState: Target {wanderTarget} is not walkable, trying again (attempt {_wanderAttempts})");
                // No marcar como wandering para que intente de nuevo en el próximo UpdateState
                _isWandering = false;
            }
        }

        public override void ExitState()
        {
            // Nothing to clean up
        }
    }
}

// ScriptRole: Manages the dwarf's behavior when it has no work to do, causing it to wander or rest temporarily. 