using UnityEngine;

[CreateAssetMenu(menuName = "ColdCoffin/Expedition Location", fileName = "ExpeditionLocation")]
public class ExpeditionLocation : ScriptableObject
{
    public string id;
    public string displayName;
    [TextArea] public string description;
    public int riskLevel; // 0 low, 1 mid, 2 high
    public int rewardFood;
    public int rewardWater;
    public int rewardFuel;
    public string[] recommendedProfessions;
}