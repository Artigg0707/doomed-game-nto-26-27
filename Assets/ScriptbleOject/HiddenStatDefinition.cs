using UnityEngine;

[CreateAssetMenu(fileName = "HiddenStatDefinition", menuName = "Scriptable Objects/Новый скрытый параметр")]
public class HiddenStatDefinition : ScriptableObject
{
    public string statName;
    public string statDesc;
    
    
    public float minValue;
    public float maxValue;

    public bool isMnozhitel;
    
    public Stati linkStat;
    
}
