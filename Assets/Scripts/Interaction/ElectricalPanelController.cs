using System;
using UnityEngine;
using System.Collections;

public class ElectricalPanelController : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private int fuelCostPerNight = 1;
    [SerializeField] private int repairAttemptLimit = 3;

    [Header("Статус")]
    public bool IsGeneratorWorking => !resourceManager.GeneratorBroken && resourceManager.FuelCans > 0;

    private int repairAttemptsThisSession = 0;
    private bool generatorPermanentlyBroken = false;

    public event Action OnResourcesChanged;

    private void Awake()
    {
        if (resourceManager == null)
            resourceManager = ResourceManager.Instance;
    }

    public void ToggleGenerator()
    {
        if (generatorPermanentlyBroken)
        {
            Debug.Log("Генератор окончательно сломан. Не восстанавливается.");
            return;
        }

        if (IsGeneratorWorking)
        {
        }
        else
        {
            if (resourceManager.FuelCans >= fuelCostPerNight)
            {
                AttemptRepairGenerator();
            }
            else
            {
                Debug.Log("Не хватает топлива для включения генератора.");
            }
        }
        OnResourcesChanged?.Invoke();
    }

    public void AttemptRepairGenerator()
    {
        if (generatorPermanentlyBroken)
        {
            Debug.Log("Генератор уже окончательно сломан.");
            return;
        }

        repairAttemptsThisSession++;

        Debug.Log($"Попытка ремонта генератора #{repairAttemptsThisSession}");

        if (repairAttemptsThisSession >= 2)
        {
            FixGenerator();
            repairAttemptsThisSession = 0;
            Debug.Log("Генератор успешно починен!");
            return;
        }

        float roll = UnityEngine.Random.value;

        if (roll < 0.5f)
        {
            FixGenerator();
            repairAttemptsThisSession = 0;
            Debug.Log("Генератор успешно починен!");
        }
        else if (roll < 0.7f)
        {
            resourceManager.BreakGenerator();
            Debug.Log("Персонаж получил удар током! -30 HP.");
            repairAttemptsThisSession++;
            Debug.Log("Результат: Удар током. Попыток ремонта: " + repairAttemptsThisSession);
        }
        else
        {
            Debug.Log("Результат: Нечинил генератор. Попыток ремонта: " + repairAttemptsThisSession);
            repairAttemptsThisSession++;
        }

        if (repairAttemptsThisSession >= repairAttemptLimit)
        {
            PermanentlyBreakGenerator();
        }

        OnResourcesChanged?.Invoke();
    }

    private void FixGenerator()
    {
        resourceManager.RepairGenerator();
        generatorPermanentlyBroken = false;
        repairAttemptsThisSession = 0;
        Debug.Log("Генератор восстановлен.");
    }

    private void PermanentlyBreakGenerator()
    {
        resourceManager.BreakGenerator();
        generatorPermanentlyBroken = true;
        repairAttemptsThisSession = 0;
        Debug.Log("Генератор окончательно сломан. Требуется замена или вылазка за запчастями.");
    }
}