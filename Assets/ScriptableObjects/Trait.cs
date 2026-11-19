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

        [Tooltip("�������� �������. ��������, 25 ��� -10. ��� �������� ��� ����� �� ������������.")]
        public float value;

        [Tooltip("������������ ������ ��� CannotGoOnExpeditionToLocation. ��������� ������� ���������� � 1.")]
        public int locationNumber = 3;
    }

    [Header("��������")]
    public string traitName;

    [TextArea(2, 5)]
    public string traitDesc;

    public Sprite traitSprite;

    [Header("���������")]
    public TraitCategory category;

    [Header("�������")]
    public List<TraitEffect> effects = new List<TraitEffect>();
}