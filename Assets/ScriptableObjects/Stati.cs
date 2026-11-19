using Unity.Cinemachine;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Stati", menuName = "Scriptable Objects/Новый параметр")]
public class Stati : ScriptableObject
{
    public Sprite icon;
    public string statName;
    public string statDesc;
    public Color statColor;
    
    
    
    public float minValue;
    public float maxValue;
    public float minDefaultValue;
    public float maxDefaultValue;


    public float baseDecayPerDay;

    //допистаь состояния и влияние скрытых параметров
    //
    //
    //
    //


}


