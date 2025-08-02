using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dwarfs;
using Core.Shared;
using Core;

namespace UI
{
    public class DwarfInfoPanel : MonoBehaviour
    {
        [Header("UI References")]
        public Image dwarfIcon;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI ageText;
        public TextMeshProUGUI statusText;
        public TextMeshProUGUI taskText;
        public Image workTimeBar;
        public Image livingTimeBar;
        public GameObject actionButtonsContainer;

        private DwarfData _dwarfData;
        private DwarfBrain _dwarfBrain;

        public void Initialize(DwarfData dwarfData, DwarfBrain dwarfBrain)
        {
            if (dwarfData == null)
                throw new System.ArgumentNullException(nameof(dwarfData));
            if (dwarfBrain == null)
                throw new System.ArgumentNullException(nameof(dwarfBrain));

            _dwarfData = dwarfData;
            _dwarfBrain = dwarfBrain;

            // Suscribirse a eventos
            _dwarfData.OnCustomizationChanged += UpdateCustomization;
            _dwarfData.OnAgeChanged += UpdateAge;
            _dwarfData.OnBudgetsChanged += UpdateTimeBars;
            _dwarfData.OnDirectiveChanged += UpdateTask;

            // Actualización inicial
            UpdateAll();
        }

        private void OnDestroy()
        {
            if (_dwarfData != null)
            {
                _dwarfData.OnCustomizationChanged -= UpdateCustomization;
                _dwarfData.OnAgeChanged -= UpdateAge;
                _dwarfData.OnBudgetsChanged -= UpdateTimeBars;
                _dwarfData.OnDirectiveChanged -= UpdateTask;
            }
        }

        private void Update()
        {
            if (_dwarfBrain != null)
            {
                UpdateStatus();
            }
        }

        private void UpdateAll()
        {
            UpdateCustomization();
            UpdateAge();
            UpdateStatus();
            UpdateTask();
            UpdateTimeBars();
        }

        private void UpdateCustomization()
        {
            if (_dwarfData == null) return;

            // Actualizar nombre
            nameText.text = _dwarfData.CustomName;

            // Actualizar icono
            if (_dwarfData.CustomIcon != null)
            {
                dwarfIcon.sprite = _dwarfData.CustomIcon;
                dwarfIcon.gameObject.SetActive(true);
            }
            else
            {
                dwarfIcon.gameObject.SetActive(false);
            }
        }

        private void UpdateAge()
        {
            if (_dwarfData == null) return;
            ageText.text = $"Age: {_dwarfData.AgeInDays}/{_dwarfData.LifespanInDays} days";
        }

        private void UpdateStatus()
        {
            if (_dwarfBrain == null || _dwarfBrain.CurrentState == null) return;
            statusText.text = $"Status: {_dwarfBrain.CurrentState.GetType().Name.Replace("State", "")}";
        }

        private void UpdateTask()
        {
            if (_dwarfData == null) return;
            
            var directive = _dwarfData.CurrentDirective;
            taskText.text = directive != null ? 
                $"Task: {directive.Description}" : 
                "Task: None";
        }

        private void UpdateTimeBars()
        {
            if (_dwarfData == null) return;

            var timeManager = TimeManager.Instance;
            if (timeManager == null)
            {
                Debug.LogWarning("TimeManager not found. Cannot update time bars.");
                return;
            }

            float maxWorkTime = _dwarfData.TimeBudgets.workBudgetPercentage * timeManager.TotalDayDuration;
            float maxLivingTime = _dwarfData.TimeBudgets.livingBudgetPercentage * timeManager.TotalDayDuration;

            workTimeBar.fillAmount = maxWorkTime > 0 ? _dwarfData.WorkTimeBudget / maxWorkTime : 0;
            livingTimeBar.fillAmount = maxLivingTime > 0 ? _dwarfData.LivingTimeBudget / maxLivingTime : 0;
        }
    }
}

// ScriptRole: Manages the UI panel that displays comprehensive dwarf information
// Dependencies: TextMeshProUGUI, Image components, DwarfData, DwarfBrain
// HandlesEvents: 
//   - DwarfData.OnCustomizationChanged
//   - DwarfData.OnAgeChanged
//   - DwarfData.OnBudgetsChanged
//   - DwarfData.OnDirectiveChanged
// NeedsSetup:
//   1. Created automatically via CreateDwarfUIMenu
//   2. References assigned in editor script
//   3. Call Initialize with DwarfData and DwarfBrain references