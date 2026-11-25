using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Центральная конфигурация выживания: день победы, штрафы морали за смерти,
/// штрафы голода/жажды по дням, урон от мороза и пороги температуры.
/// すべて значения настраиваются в инспекторе через SurvivalConfig.asset —
/// в коде больше нет магических чисел.
/// </summary>
[CreateAssetMenu(fileName = "SurvivalConfig", menuName = "Game/Survival Config")]
public class SurvivalConfig : ScriptableObject
{
    [Serializable]
    public class SurvivalPenaltyRow
    {
        [Tooltip("День голода/жажды, с которого применяется эта строка.")]
        [Min(0)] public int day = 0;

        [Tooltip("Штраф к морали за этот день.")]
        [Min(0)] public int moralePenalty = 0;

        [Tooltip("Урон здоровью за этот день.")]
        [Min(0)] public int healthDamage = 0;

        [Tooltip("Если true, персонаж помечается мёртвым, когда здоровье <= 0 после урона.")]
        public bool lethal = false;

        [Tooltip("Подпись для ночного отчёта.")]
        public string label = "";
    }

    [Header("Победа")]
    [Tooltip("На какой день завершается игра победой (отсчёт завершённых дней).")]
    [Min(1)] public int victoryDay = 10;

    [Header("Мораль")]
    [Tooltip("Базовый штраф к морали всех живущих в бункере за одну смерть.")]
    [Min(0)] public int baseMoralePenaltyPerDeath = 10;

    [Header("Голод — штрафы по дням без еды")]
    [Tooltip("Каждая строка — штраф за соответствующий день без еды. " +
             "Если точного совпадения нет, применяется самая поздняя подходящая строка " +
             "(действует каждый следующий день).")]
    public List<SurvivalPenaltyRow> hungerPenalties = new List<SurvivalPenaltyRow>(4)
    {
        new SurvivalPenaltyRow { day = 1, label = "Голод 1 день (без штрафа)" },
        new SurvivalPenaltyRow { day = 2, moralePenalty = 5, label = "мораль −5 (голод 2 дня)" },
        new SurvivalPenaltyRow { day = 3, moralePenalty = 10, healthDamage = 10, label = "Истощен! −10 HP, −10 мораль" },
        new SurvivalPenaltyRow { day = 4, moralePenalty = 15, healthDamage = 15, lethal = true, label = "Критический голод! −15 HP, −15 мораль" },
    };

    [Header("Жажда — штрафы по дням без воды")]
    public List<SurvivalPenaltyRow> thirstPenalties = new List<SurvivalPenaltyRow>(4)
    {
        new SurvivalPenaltyRow { day = 1, moralePenalty = 5, label = "мораль −5 (жажда 1 день)" },
        new SurvivalPenaltyRow { day = 2, healthDamage = 5, moralePenalty = 10, label = "Обезвоживание: −5 HP, −10 мораль" },
        new SurvivalPenaltyRow { day = 3, healthDamage = 15, moralePenalty = 15, label = "Сильное обезвоживание: −15 HP, −15 мораль" },
        new SurvivalPenaltyRow { day = 4, healthDamage = 20, moralePenalty = 15, lethal = true, label = "Критическое обезвоживание: −20 HP, −15 мораль" },
    };

    [Header("Мороз")]
    [Tooltip("Уровень температуры на этом значении и ниже — урон от мороза каждый день.")]
    [Min(0)] public int freezingTemperatureThreshold = 1;
    [Tooltip("Урон от мороза при температуре 0.")]
    [Min(0)] public int coldDamageAtZero = 30;
    [Tooltip("Урон от мороза при температуре 1..порог.")]
    [Min(0)] public int coldDamageLow = 15;
    [Tooltip("Штраф к морали от мороза.")]
    [Min(0)] public int coldMoralePenalty = 10;

    [Header("Экспедиции — диапазоны для валидации")]
    [Range(0, 100)] public int deathChancePercent = 5;
    [Tooltip("Минимальное число дней на восстановление локации после вылазки.")]
    [Min(1)] public int minCooldownDays = 2;
    [Tooltip("Максимальное число дней на восстановление локации после вылазки.")]
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
