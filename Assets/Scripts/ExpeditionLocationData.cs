using System;
using System.Collections.Generic;
using UnityEngine;

public enum ExpeditionRewardType
{
    Food,
    Water,
    Fuel
}

[Serializable]
public class ExpeditionRewardData
{
    public ExpeditionRewardType resourceType;

    [Min(0)] public int minAmount = 1;
    [Min(0)] public int maxAmount = 1;
}

[CreateAssetMenu(
    fileName = "NewExpeditionLocation",
    menuName = "Game/Expeditions/Location")]
public class ExpeditionLocationData : ScriptableObject
{
    [Header("Basic Information")]
    public string locationId;
    public string locationName;
    public Sprite locationImage;

    [TextArea(3, 6)]
    public string description;

    [TextArea(2, 4)]
    public string riskDescription;

    [Min(1)]
    public int durationDays = 1;

    [Header("Risk")]
    [Range(0, 100)]
    public int injuryChancePercent = 25;

    [Range(0, 100)]
    public int deathChancePercent = 5;

    [Min(0)]
    public int minInjuryDamage = 10;

    [Min(0)]
    public int maxInjuryDamage = 25;

    [Header("Location Cooldown")]
    [Tooltip("—лучайное количество игровых дней до повторной доступности локации.")]
    [Min(1)]
    public int minCooldownDays = 2;

    [Min(1)]
    public int maxCooldownDays = 4;

    [Header("Possible Rewards")]
    public List<ExpeditionRewardData> possibleRewards =
        new List<ExpeditionRewardData>();

    private void OnValidate() => Validate();

    public bool Validate()
    {
        bool valid = true;

        valid &= ClampPercent(ref deathChancePercent, nameof(deathChancePercent),
            "шанс смерти должен быть 0..100");
        valid &= ClampPercent(ref injuryChancePercent, nameof(injuryChancePercent),
            "шанс ранени€ должен быть 0..100");

        if (minCooldownDays < 1)
        {
            int before = minCooldownDays;
            minCooldownDays = 1;
            Debug.LogWarning(
                $"[ExpeditionLocationData] {name}: minCooldownDays = {before} < 1. ”становлено 1.", this);
            valid = false;
        }

        if (maxCooldownDays < 1)
        {
            int before = maxCooldownDays;
            maxCooldownDays = 1;
            Debug.LogWarning(
                $"[ExpeditionLocationData] {name}: maxCooldownDays = {before} < 1. ”становлено 1.", this);
            valid = false;
        }

        if (minCooldownDays > maxCooldownDays)
        {
            int before = minCooldownDays;
            minCooldownDays = maxCooldownDays;
            Debug.LogWarning(
                $"[ExpeditionLocationData] {name}: minCooldownDays ({before}) > maxCooldownDays ({maxCooldownDays}). " +
                $"minCooldownDays установлен равным maxCooldownDays.", this);
            valid = false;
        }

        if (minInjuryDamage > maxInjuryDamage)
        {
            int before = maxInjuryDamage;
            maxInjuryDamage = minInjuryDamage;
            Debug.LogWarning(
                $"[ExpeditionLocationData] {name}: minInjuryDamage ({minInjuryDamage}) > maxInjuryDamage ({before}). " +
                $"maxInjuryDamage установлен равным minInjuryDamage.", this);
            valid = false;
        }

        if (durationDays < 1)
        {
            int before = durationDays;
            durationDays = 1;
            Debug.LogWarning(
                $"[ExpeditionLocationData] {name}: durationDays = {before} < 1. ”становлено 1.", this);
            valid = false;
        }

        return valid;
    }

    private bool ClampPercent(
        ref int value,
        string fieldName,
        string reason)
    {
        if (value < 0 || value > 100)
        {
            int before = value;
            value = Mathf.Clamp(value, 0, 100);
            Debug.LogWarning(
                $"[ExpeditionLocationData] {name}: {fieldName} = {before} вне диапазона 0..100 ({reason}). " +
                $"”становлено {value}.", this);
            return false;
        }
        return true;
    }
}