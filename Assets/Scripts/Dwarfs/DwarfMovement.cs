using System;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

namespace Dwarfs
{
    [RequireComponent(typeof(CharacterController))]
    public class DwarfMovement : MonoBehaviour
    {
        public event Action OnArrival;
        
        [Header("Movement Settings")]
        [SerializeField] private float speed = 3f;

        private CharacterController _characterController;
        private List<PathNode> _path;
        private int _pathIndex;
        private Vector3 _velocity;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            ApplyGravity();
            FollowPath();
        }

        public void GoToTarget(Transform target)
        {
            List<PathNode> newPath = Pathfinder.FindPath(transform.position, target.position);
            if (newPath != null && newPath.Count > 0)
            {
                _path = newPath;
                _pathIndex = 0;
                Debug.Log($"{name} found a path to {target.name} with {_path.Count} nodes.");
            }
            else
            {
                Debug.LogWarning($"{name} could not find a path to {target.name}.");
                _path = null;
                OnArrival?.Invoke(); 
            }
        }
        
        public void StopMoving()
        {
            _path = null;
            _pathIndex = 0;
            Debug.Log($"{name} has stopped moving.");
        }

        private void ApplyGravity()
        {
            if (_characterController.isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }
            _velocity.y += Physics.gravity.y * Time.deltaTime;
            _characterController.Move(_velocity * Time.deltaTime);
        }

        private void FollowPath()
        {
            if (_path == null || _pathIndex >= _path.Count) return;

            Vector3 targetNodePosition = _path[_pathIndex].worldPosition;
            
            // We use the CharacterController to move, which handles collisions.
            Vector3 moveDirection = (targetNodePosition - transform.position).normalized;
            _characterController.Move(moveDirection * speed * Time.deltaTime);
            
            if (moveDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection);
            }

            // Check if we are close enough to the current node to switch to the next one.
            if (Vector3.Distance(transform.position, targetNodePosition) < 0.5f)
            {
                _pathIndex++;
                if (_pathIndex >= _path.Count)
                {
                    ArrivedAtDestination();
                }
            }
        }
        
        private void ArrivedAtDestination()
        {
            Debug.Log($"{name} arrived at destination.");
            _path = null;
            OnArrival?.Invoke();
        }
    }
} 