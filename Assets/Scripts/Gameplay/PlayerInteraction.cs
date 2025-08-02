using UnityEngine;
using Dwarfs;
using Core.Shared;

namespace Gameplay
{
    public class PlayerInteraction : MonoBehaviour
    {
        public static PlayerInteraction Instance { get; private set; }

        [Header("Selection")]
        [SerializeField] private DwarfBrain _selectedDwarf;
        
        private Camera _mainCamera;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
            _mainCamera = Camera.main;
            Debug.Log("PlayerInteraction initialized.");
        }

        private void Update()
        {
            HandleLeftClick();  // For selection
            HandleRightClick(); // For commands
        }

        private void HandleLeftClick()
        {
            if (Input.GetMouseButtonDown(0)) // Left-click
            {
                Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    DwarfBrain clickedDwarf = hit.collider.GetComponent<DwarfBrain>();
                    if (clickedDwarf != null)
                    {
                        SelectDwarf(clickedDwarf);
                    }
                }
            }
        }

        private void HandleRightClick()
        {
            if (Input.GetMouseButtonDown(1) && _selectedDwarf != null) // Right-click
            {
                Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    ITargetable clickedTarget = hit.collider.GetComponent<ITargetable>();
                    if (clickedTarget != null)
                    {
                        IssueDirective(clickedTarget);
                    }
                }
            }
        }

        private void SelectDwarf(DwarfBrain dwarf)
        {
            _selectedDwarf = dwarf;
            Debug.Log($"Dwarf selected: {_selectedDwarf.name}");
            // Optional: Add visual feedback for selection here
        }

        private void IssueDirective(ITargetable target)
        {
            var directive = new PlayerDirective(target);
            _selectedDwarf.AssignDirective(directive);
            Debug.Log($"Issuing directive to {_selectedDwarf.name} to target {target.name}");
        }
    }
}

// ScriptRole: Manages player input for selecting dwarfs and issuing commands.
// Dependencies: A Camera with the 'MainCamera' tag in the scene.
// NeedsSetup: Attach to a single GameObject in the scene (like TimeManager). GameObjects for dwarfs and targets need colliders.
