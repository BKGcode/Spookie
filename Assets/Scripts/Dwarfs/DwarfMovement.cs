using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

namespace Dwarfs
{
    public class DwarfMovement : MonoBehaviour
    {
        public event Action OnArrival;
        
        [Header("Movement Settings")]
        [SerializeField] private float speed = 5f; // Speed is now higher for quicker tile-to-tile movement.
        [SerializeField] private LayerMask groundLayer;

        private Coroutine _movementCoroutine;

        private void OnEnable()
        {
            // No longer need to listen for terrain generation.
            // TerrainEvents.OnTerrainGenerated.AddListener(HandleTerrainGenerated);
        }

        private void OnDisable()
        {
            // TerrainEvents.OnTerrainGenerated.RemoveListener(HandleTerrainGenerated);
        }

        public void GoToTarget(Vector3 targetPosition)
        {
            if (_movementCoroutine != null)
            {
                StopCoroutine(_movementCoroutine);
            }

            List<PathNode> newPath = Pathfinder.FindPath(transform.position, targetPosition);
            
            if (newPath != null && newPath.Count > 0)
            {
                _movementCoroutine = StartCoroutine(FollowPathRoutine(newPath));
                Debug.Log($"{name} is moving to {targetPosition} via a path of {newPath.Count} nodes.");
            }
            else
            {
                Debug.LogWarning($"{name} could not find a path to {targetPosition}.");
                OnArrival?.Invoke(); 
            }
        }
        
        public void StopMoving()
        {
            if (_movementCoroutine != null)
            {
                StopCoroutine(_movementCoroutine);
                _movementCoroutine = null;
            }
            Debug.Log($"{name} has stopped moving.");
        }

        private IEnumerator FollowPathRoutine(List<PathNode> path)
        {
            foreach (var node in path)
            {
                Vector3 startPosition = transform.position;
                // The node's worldPosition contains the correct floor-level Y coordinate.
                Vector3 targetPosition = new Vector3(node.worldPosition.x, transform.position.y, node.worldPosition.z);
                
                // Instantly face the target direction on the horizontal plane.
                Vector3 direction = targetPosition - startPosition;
                direction.y = 0;
                if (direction.sqrMagnitude > 0.001f) // Check to avoid looking down if not moving
                {
                    transform.rotation = Quaternion.LookRotation(direction);
                }

                // Move from tile to tile
                float distance = Vector3.Distance(startPosition, targetPosition);
                float duration = distance / speed;
                float elapsedTime = 0f;

                while (elapsedTime < duration)
                {
                    // Lerp position and maintain the current Y to stay on the correct plane.
                    Vector3 newPos = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
                    transform.position = newPos;

                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                // Ensure final position is exactly on the target plane.
                transform.position = targetPosition;
            }
            
            ArrivedAtDestination();
        }
        
        private void ArrivedAtDestination()
        {
            Debug.Log($"{name} arrived at destination.");
            _movementCoroutine = null;
            OnArrival?.Invoke();
        }
    }
}

// ScriptRole: Handles the physical movement of the dwarf using a pathfinding system.
// Dependencies: Transform
// TriggersEvents: OnArrival
// NeedsSetup: Attach to the Dwarf prefab. The 'speed' can be configured in the Inspector. 