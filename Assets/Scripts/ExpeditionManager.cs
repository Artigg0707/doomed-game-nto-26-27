using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ExpeditionManager : MonoBehaviour
{
    [Serializable]
    public class ExpeditionRun
    {
        public ExpeditionLocationData location;
        public GeneratedCharacter character;
        public int daysRemaining;
    }

    [Serializable]
    private class LocationCooldown
    {
        public ExpeditionLocationData location;
        public int availableOnDay;
    }

    [Header("References")]
    [SerializeField] private CharacterManager characterManager;

    [Header("Balance")]
    [Tooltip("Survival config for expedition parameters. If not assigned, built-in defaults are used.")]
    [SerializeField] private SurvivalConfig survivalConfig;

    [Header("Limits")]
    [Header("Expedition population dependence")]
    [Tooltip("Base population at which expedition is considered safe (no extra chances).")]

    

    

    // Read-only properties with survival-config priority; built-in defaults are used if config is missing.
    private int ReferencePopulation =>
        survivalConfig != null ? survivalConfig.referencePopulation : 5;
    private int ExpeditionDeathChanceIncreasePerMissingPerson =>
        survivalConfig != null ? survivalConfig.expeditionDeathChanceIncreasePerMissingPerson : 5;
    private int ExpeditionInjuryChanceIncreasePerMissingPerson =>
        survivalConfig != null ? survivalConfig.expeditionInjuryChanceIncreasePerMissingPerson : 5;

    [Header("Runtime state")]
    [SerializeField] private List<ExpeditionRun> activeExpeditions =
        new List<ExpeditionRun>();

    [SerializeField] private List<LocationCooldown> locationCooldowns =
        new List<LocationCooldown>();

    public event Action OnExpeditionsChanged;

    public void NotifyExpeditionsChanged() => OnExpeditionsChanged?.Invoke();

    public int ActiveExpeditionCount =>
        activeExpeditions != null ? activeExpeditions.Count : 0;

    public int MaxConcurrentExpeditions =>
        Mathf.Max(1, Mathf.RoundToInt(AliveCharactersCount() / 2f));

    private int AliveCharactersCount()
    {
        if (characterManager == null || characterManager.characters == null) return 1;
        int count = 0;
        foreach (var character in characterManager.characters)
        {
            if (character != null && !character.isDead && !character.isExiled && !IsCharacterOnExpedition(character))
                count++;
        }
        return Mathf.Max(1, count);
    }

    private int lastProcessedDay = -1;

    private int CurrentDay
    {
        get
        {
            if (DayCycleManager.Instance != null)
                return Mathf.Max(1, DayCycleManager.Instance.CurrentDay);

            return 1;
        }
    }

    private void Awake()
    {
        if (characterManager == null)
            characterManager = GetComponent<CharacterManager>();

        if (activeExpeditions == null)
            activeExpeditions = new List<ExpeditionRun>();

        if (locationCooldowns == null)
            locationCooldowns = new List<LocationCooldown>();
    }

    private void Start()
    {
        if (characterManager == null)
        {
            characterManager = GetComponent<CharacterManager>();
            if (characterManager == null)
                characterManager = FindObjectOfType<CharacterManager>();
        }

        if (characterManager == null)
            Debug.LogError(
                "[ExpeditionManager] CharacterManager не назначен и не найден на сцене. " +
                "Вылазки недоступны.", this);

        if (DayCycleManager.Instance == null)
            Debug.LogWarning(
                "[ExpeditionManager] DayCycleManager.Instance не найден на сцене. " +
                "CurrentDay будет считаться равным 1.", this);

        if (activeExpeditions == null)
            activeExpeditions = new List<ExpeditionRun>();
        if (locationCooldowns == null)
            locationCooldowns = new List<LocationCooldown>();
    }

    public ExpeditionRun GetActiveExpeditionAt(int index)
    {
        if (activeExpeditions == null ||
            index < 0 ||
            index >= activeExpeditions.Count)
        {
            return null;
        }

        return activeExpeditions[index];
    }

    public bool IsCharacterOnExpedition(GeneratedCharacter character)
    {
        if (character == null || activeExpeditions == null)
            return false;

        foreach (ExpeditionRun expedition in activeExpeditions)
        {
            if (expedition != null && expedition.character == character)
                return true;
        }

        return false;
    }

    public bool IsLocationOnExpedition(ExpeditionLocationData location)
    {
        if (location == null || activeExpeditions == null)
            return false;

        foreach (ExpeditionRun expedition in activeExpeditions)
        {
            if (expedition != null && expedition.location == location)
                return true;
        }

        return false;
    }

    public int GetLocationCooldownDaysRemaining(
        ExpeditionLocationData location)
    {
        LocationCooldown cooldown = FindCooldown(location);

        if (cooldown == null)
            return 0;

        return Mathf.Max(0, cooldown.availableOnDay - CurrentDay);
    }

    public bool IsLocationAvailable(ExpeditionLocationData location)
    {
        if (location == null)
            return false;

        if (IsLocationOnExpedition(location))
            return false;

        return GetLocationCooldownDaysRemaining(location) <= 0;
    }

    public string GetLocationUnavailableReason(
        ExpeditionLocationData location)
    {
        if (location == null)
            return "Локация недоступна";

        if (IsLocationOnExpedition(location))
            return "Идёт вылазка";

        int daysRemaining = GetLocationCooldownDaysRemaining(location);

        if (daysRemaining > 0)
            return $"Доступна через {FormatDays(daysRemaining)}";

        return "";
    }

    public bool TryStartExpedition(
        ExpeditionLocationData location,
        GeneratedCharacter character,
        out string message)
    {
        if (location == null)
        {
            message = "Не выбрана локация.";
            return false;
        }

        if (character == null)
        {
            message = "Не выбран персонаж.";
            return false;
        }

        if (characterManager == null || characterManager.characters == null)
        {
            message = "Не найден CharacterManager или список персонажей.";
            return false;
        }

        if (ActiveExpeditionCount >= MaxConcurrentExpeditions)
        {
            message =
                $"Уже отправлено максимальное количество вылазок: " +
                $"{ActiveExpeditionCount}/{MaxConcurrentExpeditions}.";
            return false;
        }

        if (!IsLocationAvailable(location))
        {
            message = GetLocationUnavailableReason(location);
            return false;
        }

        if (!characterManager.characters.Contains(character))
        {
            message = "Персонаж отсутствует в списке CharacterManager.";
            return false;
        }

        if (character.isDead || character.isExiled)
        {
            message = "Этот персонаж не может отправиться в вылазку.";
            return false;
        }

        if (IsCharacterOnExpedition(character))
        {
            message = "Этот персонаж уже находится в вылазке.";
            return false;
        }
        
        if (character.receivesFoodToday)
            characterManager.SetFoodRation(character, false);

        if (character.receivesWaterToday)
            characterManager.SetWaterRation(character, false);

        int minCooldown = Mathf.Max(1, location.minCooldownDays);
        int maxCooldown = Mathf.Max(minCooldown, location.maxCooldownDays);
        int cooldownDays = UnityEngine.Random.Range(
            minCooldown,
            maxCooldown + 1);

        SetLocationCooldown(location, CurrentDay + cooldownDays);

        activeExpeditions.Add(new ExpeditionRun
        {
            location = location,
            character = character,
            daysRemaining = Mathf.Max(1, location.durationDays)
        });

        message =
            $"{character.fullName} отправлен(а) в локацию " +
            $"«{location.locationName}». Локация снова станет доступна " +
            $"через {FormatDays(cooldownDays)}.";

        OnExpeditionsChanged?.Invoke();
        return true;
    }
    
    public void ProcessCompletedDay(NightReportData report)
    {
        int processingDay = CurrentDay;
        
        if (lastProcessedDay == processingDay)
            return;

        lastProcessedDay = processingDay;

        if (activeExpeditions == null)
            activeExpeditions = new List<ExpeditionRun>();

        for (int i = activeExpeditions.Count - 1; i >= 0; i--)
        {
            ExpeditionRun expedition = activeExpeditions[i];

            if (expedition == null)
            {
                activeExpeditions.RemoveAt(i);
                continue;
            }

            expedition.daysRemaining =
                Mathf.Max(0, expedition.daysRemaining - 1);

            if (expedition.daysRemaining > 0)
                continue;

            string returnMessage = CompleteExpedition(expedition, report);

            if (report != null && report.expeditionReturns != null)
                report.expeditionReturns.Add(returnMessage);

            activeExpeditions.RemoveAt(i);
        }

        RemoveExpiredCooldowns(processingDay);
        
        OnExpeditionsChanged?.Invoke();
    }

    private string CompleteExpedition(
        ExpeditionRun expedition,
        NightReportData report)
    {
        if (expedition.character == null)
            return "Вылазка завершилась без персонажа. Награды не получены.";

        GeneratedCharacter character = expedition.character;
        ExpeditionLocationData location = expedition.location;

        if (character.isDead)
            return $"{character.fullName} погиб(ла) во время вылазки. Награды не получены.";

        if (location == null)
            return $"{character.fullName} вернулся(лась), но локация не найдена.";
        
        int aliveInBunker = CountAliveInBunker();
        int missingPeople = Mathf.Max(0, ReferencePopulation - aliveInBunker);
        int adjustedDeathChance = Mathf.Clamp(
            location.deathChancePercent + missingPeople * ExpeditionDeathChanceIncreasePerMissingPerson,
            0, 100);
        int adjustedInjuryChance = Mathf.Clamp(
            location.injuryChancePercent + missingPeople * ExpeditionInjuryChanceIncreasePerMissingPerson,
            0, 100);

        if (RollChance(adjustedDeathChance))
        {
            MarkCharacterDead(character, report);

            return
                $"{character.fullName} погиб(ла) во время вылазки " +
                $"в «{location.locationName}». Награды не получены.";
        }
        
        string injuryMessage = "";

        if (RollChance(adjustedInjuryChance))
        {
            int minDamage = Mathf.Max(0, location.minInjuryDamage);
            int maxDamage = Mathf.Max(minDamage, location.maxInjuryDamage);
            int damage = UnityEngine.Random.Range(minDamage, maxDamage + 1);

            character.health = Mathf.Max(0, character.health - damage);

            if (character.health <= 0)
            {
                MarkCharacterDead(character, report);

                return
                    $"{character.fullName} получил(а) смертельное ранение " +
                    $"во время вылазки в «{location.locationName}». " +
                    $"Награды не получены.";
            }

            injuryMessage =
                $" Получил(а) урон {damage} HP; осталось {character.health} HP.";
        }

        string rewards = GiveRewards(location);

        return
            $"{character.fullName} вернулся(лась) из локации " +
            $"«{location.locationName}».{injuryMessage} Награды: {rewards}.";
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
                if (IsCharacterOnExpedition(character))
                    continue;

                count++;
            }
        }

        return count;
    }

    private bool RollChance(int chancePercent)
    {
        int clampedChance = Mathf.Clamp(chancePercent, 0, 100);
        return UnityEngine.Random.Range(0, 100) < clampedChance;
    }

    private void MarkCharacterDead(
        GeneratedCharacter character,
        NightReportData report)
    {
        character.health = 0;
        character.isDead = true;

        if (report == null)
            return;

        if (report.deaths != null &&
            !report.deaths.Contains(character.fullName))
        {
            report.deaths.Add(character.fullName);
        }

        if (report.survivalChanges != null)
        {
            report.survivalChanges.Add(
                $"{character.fullName} погиб(ла) во время экспедиции.");
        }
    }

    private string GiveRewards(ExpeditionLocationData location)
    {
        if (location == null ||
            location.possibleRewards == null ||
            location.possibleRewards.Count == 0)
        {
            return "нет наград";
        }

        ResourceManager resources = ResourceManager.Instance;

        if (resources == null)
        {
            Debug.LogError("ResourceManager не найден: награды не выданы.");
            return "не удалось выдать награды";
        }

        StringBuilder result = new StringBuilder();

        foreach (ExpeditionRewardData reward in location.possibleRewards)
        {
            if (reward == null)
                continue;

            int minAmount = Mathf.Max(0, reward.minAmount);
            int maxAmount = Mathf.Max(minAmount, reward.maxAmount);
            
            int amount = UnityEngine.Random.Range(minAmount, maxAmount + 1);

            if (amount <= 0)
                continue;

            switch (reward.resourceType)
            {
                case ExpeditionRewardType.Food:
                    resources.AddFood(amount);
                    break;

                case ExpeditionRewardType.Water:
                    resources.AddWater(amount);
                    break;

                case ExpeditionRewardType.Fuel:
                    resources.AddFuel(amount);
                    break;
            }

            if (result.Length > 0)
                result.Append(", ");

            result.Append(GetRewardName(reward.resourceType));
            result.Append(" +");
            result.Append(amount);
        }

        return result.Length > 0 ? result.ToString() : "нет наград";
    }

    private void SetLocationCooldown(
        ExpeditionLocationData location,
        int availableOnDay)
    {
        LocationCooldown cooldown = FindCooldown(location);

        if (cooldown == null)
        {
            cooldown = new LocationCooldown
            {
                location = location
            };

            locationCooldowns.Add(cooldown);
        }

        cooldown.availableOnDay = availableOnDay;
    }

    private LocationCooldown FindCooldown(
        ExpeditionLocationData location)
    {
        if (location == null || locationCooldowns == null)
            return null;

        foreach (LocationCooldown cooldown in locationCooldowns)
        {
            if (cooldown != null && cooldown.location == location)
                return cooldown;
        }

        return null;
    }

    private void RemoveExpiredCooldowns(int currentDay)
    {
        if (locationCooldowns == null)
            return;

        for (int i = locationCooldowns.Count - 1; i >= 0; i--)
        {
            LocationCooldown cooldown = locationCooldowns[i];

            if (cooldown == null ||
                cooldown.location == null ||
                cooldown.availableOnDay <= currentDay)
            {
                locationCooldowns.RemoveAt(i);
            }
        }
    }

    private string GetRewardName(ExpeditionRewardType rewardType)
    {
        switch (rewardType)
        {
            case ExpeditionRewardType.Food:
                return "еда";

            case ExpeditionRewardType.Water:
                return "вода";

            case ExpeditionRewardType.Fuel:
                return "топливо";

            default:
                return "ресурс";
        }
    }

    private string FormatDays(int days)
    {
        int lastTwoDigits = days % 100;
        int lastDigit = days % 10;

        if (lastTwoDigits >= 11 && lastTwoDigits <= 14)
            return $"{days} дней";

        if (lastDigit == 1)
            return $"{days} день";

        if (lastDigit >= 2 && lastDigit <= 4)
            return $"{days} дня";

        return $"{days} дней";
    }
}