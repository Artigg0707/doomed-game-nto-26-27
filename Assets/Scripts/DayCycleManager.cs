using System;
using System.Collections.Generic;
using UnityEngine;

public class DayCycleManager : MonoBehaviour
{
    [Header("Конфиг выживания")]
    [SerializeField] private SurvivalConfig survivalConfig;

    public static DayCycleManager Instance { get; private set; }

    private int VictoryDay =>
        survivalConfig != null ? survivalConfig.victoryDay : 10;

    private int BaseMoralePenaltyPerDeath =>
        survivalConfig != null ? survivalConfig.baseMoralePenaltyPerDeath : 10;

    private bool _configMissingWarned;

    [SerializeField] private ExpeditionManager expeditionManager;
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private ElectricalPanelController electricalPanelController;

    public int CurrentDay { get; private set; } = 1;

    public event Action<int> OnDayChanged;
    public event Action<NightReportData> OnNightComplete;
    public event Action OnDefeat;
    public event Action OnVictory;

    public void NotifyDayChanged() => OnDayChanged?.Invoke(CurrentDay);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (expeditionManager == null)
            expeditionManager = GetComponent<ExpeditionManager>();

        if (characterManager == null)
            characterManager = GetComponent<CharacterManager>();

        if (characterManager == null)
            characterManager = FindObjectOfType<CharacterManager>();

        if (electricalPanelController == null)
            electricalPanelController = FindObjectOfType<ElectricalPanelController>();

        ResourceManager resources = ResourceManager.Instance;

        if (resources != null && resources.resourceData != null)
        {
            if (resources.resourceData.currentDay > 0)
                CurrentDay = resources.resourceData.currentDay;
            else
                resources.resourceData.currentDay = 1;
        }

        if (survivalConfig == null)
        {
            var found = Resources.LoadAll<SurvivalConfig>("");
            if (found.Length > 0)
                survivalConfig = found[0];
        }

        if (survivalConfig == null && !_configMissingWarned)
        {
            _configMissingWarned = true;
            Debug.LogWarning(
                "DayCycleManager: SurvivalConfig не назначен и не найден в Resources. " +
                "Будут использованы значения по умолчанию (победа на 10-й день).",
                this);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void StartNextDay()
    {
        ResourceManager resources = ResourceManager.Instance;

        if (resources == null)
        {
            Debug.LogError("ResourceManager is not found.", this);
            return;
        }

        int temperatureBefore = resources.TemperatureLevel;
        bool generatorWasBroken = resources.GeneratorBroken;
        bool generatorWasOn = resources.GeneratorIsOn;

        bool fuelConsumed = false;

        if (generatorWasOn && !resources.GeneratorBroken)
        {
            fuelConsumed = resources.ConsumeFuel(1);
        }

        if (fuelConsumed)
        {
            resources.AddTemperature(1);
        }
        else
        {
            resources.SetTemperature(resources.TemperatureLevel - 1);
        }

        NightReportData report = new NightReportData
        {
            completedDay = CurrentDay,
            newDay = CurrentDay + 1,
            fuelConsumed = fuelConsumed,
            generatorWorked = generatorWasOn && !resources.GeneratorBroken && fuelConsumed,
            temperatureBefore = temperatureBefore,
            temperatureAfter = resources.TemperatureLevel
        };

        if (characterManager != null)
        {
            UpdateSurvivalCounters(resources, characterManager, report);
            characterManager.ResetDailyRations();
        }
        else
        {
            Debug.LogError(
                "CharacterManager is not assigned in DayCycleManager.",
                this);
        }

        CurrentDay++;

        if (resources.resourceData != null)
            resources.resourceData.currentDay = CurrentDay;

        report.newDay = CurrentDay;

        if (expeditionManager != null)
            expeditionManager.ProcessCompletedDay(report);

        if (electricalPanelController != null)
            electricalPanelController.ProcessGeneratorNight(report);

        if (characterManager != null)
        {
            ApplyDeathMoralePenalty(characterManager, report);
            ProcessMoraleDepartures(characterManager, report);
        }

        OnDayChanged?.Invoke(CurrentDay);
        OnNightComplete?.Invoke(report);

        if (EventManager.Instance != null)
        {
            var morningLines = EventManager.Instance.GetMorningLinesForDay(CurrentDay);
            if (morningLines != null && report != null)
            {
                foreach (var line in morningLines)
                    report.specialEvents.Add(line);
            }
        }

        if (IsDefeat())
        {
            OnDefeat?.Invoke();
        }
        else if (report.completedDay >= VictoryDay)
        {
            OnVictory?.Invoke();
        }


    }

    public bool IsDefeat()
    {
        if (characterManager == null)
            characterManager = FindObjectOfType<CharacterManager>();

        if (characterManager == null)
            return false;

        if (characterManager.characters == null)
            return true;

        foreach (GeneratedCharacter character in characterManager.characters)
        {
            if (character != null &&
                !character.isDead &&
                !character.isExiled)
            {
                return false;
            }
        }

        return true;
    }

    public string GetDefeatReason()
    {
        if (characterManager == null)
            characterManager = FindObjectOfType<CharacterManager>();

        if (characterManager == null || characterManager.characters == null)
            return "Игра окончена";

        int deadCount = 0;
        int exiledCount = 0;
        int totalAlive = 0;

        foreach (GeneratedCharacter character in characterManager.characters)
        {
            if (character == null) continue;
            if (character.isDead) deadCount++;
            if (character.isExiled) exiledCount++;
            if (!character.isDead && !character.isExiled) totalAlive++;
        }

        if (totalAlive == 0 && deadCount > 0 && exiledCount > 0)
            return "Все погибли и ушли";
        if (totalAlive == 0 && deadCount > 0)
            return "Все погибли";
        if (totalAlive == 0 && exiledCount > 0)
            return "Все ушли из бункера";
        return "Часть жителей ушла, часть осталась";
    }

    public bool IsVictory()
    {
        return !IsDefeat() && GetCompletedDay() >= VictoryDay;
    }

    private int GetCompletedDay()
    {
        return Mathf.Max(0, CurrentDay - 1);
    }

    private void UpdateSurvivalCounters(
        ResourceManager resources,
        CharacterManager cm,
        NightReportData report)
    {
        int foodSpent = 0;
        int waterSpent = 0;

        if (cm.characters != null)
        {
            foreach (GeneratedCharacter character in cm.characters)
            {
                if (character == null || character.isDead || character.isExiled)
                    continue;
                
                if (IsCharacterOnExpedition(character))
                    continue;
                
                if (character.receivesFoodToday)
                {
                    character.daysWithoutFood = 0;
                    foodSpent++;
                }
                else
                {
                    character.daysWithoutFood++;
                }
                
                if (character.receivesWaterToday)
                {
                    character.daysWithoutWater = 0;
                    waterSpent++;
                }
                else
                {
                    character.daysWithoutWater++;
                }
                
                report.survivalChanges.Add(GetCharacterStatus(character));
                
                var hungerRow = GetPenaltyRow(survivalConfig?.hungerPenalties, character.daysWithoutFood);
                if (hungerRow != null)
                {
                    if (hungerRow.moralePenalty != 0 || hungerRow.healthDamage != 0)
                    {
                        character.morale -= hungerRow.moralePenalty;
                        character.health -= hungerRow.healthDamage;
                    }

                    report.survivalChanges.Add($"  -> {hungerRow.label}.");

                    if (hungerRow.lethal && character.health <= 0)
                    {
                        character.isDead = true;
                        report.deaths.Add(character.fullName);
                    }
                }

                if (character.isDead)
                    continue;
                
                var thirstRow = GetPenaltyRow(survivalConfig?.thirstPenalties, character.daysWithoutWater);
                if (thirstRow != null)
                {
                    if (thirstRow.moralePenalty != 0 || thirstRow.healthDamage != 0)
                    {
                        character.morale -= thirstRow.moralePenalty;
                        character.health -= thirstRow.healthDamage;
                    }

                    report.survivalChanges.Add($"  -> {thirstRow.label}.");

                    if (thirstRow.lethal && character.health <= 0)
                    {
                        character.isDead = true;
                        report.deaths.Add(character.fullName);
                    }
                }
                
                if (!character.isDead && IsFreezing(resources.TemperatureLevel))
                {
                    int coldDamage = GetColdDamage(resources.TemperatureLevel);
                    int coldMorale = survivalConfig != null ? survivalConfig.coldMoralePenalty : 10;

                    character.health -= coldDamage;
                    character.morale -= coldMorale;

                    report.survivalChanges.Add(
                        $"  -> Мороз, −{coldDamage} HP, −{coldMorale} мораль.");

                    if (character.health <= 0)
                    {
                        character.isDead = true;
                        report.deaths.Add(character.fullName);
                    }
                }
                
                if (!character.isDead)
                {
                    bool foodRecovery = character.consecutiveFoodDays >= 2;
                    bool waterRecovery = character.consecutiveWaterDays >= 2;

                    if (foodRecovery || waterRecovery)
                    {
                        bool hpRecovered = false;
                        bool moraleRecovered = false;

                        if (character.health < 100)
                        {
                            character.health = Mathf.Min(100, character.health + 5);
                            hpRecovered = true;
                        }
                        if (character.morale < 100)
                        {
                            character.morale = Mathf.Min(100, character.morale + 5);
                            moraleRecovered = true;
                        }

                        string recoverySource = foodRecovery && waterRecovery ? "еду и воду" : (foodRecovery ? "еду" : "воду");
                        report.survivalChanges.Add(
                            $"  -> Восстановление после сна ({recoverySource}): +5 HP, +5 morale.");
                    }
                }
            }
        }
        
        if (report.fuelConsumed)
            report.resourceChanges.Add(
                "Топлива −1 канистра (обогрев работал).");
        else
            report.resourceChanges.Add(
                "Топлива не хватило. Обогрев отключён.");

        if (foodSpent > 0)
            report.resourceChanges.Add(
                $"Еды израсходовано: {foodSpent} банок.");

        if (waterSpent > 0)
            report.resourceChanges.Add(
                $"Воды израсходовано: {waterSpent} бутылок.");

        report.resourceChanges.Add(
            $"Еда на складе: {resources.FoodCans} банок.");

        report.resourceChanges.Add(
            $"Вода на складе: {resources.WaterBottles} бутылок.");

        report.resourceChanges.Add(
            $"Температура: {resources.TemperatureLevel}/3.");
    }

    private void ApplyDeathMoralePenalty(
        CharacterManager cm,
        NightReportData report)
    {
        if (report.deaths == null || report.deaths.Count == 0)
            return;

        if (cm.characters == null)
            return;

        
        int aliveCount = 0;
        foreach (GeneratedCharacter c in cm.characters)
        {
            if (c != null && !c.isDead && !c.isExiled)
                aliveCount++;
        }

        
        int populationMultiplier = Mathf.Max(1, 6 - aliveCount);

        foreach (GeneratedCharacter character in cm.characters)
        {
            if (character == null || character.isDead || character.isExiled)
                continue;

            if (IsCharacterOnExpedition(character))
                continue;

            int penalty = BaseMoralePenaltyPerDeath * report.deaths.Count * populationMultiplier;
            character.morale -= penalty;

            report.survivalChanges.Add(
                $"{character.fullName}: мораль −{penalty} " +
                $"из-за смерти жителей (множитель населения ×{populationMultiplier}).");
        }
    }

    private void ProcessMoraleDepartures(
        CharacterManager cm,
        NightReportData report)
    {
        if (cm.characters == null)
            return;

        foreach (GeneratedCharacter character in cm.characters)
        {
            if (character == null ||
                character.isDead ||
                character.isExiled ||
                character.morale >= 0)
            {
                continue;
            }

            if (IsCharacterOnExpedition(character))
                continue;

            character.isExiled = true;
            report.departures.Add(character.fullName);

            report.survivalChanges.Add(
                $"{character.fullName} ушёл из бункера: " +
                "мораль опустилась ниже нуля.");
        }
    }

    private bool IsCharacterOnExpedition(GeneratedCharacter character)
    {
        return expeditionManager != null &&
               expeditionManager.IsCharacterOnExpedition(character);
    }

    private string GetCharacterStatus(GeneratedCharacter character)
    {
        string foodState = character.daysWithoutFood == 0
            ? "сыт"
            : $"без еды {character.daysWithoutFood} дн.";

        string waterState = character.daysWithoutWater == 0
            ? "напоен"
            : $"без воды {character.daysWithoutWater} дн.";

        string healthState = character.health >= 75
            ? "здоров"
            : character.health >= 35
                ? "ранен"
                : "при смерти";

        return $"{character.fullName}: {foodState}, {waterState}, " +
               $"{character.health} HP, мораль {character.morale} " +
               $"({healthState}).";
    }

    private SurvivalConfig.SurvivalPenaltyRow GetPenaltyRow(
        List<SurvivalConfig.SurvivalPenaltyRow> rows,
        int daysWithout)
    {
        if (rows == null || rows.Count == 0)
            return null;

        SurvivalConfig.SurvivalPenaltyRow best = null;

        foreach (var row in rows)
        {
            if (row == null) continue;

            // Точное совпадение дня — приоритет.
            if (row.day == daysWithout)
                return row;

            // Иначе самая поздняя подходящая строка действует каждый последующий день.
            if (row.day <= daysWithout &&
                (best == null || row.day > best.day))
                best = row;
        }

        return best;
    }

    private bool IsFreezing(int temperatureLevel)
    {
        if (survivalConfig != null)
            return temperatureLevel <= survivalConfig.freezingTemperatureThreshold;

        return temperatureLevel <= 1;
    }

    private int GetColdDamage(int temperatureLevel)
    {
        if (survivalConfig != null)
            return temperatureLevel == 0 ? survivalConfig.coldDamageAtZero : survivalConfig.coldDamageLow;

        return temperatureLevel == 0 ? 30 : 15;
    }
}
