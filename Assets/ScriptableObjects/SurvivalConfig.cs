using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Главный конфиг выживания: тайминги, стартовые ресурсы, штрафы морали/здоровья, 
/// параметры генератора и вылазок. Настраивается через SurvivalConfig.asset.
/// </summary>
[CreateAssetMenu(fileName = "SurvivalConfig", menuName = "Game/Survival Config")]
public class SurvivalConfig : ScriptableObject
{
    [Serializable]
    public class SurvivalPenaltyRow
    {
        [Tooltip("С какого дня голода/жажды включается этот штраф.")]
        [Min(0)] public int day = 0;

        [Tooltip("Сколько морали отнимать в день.")]
        [Min(0)] public int moralePenalty = 0;

        [Tooltip("Сколько HP отнимать в день.")]
        [Min(0)] public int healthDamage = 0;

        [Tooltip("Может ли добить персонажа, если HP упадёт до 0.")]
        public bool lethal = false;

        [Tooltip("Текст записи в утреннем отчёте.")]
        public string label = "";
    }

    [Header("Условия победы")]
    [Tooltip("Номер дня, после завершения которого засчитывается победа.")]
    [Min(1)] public int victoryDay = 10;

    [Header("Мораль")]
    [Tooltip("Штраф к морали всех в бункере за смерть одного товарища.")]
    [Min(0)] public int baseMoralePenaltyPerDeath = 10;

    [Header("Стартовые ресурсы (День 1)")]
    [Tooltip("Минимум банок еды на старте.")]
    [Min(0)] public int startFoodMin = 8;
    [Tooltip("Максимум банок еды на старте.")]
    [Min(0)] public int startFoodMax = 10;

    [Tooltip("Минимум бутылок воды на старте.")]
    [Min(0)] public int startWaterMin = 8;
    [Tooltip("Максимум бутылок воды на старте.")]
    [Min(0)] public int startWaterMax = 10;

    [Tooltip("Минимум канистр топлива на старте.")]
    [Min(0)] public int startFuelMin = 4;
    [Tooltip("Максимум канистр топлива на старте.")]
    [Min(0)] public int startFuelMax = 6;

    [Header("Генератор: поломки")]
    [Tooltip("Базовый шанс поломки генератора за ночь (%).")]
    [Range(0, 100)] public int baseBreakdownChancePercent = 15;
    [Tooltip("Шанс поломки, если в бункере остался всего 1 живой человек.")]
    [Range(0, 100)] public int breakdownChanceForOnePerson = 60;
    [Tooltip("На сколько % снижается шанс поломки за каждого дополнительного человека в бункере.")]
    [Range(0, 100)] public int breakdownReductionPerAdditionalPerson = 10;

    [Header("Голод")]
    [Tooltip("Штрафы по дням без еды. Если текущего дня нет в списке, берётся последняя подходящая строка.")]
    public List<SurvivalPenaltyRow> hungerPenalties = new List<SurvivalPenaltyRow>(4)
    {
        new SurvivalPenaltyRow { day = 1, label = "Голод 1 день (без штрафа)" },
        new SurvivalPenaltyRow { day = 2, moralePenalty = 5, label = "мораль −5 (голод 2 дня)" },
        new SurvivalPenaltyRow { day = 3, moralePenalty = 10, healthDamage = 10, label = "Истощен! −10 HP, −10 мораль" },
        new SurvivalPenaltyRow { day = 4, moralePenalty = 15, healthDamage = 15, lethal = true, label = "Критический голод! −15 HP, −15 мораль" },
    };

    [Header("Жажда")]
    [Tooltip("Штрафы по дням без воды. Если текущего дня нет в списке, берётся последняя подходящая строка.")]
    public List<SurvivalPenaltyRow> thirstPenalties = new List<SurvivalPenaltyRow>(4)
    {
        new SurvivalPenaltyRow { day = 1, moralePenalty = 5, label = "мораль −5 (жажда 1 день)" },
        new SurvivalPenaltyRow { day = 2, healthDamage = 5, moralePenalty = 10, label = "Обезвоживание: −5 HP, −10 мораль" },
        new SurvivalPenaltyRow { day = 3, healthDamage = 15, moralePenalty = 15, label = "Сильное обезвоживание: −15 HP, −15 мораль" },
        new SurvivalPenaltyRow { day = 4, healthDamage = 20, moralePenalty = 15, lethal = true, label = "Критическое обезвоживание: −20 HP, −15 мораль" },
    };

    [Header("Холод")]
    [Tooltip("Порог замерзания: при этой температуре и ниже люди получают урон каждый день.")]
    [Min(0)] public int freezingTemperatureThreshold = 1;
    [Tooltip("Урон здоровью, если температура упала до 0.")]
    [Min(0)] public int coldDamageAtZero = 30;
    [Tooltip("Урон здоровью при температуре от 1 до порога замерзания.")]
    [Min(0)] public int coldDamageLow = 15;
    [Tooltip("Ежедневный штраф к морали от холода.")]
    [Min(0)] public int coldMoralePenalty = 10;

    [Header("Генератор: ремонт")]
    [Tooltip("Базовый шанс починить генератор (%).")]
    [Range(0, 100)] public int baseRepairSuccessChancePercent = 60;
    [Tooltip("Шанс получить травму при неудачном ремонте (%).")]
    [Range(0, 100)] public int baseRepairInjuryChancePercent = 25;
    [Tooltip("Сколько HP теряет ремонтник при травме.")]
    [Min(0)] public int repairFailureDamageHP = 15;
    [Tooltip("Бонус к шансу починки за каждого свободного человека в бункере (%).")]
    [Range(0, 100)] public int repairSuccessBonusPerAdditionalPerson = 10;

    [Header("Экспедиции: штраф за нехватку людей")]
    [Tooltip("Норма людей в бункере. Если их меньше — экспедиции становятся опаснее.")]
    [Min(1)] public int referencePopulation = 5;
    [Tooltip("+% к шансу гибели за каждого нехватающего человека (ниже нормы).")]
    [Range(0, 100)] public int expeditionDeathChanceIncreasePerMissingPerson = 5;
    [Tooltip("+% к шансу травмы за каждого нехватающего человека (ниже нормы).")]
    [Range(0, 100)] public int expeditionInjuryChanceIncreasePerMissingPerson = 5;

    [Header("Экспедиции: базовые параметры")]
    [Tooltip("Базовый шанс гибели на вылазке (%).")]
    [Range(0, 100)] public int deathChancePercent = 5;
    [Tooltip("Минимум дней на восстановление локации.")]
    [Min(1)] public int minCooldownDays = 2;
    [Tooltip("Максимум дней на восстановление локации.")]
    [Min(1)] public int maxCooldownDays = 4;

    [NonSerialized] private bool _validationLogged;

    private void OnValidate() => Validate();
    private void OnEnable() => Validate();

    /// <summary>
    /// Проверяет все диапазоны при загрузке/изменении SO. never throws —
    /// некорректные значения только приводятся к допустимым с предупреждением в лог.
    /// </summary>
    public bool Validate(bool forceLog = false)
    {
        bool valid = true;

        valid &= Clamp(ref victoryDay, 1, int.MaxValue, nameof(victoryDay),
            "день победы должен быть >= 1");
        valid &= Clamp(ref baseMoralePenaltyPerDeath, 0, 100, nameof(baseMoralePenaltyPerDeath),
            "штраф морали за смерть должен быть 0..100");
        valid &= ClampPercent(ref deathChancePercent, nameof(deathChancePercent));
        valid &= ValidateCooldowns();
        valid &= ValidatePenaltyRows(hungerPenalties, nameof(hungerPenalties));
        valid &= ValidatePenaltyRows(thirstPenalties, nameof(thirstPenalties));
        valid &= Clamp(ref freezingTemperatureThreshold, 0, int.MaxValue, nameof(freezingTemperatureThreshold),
            "порог температуры должен быть >= 0");
        valid &= Clamp(ref coldDamageAtZero, 0, int.MaxValue, nameof(coldDamageAtZero),
            "урон от мороза должен быть >= 0");
        valid &= Clamp(ref coldDamageLow, 0, int.MaxValue, nameof(coldDamageLow),
            "урон от мороза должен быть >= 0");
        valid &= Clamp(ref coldMoralePenalty, 0, 100, nameof(coldMoralePenalty),
            "штраф морали от мороза должен быть 0..100");

        valid &= ClampPercent(ref baseBreakdownChancePercent, nameof(baseBreakdownChancePercent));
        valid &= ClampPercent(ref breakdownChanceForOnePerson, nameof(breakdownChanceForOnePerson));
        valid &= ClampPercent(ref breakdownReductionPerAdditionalPerson, nameof(breakdownReductionPerAdditionalPerson));

        valid &= ClampPercent(ref baseRepairSuccessChancePercent, nameof(baseRepairSuccessChancePercent));
        valid &= ClampPercent(ref baseRepairInjuryChancePercent, nameof(baseRepairInjuryChancePercent));
        valid &= Clamp(ref repairFailureDamageHP, 0, 100, nameof(repairFailureDamageHP), "0..100");
        valid &= ClampPercent(ref repairSuccessBonusPerAdditionalPerson, nameof(repairSuccessBonusPerAdditionalPerson));

        // ClampRange logic: ensure min <= max and min >= 0
        if (startFoodMin < 0) { startFoodMin = 0; valid = false; }
        if (startFoodMax < startFoodMin) { startFoodMax = startFoodMin; valid = false; }
        if (startWaterMin < 0) { startWaterMin = 0; valid = false; }
        if (startWaterMax < startWaterMin) { startWaterMax = startWaterMin; valid = false; }
        if (startFuelMin < 0) { startFuelMin = 0; valid = false; }
        if (startFuelMax < startFuelMin) { startFuelMax = startFuelMin; valid = false; }

        valid &= Clamp(ref referencePopulation, 1, int.MaxValue, nameof(referencePopulation), ">= 1");
        valid &= ClampPercent(ref expeditionDeathChanceIncreasePerMissingPerson, nameof(expeditionDeathChanceIncreasePerMissingPerson));
        valid &= ClampPercent(ref expeditionInjuryChanceIncreasePerMissingPerson, nameof(expeditionInjuryChanceIncreasePerMissingPerson));

        if (!valid || forceLog)
        {
            _validationLogged = true;
            if (!valid)
                Debug.LogWarning(
                    $"[SurvivalConfig] {name}: найдены некорректные значения, приведены к допустимым. " +
                    "См. предупреждения выше.", this);
            else
                Debug.Log(
                    $"[SurvivalConfig] {name}: все значения в допустимых диапазонах.", this);
        }

        return valid;
    }

    private bool Clamp(
        ref int value,
        int min,
        int max,
        string fieldName,
        string reason)
    {
        if (value < min || value > max)
        {
            int before = value;
            value = Mathf.Clamp(value, min, max);
            Debug.LogWarning(
                $"[SurvivalConfig] {name}: {fieldName} = {before} вне допустимого диапазона ({reason}). " +
                $"Установлено {value}.", this);
            return false;
        }
        return true;
    }

    private bool ClampPercent(ref int value, string fieldName)
    {
        return Clamp(ref value, 0, 100, fieldName, "значение должно быть в диапазоне 0..100");
    }

    private bool ValidateCooldowns()
    {
        bool valid = true;

        if (minCooldownDays < 1)
        {
            int before = minCooldownDays;
            minCooldownDays = 1;
            Debug.LogWarning(
                $"[SurvivalConfig] {name}: minCooldownDays = {before} < 1. Установлено 1.", this);
            valid = false;
        }

        if (maxCooldownDays < 1)
        {
            int before = maxCooldownDays;
            maxCooldownDays = 1;
            Debug.LogWarning(
                $"[SurvivalConfig] {name}: maxCooldownDays = {before} < 1. Установлено 1.", this);
            valid = false;
        }

        if (minCooldownDays > maxCooldownDays)
        {
            int before = minCooldownDays;
            minCooldownDays = maxCooldownDays;
            Debug.LogWarning(
                $"[SurvivalConfig] {name}: minCooldownDays ({before}) > maxCooldownDays ({maxCooldownDays}). " +
                $"minCooldownDays установлен равным maxCooldownDays.", this);
            valid = false;
        }

        return valid;
    }

    private bool ValidatePenaltyRows(List<SurvivalPenaltyRow> rows, string fieldName)
    {
        if (rows == null)
        {
            Debug.LogWarning(
                $"[SurvivalConfig] {name}: {fieldName} равен null — штрафы по этому фактору не будут применяться.", this);
            return false;
        }

        bool valid = true;
        foreach (var row in rows)
        {
            if (row == null) continue;

            if (row.day < 0)
            {
                row.day = 0;
                valid = false;
                Debug.LogWarning(
                    $"[SurvivalConfig] {name}: {fieldName} — день строки < 0. Установлено 0.", this);
            }

            if (row.moralePenalty < 0)
            {
                row.moralePenalty = 0;
                valid = false;
                Debug.LogWarning(
                    $"[SurvivalConfig] {name}: {fieldName} — штраф морали < 0. Установлено 0.", this);
            }

            if (row.healthDamage < 0)
            {
                row.healthDamage = 0;
                valid = false;
                Debug.LogWarning(
                    $"[SurvivalConfig] {name}: {fieldName} — урон здоровью < 0. Установлено 0.", this);
            }
        }

        return valid;
    }
}