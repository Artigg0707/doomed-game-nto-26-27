using System;
using System.Collections.Generic;
using UnityEngine;

public class DayCycleManager : MonoBehaviour
{
    private const int MoralePenaltyPerDeath = 10;

    public static DayCycleManager Instance { get; private set; }

    public int CurrentDay { get; private set; } = 1;
    public event Action<int> OnDayChanged;
    public event Action<NightReportData> OnNightComplete;

    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private ExpeditionManager expeditionManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ResourceManager resources = ResourceManager.Instance;
        if (resources != null && resources.resourceData != null)
        {
            if (resources.resourceData.currentDay > 0)
                CurrentDay = resources.resourceData.currentDay;
            else
                resources.resourceData.currentDay = 1;
        }
    }

    public void StartNextDay()
    {
        ResourceManager resources = ResourceManager.Instance;

        if (resources == null)
        {
            Debug.LogError("ResourceManager is not found.", this);
            return;
        }

        // Состояние "до" для отчёта
        int tempBefore = resources.TemperatureLevel;
        bool generatorWasBroken = resources.GeneratorBroken;

        // 1. Потратить топливо на обогрев
        bool fuelConsumed = resources.ConsumeFuel(1);
        if (!fuelConsumed)
        {
            resources.SetTemperature(resources.TemperatureLevel - 1);
        }

        // 2. Формируем отчёт ночи
        NightReportData report = new NightReportData
        {
            completedDay = CurrentDay,
            newDay = CurrentDay + 1,
            fuelConsumed = fuelConsumed,
            generatorWorked = !generatorWasBroken && fuelConsumed,
            temperatureBefore = tempBefore,
            temperatureAfter = resources.TemperatureLevel
        };

        // 3. Выживание персонажей
        if (characterManager != null)
        {
            UpdateSurvivalCounters(resources, characterManager, report);
            characterManager.ResetDailyRations();
        }
        else
        {
            Debug.LogError("CharacterManager is not assigned in DayCycleManager.", this);
        }

        // 4. Переход к следующему дню
        CurrentDay++;
        resources.resourceData.currentDay = CurrentDay;
        report.newDay = CurrentDay;

        if (expeditionManager == null)
            expeditionManager = ExpeditionManager.Instance;
        if (expeditionManager != null)
            expeditionManager.ResolveReturns(CurrentDay, report);

        OnDayChanged?.Invoke(CurrentDay);
        OnNightComplete?.Invoke(report);

        Debug.Log($"Started next day: {CurrentDay}");
    }

    private void UpdateSurvivalCounters(ResourceManager resources, CharacterManager cm, NightReportData report)
    {
        int foodSpent = 0;
        int waterSpent = 0;

        foreach (GeneratedCharacter character in cm.characters)
        {
            if (character == null || character.isDead || character.isExiled)
                continue;
            if (character.status == CharacterStatus.OnExpedition)
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

            // ---------- Вода ----------
            if (character.receivesWaterToday)
            {
                character.daysWithoutWater = 0;
                waterSpent++;
            }
            else
            {
                character.daysWithoutWater++;
            }

            // ---------- Статус жителя (всегда добавляем в отчёт) ----------
            string status = GetCharacterStatus(character);
            report.survivalChanges.Add(status);

            // ---------- Штрафы за голод/жажду/холод ----------
            // Голод
            if (character.daysWithoutFood == 1)
            {
                character.morale -= 10;
                report.survivalChanges.Add($"  ↳ мораль −10 (голод 1 день)");
            }
            else if (character.daysWithoutFood >= 2)
            {
                character.health -= 25;
                character.morale -= 20;
                report.survivalChanges.Add($"  ↳ Истощен! −25 HP, −20 мораль.");
                if (character.health <= 0)
                {
                    character.isDead = true;
                    report.deaths.Add(character.fullName);
                }
            }

            if (character.isDead)
                continue;

            // Жажда
            if (character.daysWithoutWater == 1)
            {
                character.morale -= 15;
                report.survivalChanges.Add($"  ↳ мораль −15 (жажда 1 день)");
            }
            else if (character.daysWithoutWater >= 2)
            {
                character.health -= 40;
                report.survivalChanges.Add($"  ↳ Обезвоживание! −40 HP.");
                if (character.health <= 0)
                {
                    character.isDead = true;
                    report.deaths.Add(character.fullName);
                }
            }

            // Холод
            if (!character.isDead && resources.TemperatureLevel <= 1)
            {
                int coldDamage = resources.TemperatureLevel == 0 ? 35 : 15;
                character.health -= coldDamage;
                report.survivalChanges.Add($"  ↳ Мороз, −{coldDamage} HP.");
                if (character.health <= 0)
                {
                    character.isDead = true;
                    report.deaths.Add(character.fullName);
                }
            }
        }

        ApplyDeathMoralePenalty(cm, report);
        ProcessMoraleDepartures(cm, report);

        // Ресурсы для отчёта
        if (report.fuelConsumed)
            report.resourceChanges.Add("Топлива −1 канистра (обогрев работал).");
        else
            report.resourceChanges.Add("Топлива не хватило. Обогрев отключён.");

        if (foodSpent > 0)
            report.resourceChanges.Add($"Еды израсходовано: {foodSpent} банок.");
        if (waterSpent > 0)
            report.resourceChanges.Add($"Воды израсходовано: {waterSpent} бутылок.");

        report.resourceChanges.Add($"Еда на складе: {resources.FoodCans} банок.");
        report.resourceChanges.Add($"Вода на складе: {resources.WaterBottles} бутылок.");
        report.resourceChanges.Add($"Температура: {resources.TemperatureLevel}/3.");
    }

    private void ApplyDeathMoralePenalty(CharacterManager cm, NightReportData report)
    {
        if (report.deaths.Count == 0)
            return;

        foreach (GeneratedCharacter character in cm.characters)
        {
            if (character == null || character.isDead || character.isExiled)
                continue;

            character.morale -= MoralePenaltyPerDeath * report.deaths.Count;
            report.survivalChanges.Add(
                $"{character.fullName}: мораль −{MoralePenaltyPerDeath * report.deaths.Count} из-за смерти жителей.");
        }
    }

    private void ProcessMoraleDepartures(CharacterManager cm, NightReportData report)
    {
        foreach (GeneratedCharacter character in cm.characters)
        {
            if (character == null || character.isDead || character.isExiled || character.morale >= 0)
                continue;

            character.isExiled = true;
            report.departures.Add(character.fullName);
            report.survivalChanges.Add(
                $"{character.fullName} ушёл из бункера: мораль опустилась ниже нуля.");
        }
    }

    private string GetCharacterStatus(GeneratedCharacter c)
    {
        string foodState = c.daysWithoutFood == 0 ? "сыт" : $"без еды {c.daysWithoutFood} дн.";
        string waterState = c.daysWithoutWater == 0 ? "напоен" : $"без воды {c.daysWithoutWater} дн.";
        string healthState = c.health >= 75 ? "здоров" : (c.health >= 35 ? "ранен" : "при смерти");
        return $"{c.fullName}: {foodState}, {waterState}, {c.health} HP, мораль {c.morale} ({healthState}).";
    }
}
