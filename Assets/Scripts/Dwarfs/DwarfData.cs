using UnityEngine;
using Core;
using Core.Shared;
using SO;
using System;
using TMPro;

namespace Dwarfs
{
    [RequireComponent(typeof(DwarfBrain), typeof(DwarfMovement))]
    public class DwarfData : MonoBehaviour
    {
        private static int _dwarfCounter = 0;
        public int DwarfId { get; private set; }

        [Header("Configuration")]
        [SerializeField] 
        [Required]
        private DwarfStatsSO _stats;
        public DwarfStatsSO Stats 
        { 
            get
            {
                if (_stats == null)
                    throw new InvalidOperationException($"DwarfStatsSO not assigned to {name}!");
                return _stats;
            }
        }

        [Header("Customization")]
        [SerializeField] 
        private DwarfCustomizationSO customization;
        private string _customName = string.Empty;
        private Sprite _customIcon;
        private DwarfGender _gender;

        [Header("Time Management")]
        [SerializeField]
        [Required]
        private DwarfTimeBudgetsSO _timeBudgets;
        public DwarfTimeBudgetsSO TimeBudgets
        {
            get
            {
                if (_timeBudgets == null)
                    throw new InvalidOperationException($"DwarfTimeBudgetsSO not assigned to {name}!");
                return _timeBudgets;
            }
        }

        [Header("State")]
        private PlayerDirective _currentDirective;
        public PlayerDirective CurrentDirective
        {
            get => _currentDirective;
            set
            {
                if (_currentDirective != value)
                {
                    _currentDirective = value;
                    OnDirectiveChanged?.Invoke();
                }
            }
        }

        // Public properties for the brain to access
        public float WorkTimeBudget { get; private set; }
        public float LivingTimeBudget { get; private set; }
        
        // Lifecycle
        public int AgeInDays { get; private set; }
        public int LifespanInDays { get; private set; }

        // Events to notify state changes
        public event Action OnStatsChanged;
        public event Action OnBudgetsChanged;
        public event Action OnDirectiveChanged;
        public event Action OnAgeChanged;
        public event Action OnCustomizationChanged;

        // Public properties for customization
        public string CustomName 
        { 
            get => _customName;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Dwarf name cannot be empty or whitespace.");
                _customName = value.Trim();
                name = _customName; // Update GameObject name
            }
        }
        
        public Sprite CustomIcon => _customIcon;
        public DwarfGender Gender => _gender;

        private void Awake()
        {
            _dwarfCounter++;
            DwarfId = _dwarfCounter;
            
            ValidateReferences();
            InitializeCustomization();
            
            if (_stats != null)
            {
                LifespanInDays = UnityEngine.Random.Range(_stats.LifespanDaysRange.x, _stats.LifespanDaysRange.y + 1);
                Debug.Log($"Dwarf {name} initialized with stats: {_stats.name}. Lifespan: {LifespanInDays} days.");
            }
            else
            {
                Debug.LogError($"Dwarf {name} has no stats assigned!");
                LifespanInDays = 1;
            }
        }

        private void ValidateReferences()
        {
            try
            {
                // Estas propiedades lanzarán excepciones si las referencias son null
                _ = Stats;
                _ = TimeBudgets;

                if (customization == null)
                {
                    Debug.LogWarning($"Dwarf {name} is missing DwarfCustomizationSO reference. Will use default name and icon.", this);
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogError($"Validation failed for {name}: {ex.Message}", this);
                enabled = false;
                throw; // Re-throw para que Unity muestre el error en la consola
            }
        }

        private void OnEnable()
        {
            try
            {
                TimeManager.OnDayStart += ResetBudgets;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to subscribe to TimeManager events: {ex.Message}", this);
                enabled = false;
            }
        }

        private void OnDisable()
        {
            TimeManager.OnDayStart -= ResetBudgets;
        }

        private void ResetBudgets()
        {
            if (TimeManager.Instance == null)
            {
                throw new InvalidOperationException("TimeManager instance not found!");
            }

            InitializeBudgets(TimeManager.Instance.TotalDayDuration);
            AgeOneDay();
        }

        public void AgeOneDay()
        {
            AgeInDays++;
            
            if (AgeInDays >= LifespanInDays)
            {
                Debug.Log($"{name} has reached the end of their lifespan ({AgeInDays}/{LifespanInDays} days).");
                // TODO: Implementar lógica de muerte del enano
            }
            else
            {
                Debug.Log($"{name} is now {AgeInDays} days old (Lifespan: {LifespanInDays} days).");
            }
            
            OnAgeChanged?.Invoke();
        }

        public void InitializeBudgets(float dayDuration)
        {
            if (dayDuration <= 0)
                throw new ArgumentException("Day duration must be positive!", nameof(dayDuration));

            var budgets = TimeBudgets; // Esto validará que _timeBudgets no sea null
            
            // Calcular y validar presupuestos
            float workBudget = dayDuration * budgets.workBudgetPercentage;
            float livingBudget = dayDuration * budgets.livingBudgetPercentage;

            // Validar que los presupuestos no excedan el día
            float totalBudget = workBudget + livingBudget;
            if (totalBudget > dayDuration)
            {
                float scale = dayDuration / totalBudget;
                workBudget *= scale;
                livingBudget *= scale;
                Debug.LogWarning($"Budgets exceeded day duration for {name}. Scaling to fit.", this);
            }

            WorkTimeBudget = workBudget;
            LivingTimeBudget = livingBudget;
            
            Debug.Log($"Dwarf {name} budgets initialized - Work: {WorkTimeBudget:F1}s, Living: {LivingTimeBudget:F1}s");
            OnBudgetsChanged?.Invoke();
        }

        public void ResetDay()
        {
            CurrentDirective = null; // Usa la propiedad pública
        }

        public bool ConsumeWorkTime(float amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Time amount must be positive!", nameof(amount));
                
            if (WorkTimeBudget <= 0) return false;
            
            WorkTimeBudget = Mathf.Max(0, WorkTimeBudget - amount);
            OnBudgetsChanged?.Invoke();
            return WorkTimeBudget > 0;
        }

        public bool ConsumeLivingTime(float amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Time amount must be positive!", nameof(amount));
                
            if (LivingTimeBudget <= 0) return false;
            
            LivingTimeBudget = Mathf.Max(0, LivingTimeBudget - amount);
            OnBudgetsChanged?.Invoke();
            return LivingTimeBudget > 0;
        }

        private void InitializeCustomization()
        {
            try
            {
                if (customization == null)
                {
                    CustomName = $"Dwarf-{DwarfId}";
                    _customIcon = null;
                    _gender = UnityEngine.Random.value < 0.5f ? DwarfGender.Male : DwarfGender.Female;
                    return;
                }

                // Generar género aleatorio
                _gender = customization.GenerateRandomGender();

                // Generar nombre aleatorio según género
                CustomName = customization.GenerateRandomName(_gender);

                // Obtener icono aleatorio según género
                var icon = customization.GetRandomIcon(_gender);
                if (icon == null)
                {
                    Debug.LogWarning($"No icon found for {_gender} dwarf. Using default.", this);
                }
                _customIcon = icon;

                OnCustomizationChanged?.Invoke();
                Debug.Log($"Dwarf customized: {CustomName} ({_gender})");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize customization for {name}: {ex.Message}", this);
                // Usar valores por defecto seguros
                CustomName = $"Dwarf-{DwarfId}";
                _customIcon = null;
                _gender = DwarfGender.Male;
            }
        }

        public void SetCustomName(string newName)
        {
            try
            {
                CustomName = newName; // Usa la propiedad que ya tiene validación
            }
            catch (ArgumentException ex)
            {
                Debug.LogError($"Failed to set name for {name}: {ex.Message}", this);
            }
        }

        // Removed SetCustomIcon method as part of YAGNI simplification.
        // Icons are now only set randomly through the customization system.

        public void SetGender(DwarfGender newGender)
        {
            if (!Enum.IsDefined(typeof(DwarfGender), newGender))
                throw new ArgumentException($"Invalid gender value: {newGender}", nameof(newGender));
                
            if (_gender == newGender) return;
            
            _gender = newGender;
            
            try
            {
                if (customization != null)
                {
                    // Actualizar nombre e icono para que coincidan con el nuevo género
                    CustomName = customization.GenerateRandomName(_gender);
                    _customIcon = customization.GetRandomIcon(_gender);
                }
                OnCustomizationChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to update customization after gender change: {ex.Message}", this);
                // Revertir el cambio de género si falla la actualización
                _gender = newGender;
            }
        }
    }
}

// ScriptRole: Manages core data, stats, customization and time budgets for a dwarf entity.
// Dependencies: Requires DwarfBrain and DwarfMovement components.
// UsesSO: DwarfStatsSO (required), DwarfCustomizationSO (optional), DwarfTimeBudgetsSO (required)
// ReceivesFrom: TimeManager (day cycle events)
// SendsTo: Broadcasts various state change events (OnStatsChanged, OnBudgetsChanged, etc.)
// NeedsSetup: 
//   1. Attach to Dwarf prefab
//   2. Assign required ScriptableObjects:
//      - DwarfStatsSO: Basic stats and capabilities
//      - DwarfTimeBudgetsSO: Daily time allocation
//   3. Optional: Assign DwarfCustomizationSO for name/icon generation