using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Trait", menuName = "Scriptable Objects/Trait")]
public class Trait : ScriptableObject
{
    public enum TraitCategory
    {
        Positive,
        Negative
    }

    public enum EffectType
    {
        FoodConsumptionReductionPercent,

        PlumbingBreakChanceChangePoints,
        ElectricityBreakChanceChangePoints,

        NoInjuryOnGeneratorRepair,

        RestHealthRecoveryIncreasePercent,
        RestMoraleRecoveryIncreasePercent,

        ExpeditionSuccessChanceChangePoints,
        CannotGoOnExpeditionToLocation,

        WaterRepairInjuryChanceChangePoints,
        ElectricityRepairInjuryChanceChangePoints,
        WaterRepairInjuryChanceReductionPercent,
        ElectricityRepairInjuryChanceReductionPercent,

        CannotRepairWater,
        CannotRepairElectricity,

        InjuryChanceChangePoints,

        CannotBeSpiedOn,

        AvoidNegativeChoices
    }

    [System.Serializable]
    public class TraitEffect
    {
        public EffectType effectType;

        [Tooltip("Значение эффекта. Например, 25 или -10. Для эффектов без числа не используется.")]
        public float value;

        [Tooltip("Используется только для CannotGoOnExpeditionToLocation. Нумерация локаций начинается с 1.")]
        public int locationNumber = 3;
    }

    [Header("Описание")]
    public string traitName;

    [TextArea(2, 5)]
    public string traitDesc;

    public Sprite traitSprite;

    [Header("Категория")]
    public TraitCategory category;

    [Header("Эффекты")]
    public List<TraitEffect> effects = new List<TraitEffect>();
}