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
    [Tooltip("Случайное количество игровых дней до повторной доступности локации.")]
    [Min(1)]
    public int minCooldownDays = 2;

    [Min(1)]
    public int maxCooldownDays = 4;

    [Header("Possible Rewards")]
    public List<ExpeditionRewardData> possibleRewards =
        new List<ExpeditionRewardData>();
}