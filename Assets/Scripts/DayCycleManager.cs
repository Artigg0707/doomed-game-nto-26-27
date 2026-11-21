using System;
using UnityEngine;

public class DayCycleManager : MonoBehaviour
{
    private const int MoralePenaltyPerDeath = 10;

    public static DayCycleManager Instance { get; private set; }

    [SerializeField] private ExpeditionManager expeditionManager;
    [SerializeField] private CharacterManager characterManager;

    public int CurrentDay { get; private set; } = 1;

    public event Action<int> OnDayChanged;
    public event Action<NightReportData> OnNightComplete;
    public event Action OnDefeat;

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

        ResourceManager resources = ResourceManager.Instance;

        if (resources != null && resources.resourceData != null)
        {
            if (resources.resourceData.currentDay > 0)
                CurrentDay = resources.resourceData.currentDay;
            else
                resources.resourceData.currentDay = 1;
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

        // Состояние до обработки ночи.
        int temperatureBefore = resources.TemperatureLevel;
        bool generatorWasBroken = resources.GeneratorBroken;

        // Тратим топливо на обогрев.
        bool fuelConsumed = resources.ConsumeFuel(1);

        if (!fuelConsumed)
            resources.SetTemperature(resources.TemperatureLevel - 1);

        // Создаём отчёт за завершаемый день.
        NightReportData report = new NightReportData
        {
            completedDay = CurrentDay,
            newDay = CurrentDay + 1,
            fuelConsumed = fuelConsumed,
            generatorWorked = !generatorWasBroken && fuelConsumed,
            temperatureBefore = temperatureBefore,
            temperatureAfter = resources.TemperatureLevel
        };

        // Проверяем выживание жителей, которые находятся в бункере.
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

        // Переходим на новый день.
        CurrentDay++;

        if (resources.resourceData != null)
            resources.resourceData.currentDay = CurrentDay;

        report.newDay = CurrentDay;

        // Обрабатываем вылазки после увеличения CurrentDay.
        // Отчёт ещё не отправлен в OnNightComplete, поэтому результаты
        // экспедиций попадут в этот же отчёт.
        if (expeditionManager != null)
            expeditionManager.ProcessCompletedDay(report);

        // Применяем последствия смертей и проверяем уход жителей.
        // Персонажи, которые всё ещё находятся на вылазке, пропускаются.
        if (characterManager != null)
        {
            ApplyDeathMoralePenalty(characterManager, report);
            ProcessMoraleDepartures(characterManager, report);
        }

        OnDayChanged?.Invoke(CurrentDay);
        OnNightComplete?.Invoke(report);

        if (IsDefeat())
            OnDefeat?.Invoke();

        Debug.Log($"Started next day: {CurrentDay}");
    }

    public bool IsDefeat()
    {
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

                // Отправленные персонажи не проходят ночную проверку бункера.
                if (IsCharacterOnExpedition(character))
                    continue;

                // Еда.
                if (character.receivesFoodToday)
                {
                    character.daysWithoutFood = 0;
                    foodSpent++;
                }
                else
                {
                    character.daysWithoutFood++;
                }

                // Вода.
                if (character.receivesWaterToday)
                {
                    character.daysWithoutWater = 0;
                    waterSpent++;
                }
                else
                {
                    character.daysWithoutWater++;
                }

                // Статус жителя.
                report.survivalChanges.Add(GetCharacterStatus(character));

                // Голод.
                if (character.daysWithoutFood == 1)
                {
                    character.morale -= 10;
                    report.survivalChanges.Add(
                        "  ↳ мораль −10 (голод 1 день)");
                }
                else if (character.daysWithoutFood >= 2)
                {
                    character.health -= 25;
                    character.morale -= 20;

                    report.survivalChanges.Add(
                        "  ↳ Истощен! −25 HP, −20 мораль.");

                    if (character.health <= 0)
                    {
                        character.isDead = true;
                        report.deaths.Add(character.fullName);
                    }
                }

                if (character.isDead)
                    continue;

                // Жажда.
                if (character.daysWithoutWater == 1)
                {
                    character.morale -= 15;
                    report.survivalChanges.Add(
                        "  ↳ мораль −15 (жажда 1 день)");
                }
                else if (character.daysWithoutWater >= 2)
                {
                    character.health -= 40;

                    report.survivalChanges.Add(
                        "  ↳ Обезвоживание! −40 HP.");

                    if (character.health <= 0)
                    {
                        character.isDead = true;
                        report.deaths.Add(character.fullName);
                    }
                }

                // Холод.
                if (!character.isDead && resources.TemperatureLevel <= 1)
                {
                    int coldDamage =
                        resources.TemperatureLevel == 0 ? 35 : 15;

                    character.health -= coldDamage;

                    report.survivalChanges.Add(
                        $"  ↳ Мороз, −{coldDamage} HP.");

                    if (character.health <= 0)
                    {
                        character.isDead = true;
                        report.deaths.Add(character.fullName);
                    }
                }
            }
        }

        // Ресурсы для отчёта.
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

        foreach (GeneratedCharacter character in cm.characters)
        {
            if (character == null || character.isDead || character.isExiled)
                continue;

            if (IsCharacterOnExpedition(character))
                continue;

            int penalty = MoralePenaltyPerDeath * report.deaths.Count;
            character.morale -= penalty;

            report.survivalChanges.Add(
                $"{character.fullName}: мораль −{penalty} " +
                "из-за смерти жителей.");
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
}
