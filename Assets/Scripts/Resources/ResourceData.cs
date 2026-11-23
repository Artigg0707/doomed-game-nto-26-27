using UnityEngine;

[System.Serializable]
public class ResourceData
{
    public int foodCans;
    public int waterBottles;
    public int fuelCans;
    public int currentDay;
    public int temperatureLevel; // 3 = тепло, 2 = прохладно, 1 = холодно, 0 = обледенение
    public bool generatorBroken;
    public bool generatorIsOn = false;
}
