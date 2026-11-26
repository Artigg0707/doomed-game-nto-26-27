using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElectricalPanelController : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private ExpeditionManager expeditionManager;

    [Header("Balance")]
    [Tooltip("Survival config for generator parameters. If not assigned, built-in defaults are used.")]
    [SerializeField] private SurvivalConfig survivalConfig;

    [Header("Настройки генератора")]

    // Read-only properties with survival-config priority; built-in defaults are used if config is missing.
    private int BaseBreakdownChancePercent =>
        survivalConfig != null ? survivalConfig.baseBreakdownChancePercent : 15;
    private int BaseRepairSuccessChancePercent =>
        survivalConfig != null ? survivalConfig.baseRepairSuccessChancePercent : 60;
    private int BaseRepairInjuryChancePercent =>
        survivalConfig != null ? survivalConfig.baseRepairInjuryChancePercent : 25;
    private int BreakdownChanceForOnePerson =>
        survivalConfig != null ? survivalConfig.breakdownChanceForOnePerson : 60;
    private int BreakdownReductionPerAdditionalPerson =>
        survivalConfig != null ? survivalConfig.breakdownReductionPerAdditionalPerson : 10;
    private int RepairSuccessBonusPerAdditionalPerson =>
        survivalConfig != null ? survivalConfig.repairSuccessBonusPerAdditionalPerson : 10;
    private int RepairFailureDamageHP =>
        survivalConfig != null ? survivalConfig.repairFailureDamageHP : 15;

    [Header("Статус")]
    public bool IsGeneratorWorking =>
        resourceManager != null &&
        !resourceManager.GeneratorBroken &&
        resourceManager.GeneratorIsOn &&
        resourceManager.FuelCans > 0;

    public string GeneratorStatusText
    {
        get
        {
            if (resourceManager == null)
                return "Неизвестно";
            return resourceManager.GeneratorStatus;
        }
    }

    public event Action OnResourcesChanged;
    public event Action OnGeneratorStatusChanged;
    
    private GeneratedCharacter pendingRepairCharacter;
    private int pendingRepairDaySent = -1;

    public GeneratedCharacter PendingRepairCharacter => pendingRepairCharacter;
    public bool HasPendingRepair => pendingRepairCharacter != null;

    private void Awake()
    {
        if (resourceManager == null)
            resourceManager = ResourceManager.Instance;

        if (characterManager == null)
            characterManager = FindObjectOfType<CharacterManager>();

        if (expeditionManager == null)
            expeditionManager = FindObjectOfType<ExpeditionManager>();
    }

    private void OnEnable()
    {
        if (resourceManager != null)
            resourceManager.OnResourcesChanged += HandleResourcesChanged;
    }

    private void OnDisable()
    {
        if (resourceManager != null)
            resourceManager.OnResourcesChanged -= HandleResourcesChanged;
    }

    private void HandleResourcesChanged()
    {
        OnResourcesChanged?.Invoke();
        OnGeneratorStatusChanged?.Invoke();
    }
    
    public void ToggleGenerator()
    {
        if (resourceManager == null)
            return;

        if (resourceManager.GeneratorBroken)
        {
            Debug.Log("Генератор сломан. Его нельзя включить, пока его не починят.", this);
            OnGeneratorStatusChanged?.Invoke();
            return;
        }

        bool newState = !resourceManager.GeneratorIsOn;

        if (newState && !resourceManager.CanTurnOnGenerator)
        {
            Debug.Log("Недостаточно топлива для включения генератора.", this);
            OnGeneratorStatusChanged?.Invoke();
            return;
        }

        resourceManager.SetGeneratorOn(newState);
        Debug.Log(newState ? "Генератор включён." : "Генератор выключен.", this);
    }
    
    public List<GeneratedCharacter> GetAvailableRepairCharacters()
    {
        var result = new List<GeneratedCharacter>();

        if (characterManager == null || characterManager.characters == null)
            return result;

        foreach (GeneratedCharacter character in characterManager.characters)
        {
            if (character == null)
                continue;
            if (character.isDead || character.isExiled)
                continue;
            if (expeditionManager != null && expeditionManager.IsCharacterOnExpedition(character))
                continue;
            if (HasTraitEffect(character, Trait.EffectType.CannotRepairElectricity))
                continue;

            result.Add(character);
        }

        return result;
    }
    
    public bool SendCharacterForRepair(GeneratedCharacter character, out string message)
    {
        message = "";

        if (character == null)
        {
            message = "Не выбран персонаж.";
            return false;
        }

        if (!resourceManager.GeneratorBroken)
        {
            message = "Генератор не сломан, починка не требуется.";
            return false;
        }

        if (HasPendingRepair)
        {
            message = "Ремонт уже запланирован на ближайшую ночь.";
            return false;
        }

        var available = GetAvailableRepairCharacters();
        if (!available.Contains(character))
        {
            message = "Этот персонаж не может починить генератор.";
            return false;
        }

        pendingRepairCharacter = character;
        pendingRepairDaySent = DayCycleManager.Instance != null
            ? DayCycleManager.Instance.CurrentDay
            : 1;

        message = $"{character.fullName} отправлен(а) чинить генератор. Результат будет утром.";
        Debug.Log(message, this);
        OnGeneratorStatusChanged?.Invoke();
        return true;
    }
    
    public void ProcessGeneratorNight(NightReportData report)
    {
        if (resourceManager == null || report == null)
            return;
        
        if (!resourceManager.GeneratorBroken &&
            resourceManager.GeneratorIsOn &&
            report.generatorWorked)
        {
            float breakdownChance = CalculateBreakdownChance();
            if (UnityEngine.Random.Range(0, 100) < breakdownChance)
            {
                resourceManager.BreakGenerator();
                report.specialEvents.Add(
                    $"Ночью генератор сломался (шанс {breakdownChance:F0}%).");
                Debug.Log("Генератор сломался ночью.", this);
                OnGeneratorStatusChanged?.Invoke();
            }
        }
        
        if (HasPendingRepair)
        {
            ResolvePendingRepair(report);
        }
    }

    private void ResolvePendingRepair(NightReportData report)
    {
        if (pendingRepairCharacter == null)
            return;

        GeneratedCharacter character = pendingRepairCharacter;
        int aliveInBunker = CountAliveInBunker();
        float successChance = BaseRepairSuccessChancePercent + (aliveInBunker - 1) * RepairSuccessBonusPerAdditionalPerson;

        bool success = UnityEngine.Random.Range(0, 100) < successChance;

        if (success)
        {
            resourceManager.RepairGenerator();
            string msg = $"{character.fullName} починил генератор. Генератор снова работает.";
            report.specialEvents.Add(msg);
            Debug.Log(msg, this);
        }
        else
        {
            bool canBeInjured = !HasTraitEffect(character, Trait.EffectType.NoInjuryOnGeneratorRepair);

            int injuryChance = BaseRepairInjuryChancePercent;
            foreach (var trait in character.traits)
            {
                if (trait == null) continue;
                foreach (var effect in trait.effects)
                {
                    if (effect.effectType == Trait.EffectType.ElectricityRepairInjuryChanceChangePoints)
                        injuryChance += (int)effect.value;
                    if (effect.effectType == Trait.EffectType.ElectricityRepairInjuryChanceReductionPercent)
                        injuryChance = Mathf.RoundToInt(injuryChance * (1f - effect.value / 100f));
                }
            }
            injuryChance = Mathf.Clamp(injuryChance, 0, 100);

            bool injured = canBeInjured && UnityEngine.Random.Range(0, 100) < injuryChance;

            if (injured)
            {
                int damage = Mathf.Clamp(RepairFailureDamageHP, 0, 100);
                character.health = Mathf.Max(0, character.health - damage);
                string msg = $"{character.fullName} не смог(ла) починить генератор и получил(а) урон {damage} HP.";
                report.specialEvents.Add(msg);
                Debug.Log(msg, this);

                if (character.health <= 0)
                {
                    character.isDead = true;
                    if (report.deaths != null && !report.deaths.Contains(character.fullName))
                        report.deaths.Add(character.fullName);
                }
            }
            else
            {
                string msg = $"{character.fullName} не смог(ла) починить генератор. Ремонт не удался.";
                report.specialEvents.Add(msg);
                Debug.Log(msg, this);
            }
        }
        
        pendingRepairCharacter = null;
        pendingRepairDaySent = -1;
        OnGeneratorStatusChanged?.Invoke();
    }

    private int CountAliveInBunker()
    {
        int count = 0;

        if (characterManager != null && characterManager.characters != null)
        {
            foreach (GeneratedCharacter character in characterManager.characters)
            {
                if (character == null || character.isDead || character.isExiled)
                    continue;
                if (expeditionManager != null && expeditionManager.IsCharacterOnExpedition(character))
                    continue;

                count++;
            }
        }

        return count;
    }

    private float CalculateBreakdownChance()
    {
        int aliveInBunker = CountAliveInBunker();

        float chance;
        if (aliveInBunker <= 1)
            chance = BreakdownChanceForOnePerson;
        else
            chance = Mathf.Max(0, BreakdownChanceForOnePerson - (aliveInBunker - 1) * BreakdownReductionPerAdditionalPerson);

        if (characterManager != null && characterManager.characters != null)
        {
            foreach (GeneratedCharacter character in characterManager.characters)
            {
                if (character == null || character.isDead || character.isExiled)
                    continue;
                if (expeditionManager != null && expeditionManager.IsCharacterOnExpedition(character))
                    continue;

                foreach (var trait in character.traits)
                {
                    if (trait == null) continue;
                    foreach (var effect in trait.effects)
                    {
                        if (effect.effectType == Trait.EffectType.ElectricityBreakChanceChangePoints)
                            chance += effect.value;
                    }
                }
            }
        }

        return Mathf.Clamp(chance, 0, 100);
    }

    private bool HasTraitEffect(GeneratedCharacter character, Trait.EffectType effectType)
    {
        if (character == null || character.traits == null)
            return false;

        foreach (var trait in character.traits)
        {
            if (trait == null) continue;
            foreach (var effect in trait.effects)
            {
                if (effect.effectType == effectType)
                    return true;
            }
        }

        return false;
    }
}
