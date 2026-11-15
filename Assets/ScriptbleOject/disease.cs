using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "new_disease", menuName = "Scriptable Objects/disease")]
public class disease : ScriptableObject
{
    [System.Serializable]
    public class StatEffect
    {
        public Stati targetStat;
        public float damagePerDay;
        
    }
    
    [System.Serializable]
    public class CureRequirement
    {
        public string description;
        
        //   дописать требования для лечения
        //
        //
        
    }
    
    public enum deathType
    {
        Natural,
        Suicide,
        Freezing
        // дописать ттипы сметрей
    }
    
    public string diseaseName;

    [TextArea(2, 4)]
    public string symptoms;

    public Sprite icon;

    [Header("чё оно делает")]
    public List<StatEffect> statEffects;

    [Header("чем лечить")]
    public List<CureRequirement> cureRequirements;

    [Header("смэерт")]
    [Range(0f, 1f)]
    public float baseDeathChance;

    [Range(0f, 0.1f)]
    public float deathChanceGrowth;

    [Range(0f, 1f)]
    public float maxDeathChance;

    public deathType DeathType;

}