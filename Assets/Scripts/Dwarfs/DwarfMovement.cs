using System;
using UnityEngine;
using UnityEngine.AI;

namespace Dwarfs
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class DwarfMovement : MonoBehaviour
    {
        public event Action OnArrival;

        private NavMeshAgent _agent;
        private Transform _currentTarget;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.stoppingDistance = 1.5f; // So it stops slightly before the target
            Debug.Log($"DwarfMovement initialized for {name}.");
        }

        private void Update()
        {
            if (_currentTarget != null && !_agent.pathPending)
            {
                if (_agent.remainingDistance <= _agent.stoppingDistance)
                {
                    if (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f)
                    {
                        ArrivedAtTarget();
                    }
                }
            }
        }

        public void GoToTarget(Transform target)
        {
            _currentTarget = target;
            if (_currentTarget != null)
            {
                _agent.SetDestination(_currentTarget.position);
                Debug.Log($"{name} is moving to target: {_currentTarget.name}");
            }
        }
        
        public void StopMoving()
        {
            if (_agent.hasPath)
            {
                _agent.ResetPath();
            }
            _currentTarget = null;
            Debug.Log($"{name} has stopped moving.");
        }

        private void ArrivedAtTarget()
        {
            Debug.Log($"{name} arrived at {_currentTarget.name}.");
            _currentTarget = null;
            OnArrival?.Invoke();
        }
    }
}

// ScriptRole: Handles the physical movement of the dwarf using a NavMeshAgent.
// Dependencies: NavMeshAgent.
// TriggersEvents: OnArrival.
// NeedsSetup: Attach to the Dwarf prefab. Ensure the prefab also has a NavMeshAgent component. 