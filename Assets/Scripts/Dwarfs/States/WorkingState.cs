using UnityEngine;
using Pathfinding;

namespace Dwarfs.States
{
    public class WorkingState : DwarfBaseState
    {
        private float _workTimer;

        public WorkingState(DwarfBrain brain, DwarfData data, DwarfMovement movement) : base(brain, data, movement) { }

        public override void EnterState()
        {
            if (_brain.CurrentTargetable == null)
            {
                Debug.LogWarning($"{_data.name} entered Working state with no target. Brain should prevent this.");
                return;
            }
            
            Debug.Log($"{_data.name} is now Working on {_brain.CurrentTargetable.name}.");
            _workTimer = 0f;
            _brain.CurrentTargetable.SetOccupancy(true);
        }

        public override void UpdateState()
        {
            if (_brain.CurrentTargetable == null) return;

            _data.ConsumeWorkTime(Time.deltaTime);
            _workTimer += Time.deltaTime;

            if (_workTimer >= _data.Stats.WorkCycleTime)
            {
                CompleteWork();
            }
        }

        public override void ExitState()
        {
            if (_brain.CurrentTargetable != null)
            {
                // Unset occupancy only if this dwarf was the one working on it.
                _brain.CurrentTargetable.SetOccupancy(false);
            }
        }

        private void CompleteWork()
        {
            if (_brain.CurrentTargetable == null) return;

            Debug.Log($"{_data.name} completed work on {_brain.CurrentTargetable.name}.");

            Vector3 targetPosition = _brain.CurrentTargetable.transform.position;
            
            // This assumes the targetable is on a tile that can be mined.
            TerrainManager.Instance.MineTileAt(Mathf.RoundToInt(targetPosition.x), Mathf.RoundToInt(targetPosition.z));
            PathfindingGrid.Instance.UpdateNodeWalkability(targetPosition, true);
            
            // Remove the completed directive if it was one.
            if (_brain.DirectiveQueue.Count > 0 && _brain.DirectiveQueue.Peek().Target == _brain.CurrentTargetable)
            {
                _brain.DirectiveQueue.Dequeue();
            }

            _brain.CurrentTargetable = null; 
            // The brain will transition to a new state in the next Update.
        }
    }
}

// ScriptRole: Manages the dwarf's behavior while it's actively working on a target.
// UsesSO: DwarfStatsSO (indirectly via DwarfData) 