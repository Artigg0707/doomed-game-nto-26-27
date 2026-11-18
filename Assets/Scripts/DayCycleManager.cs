using System;
using UnityEngine;

public class DayCycleManager : MonoBehaviour
{
    public static DayCycleManager Instance { get; private set; }

    public int CurrentDay { get; private set; } = 1;
    public event Action<int> OnDayChanged;

    [SerializeField] private CharacterManager characterManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ResourceManager resources = ResourceManager.Instance;
        if (resources != null && resources.resourceData != null)
        {
            if (resources.resourceData.currentDay > 0)
                CurrentDay = resources.resourceData.currentDay;
            else
                resources.resourceData.currentDay = 1;
        }
    }

    public void StartNextDay()
    {
        ResourceManager resources = ResourceManager.Instance;

        if (resources == null)
        {
            Debug.LogError("ResourceManager is not found.", this);
            return;
        }

        if (!resources.ConsumeFuel(1))
        {
            resources.SetTemperature(resources.TemperatureLevel - 1);
        }

        if (characterManager != null)
        {
            UpdateSurvivalCounters();
            characterManager.ResetDailyRations();
        }
        else
        {
            Debug.LogError("CharacterManager is not assigned in DayCycleManager.", this);
        }

        CurrentDay++;
        resources.resourceData.currentDay = CurrentDay;
        OnDayChanged?.Invoke(CurrentDay);

        Debug.Log($"Started next day: {CurrentDay}");
    }

    private void UpdateSurvivalCounters()
    {
        foreach (GeneratedCharacter character in characterManager.characters)
        {
            if (character == null || character.isDead || character.isExiled)
                continue;

            if (character.receivesFoodToday)
                character.daysWithoutFood = 0;
            else
                character.daysWithoutFood++;

            if (character.receivesWaterToday)
                character.daysWithoutWater = 0;
            else
                character.daysWithoutWater++;
        }
    }
}
