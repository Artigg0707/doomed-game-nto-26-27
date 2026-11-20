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

    [Tooltip("”никальный посто€нный ID, например abandoned_shop")]
    public string locationId;

    public string locationName;
    public Sprite locationImage;

    [TextArea(3, 6)]
    public string description;

    [TextArea(2, 4)]
    public string riskDescription;

    [Min(1)]
    public int durationDays = 1;

    [Header("Possible Rewards")]
    public List<ExpeditionRewardData> possibleRewards =
        new List<ExpeditionRewardData>();
}