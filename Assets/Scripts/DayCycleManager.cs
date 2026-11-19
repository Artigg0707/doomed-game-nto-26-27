using System;
using System.Collections.Generic;
using UnityEngine;

public class DayCycleManager : MonoBehaviour
{
    public static DayCycleManager Instance { get; private set; }

    public int CurrentDay { get; private set; } = 1;
    public event Action<int> OnDayChanged;
    public event Action<NightReportData> OnNightComplete;

    [SerializeField] private CharacterManager characterManager;

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

        OnDayChanged?.Invoke(CurrentDay);
        OnNightComplete?.Invoke(report);

        Debug.Log($"Started next day: {CurrentDay}");
    }

    private void UpdateSurvivalCounters(ResourceManager resources, CharacterManager cm, NightReportData report)
    {
        foreach (GeneratedCharacter character in cm.characters)
        {
            if (character == null || character.isDead || character.isExiled)
                continue;

            // Еда
            if (character.receivesFoodToday)
                character.daysWithoutFood = 0;
            else
            {
                character.daysWithoutFood++;
                if (character.daysWithoutFood == 1)
                {
                    character.morale -= 10;
                    report.survivalChanges.Add($"{character.fullName}: Голоден, мораль −10.");
                }
                else if (character.daysWithoutFood >= 2)
                {
                    character.health -= 25;
                    character.morale -= 20;
                    report.survivalChanges.Add($"{character.fullName}: Истощен, −25 HP, −20 мораль.");
                    if (character.health <= 0)
                    {
                        character.isDead = true;
                        report.deaths.Add(character.fullName);
                    }
                }
            }

            // Вода
            if (character.receivesWaterToday)
                character.daysWithoutWater = 0;
            else
            {
                character.daysWithoutWater++;
                if (character.daysWithoutWater == 1)
                {
                    character.morale -= 15;
                    report.survivalChanges.Add($"{character.fullName}: Жажда, мораль −15.");
                }
                else if (character.daysWithoutWater >= 2)
                {
                    character.health -= 40;
                    report.survivalChanges.Add($"{character.fullName}: Обезвоживание, −40 HP.");
                    if (character.health <= 0)
                    {
                        character.isDead = true;
                        report.deaths.Add(character.fullName);
                    }
                }
            }

            // Холод
            if (!character.isDead && resources.TemperatureLevel <= 1)
            {
                int coldDamage = resources.TemperatureLevel == 0 ? 35 : 15;
                character.health -= coldDamage;
                report.survivalChanges.Add($"{character.fullName}: Мороз, −{coldDamage} HP.");
                if (character.health <= 0)
                {
                    character.isDead = true;
                    report.deaths.Add(character.fullName);
                }
            }
        }

        // Ресурсы для отчёта
        if (report.fuelConsumed)
            report.resourceChanges.Add("Топлива −1 канистра (обогрев работал).");
        else
            report.resourceChanges.Add("Топлива не хватило. Обогрев отключён.");

        report.resourceChanges.Add($"Еда в складе: {resources.FoodCans} банок.");
        report.resourceChanges.Add($"Вода на складе: {resources.WaterBottles} бутылок.");
        report.resourceChanges.Add($"Температура: {resources.TemperatureLevel}/3.");
    }
}
