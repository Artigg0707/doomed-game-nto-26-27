using UnityEngine;
using System;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    public ResourceData resourceData;

    [Header("Balance")]
    [Tooltip("Survival config for starting resources. If not assigned, built-in defaults are used.")]
    [SerializeField] private SurvivalConfig survivalConfig;

    public SurvivalConfig SurvivalConfig => survivalConfig;

    public int FoodCans => resourceData != null ? resourceData.foodCans : 0;
    public int WaterBottles => resourceData != null ? resourceData.waterBottles : 0;
    public int FuelCans => resourceData != null ? resourceData.fuelCans : 0;
    public int TemperatureLevel => resourceData != null ? resourceData.temperatureLevel : 0;
    public bool GeneratorBroken => resourceData != null && resourceData.generatorBroken;

    public bool GeneratorIsOn => resourceData != null && resourceData.generatorIsOn;

    public string GeneratorStatus
    {
        get
        {
            if (resourceData == null)
                return "Неизвестно";

            if (resourceData.generatorBroken)
                return "Сломан";

            if (!resourceData.generatorIsOn)
                return "Выключен";

            if (resourceData.fuelCans <= 0)
                return "Нет топлива";

            return "Работает";
        }
    }

    public bool CanTurnOnGenerator =>
        resourceData != null && !resourceData.generatorBroken && resourceData.fuelCans > 0;

    public event Action OnResourcesChanged;

    private const int MinResourceCount = 0;
    private const int MaxTemperatureLevel = 3;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (resourceData == null)
            resourceData = new ResourceData();

        
        resourceData.foodCans = RandomStartValue(survivalConfig?.startFoodMin, survivalConfig?.startFoodMax, 8, 10);
        resourceData.waterBottles = RandomStartValue(survivalConfig?.startWaterMin, survivalConfig?.startWaterMax, 8, 10);
        resourceData.fuelCans = RandomStartValue(survivalConfig?.startFuelMin, survivalConfig?.startFuelMax, 4, 6);
        resourceData.temperatureLevel = 3;
        resourceData.generatorBroken = false;

        OnResourcesChanged?.Invoke();
    }

    public bool ConsumeFood(int amount)
    {
        if (resourceData == null)
            return false;

        return Consume(ref resourceData.foodCans, amount);
    }

    public bool ConsumeWater(int amount)
    {
        if (resourceData == null)
            return false;

        return Consume(ref resourceData.waterBottles, amount);
    }

    public bool ConsumeFuel(int amount)
    {
        if (resourceData == null)
            return false;

        return Consume(ref resourceData.fuelCans, amount);
    }

    public void AddFood(int amount)
    {
        if (resourceData == null)
            return;

        Add(ref resourceData.foodCans, amount);
    }

    public void AddWater(int amount)
    {
        if (resourceData == null)
            return;

        Add(ref resourceData.waterBottles, amount);
    }

    public void AddFuel(int amount)
    {
        if (resourceData == null)
            return;

        Add(ref resourceData.fuelCans, amount);
    }

    public void SetTemperature(int level)
    {
        if (resourceData == null)
            return;

        resourceData.temperatureLevel = Math.Clamp(level, MinResourceCount, MaxTemperatureLevel);
        OnResourcesChanged?.Invoke();
    }

    public void AddTemperature(int amount)
    {
        if (resourceData == null)
            return;

        SetTemperature(resourceData.temperatureLevel + amount);
    }

    public void BreakGenerator()
    {
        if (resourceData == null)
            return;

        if (resourceData.generatorBroken)
            return;

        resourceData.generatorBroken = true;
        OnResourcesChanged?.Invoke();
    }

    public void RepairGenerator()
    {
        if (resourceData == null)
            return;

        if (!resourceData.generatorBroken)
            return;

        resourceData.generatorBroken = false;
        OnResourcesChanged?.Invoke();
    }

    public void SetGeneratorOn(bool on)
    {
        if (resourceData == null)
            return;

        if (on && resourceData.generatorBroken)
            return;

        resourceData.generatorIsOn = on;
        OnResourcesChanged?.Invoke();
    }

    private bool Consume(ref int resource, int amount)
    {
        if (resourceData == null || amount < 0)
            return false;

        if (resource < amount)
            return false;

        resource -= amount;
        OnResourcesChanged?.Invoke();
        return true;
    }

    private void Add(ref int resource, int amount)
    {
        if (resourceData == null || amount < 0)
            return;

        resource = resource + amount;
        OnResourcesChanged?.Invoke();
    }

    [Obsolete("Use ConsumeFood instead.")]
    public void addFood(int amount)
    {
        ConsumeFood(amount);
    }

    [Obsolete("Use ConsumeWater instead.")]
    public void addWater(int amount)
    {
        ConsumeWater(amount);
    }

    [Obsolete("Use ConsumeFuel instead.")]
    public void addFuel(int amount)
    {
        ConsumeFuel(amount);
    }

    [Obsolete("Use BreakGenerator instead.")]
    public void BrokeGenerator()
    {
        BreakGenerator();
    }

    private int RandomStartValue(int? min, int? max, int defaultMin, int defaultMax)
    {
        int lo = min.HasValue ? Mathf.Max(0, min.Value) : defaultMin;
        int hi = max.HasValue ? Mathf.Max(lo, max.Value) : defaultMax;
        return UnityEngine.Random.Range(lo, hi + 1);
    }
}
