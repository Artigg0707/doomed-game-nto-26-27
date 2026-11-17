using UnityEngine;
using System;
using System.Linq;

public class ResoursManger : MonoBehaviour
{
    public ResoursData resoursData;
    private int minResoursCount = 0;
    private int maxTempLevel = 3;
    void Start()
    {
        var rnd = new System.Random();
        
        resoursData.currentDay = 0;
        resoursData.foodCans = rnd.Next(15, 25);
        resoursData.waterBottles = rnd.Next(15, 25);
        resoursData.fuelCans = rnd.Next(15, 25);
        resoursData.temperatureLevel = 3;
        resoursData.generatorBroken = false;
        
    }

    public void addFood(int amount)
    {
        resoursData.foodCans = Math.Max(resoursData.temperatureLevel + amount, minResoursCount);
    }
    public void addWater(int amount)
    {
        resoursData.waterBottles = Math.Max(resoursData.waterBottles + amount, minResoursCount);
    }
    public void addFuel(int amount)
    {
        resoursData.fuelCans = Math.Max(resoursData.fuelCans + amount, minResoursCount);
    }
    public void AddTemperature(int amount)
    {
        resoursData.temperatureLevel += amount;
        resoursData.temperatureLevel = Math.Clamp(resoursData.temperatureLevel, minResoursCount, maxTempLevel);
    }

    public void BrokeGenerator()
    {
        resoursData.generatorBroken = true;
    }
    public void RepairGenerator()
    {
        resoursData.generatorBroken = false;
    }
    
    
}
