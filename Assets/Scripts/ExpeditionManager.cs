using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ExpeditionManager : MonoBehaviour
{
    [Serializable]
    private class ExpeditionRun
    {
        public ExpeditionLocationData location;
        public GeneratedCharacter character;
        public int daysRemaining;
    }

    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private List<ExpeditionRun> activeExpeditions =
        new List<ExpeditionRun>();

    public event Action OnExpeditionsChanged;

    private void Awake()
    {
        if (characterManager == null)
            characterManager = GetComponent<CharacterManager>();
    }

    public bool IsCharacterOnExpedition(GeneratedCharacter character)
    {
        if (character == null)
            return false;

        foreach (ExpeditionRun expedition in activeExpeditions)
        {
            if (expedition != null && expedition.character == character)
                return true;
        }

        return false;
    }

    public bool TryStartExpedition(
        ExpeditionLocationData location,
        GeneratedCharacter character,
        out string message)
    {
        if (location == null)
        {
            message = "Не выбрана локация.";
            return false;
        }

        if (character == null)
        {
            message = "Не выбран персонаж.";
            return false;
        }

        if (characterManager == null || characterManager.characters == null)
        {
            message = "Не найден CharacterManager или список персонажей.";
            return false;
        }

        if (!characterManager.characters.Contains(character))
        {
            message = "Персонаж отсутствует в списке CharacterManager.";
            return false;
        }

        if (character.isDead || character.isExiled)
        {
            message = "Этот персонаж не может отправиться в вылазку.";
            return false;
        }

        if (IsCharacterOnExpedition(character))
        {
            message = "Этот персонаж уже находится в вылазке.";
            return false;
        }

        // Если пайки уже назначили, отменяем их через существующую логику.
        // Списанные ресурсы вернутся; ночью персонаж не останется с назначенным пайком.
        if (character.receivesFoodToday)
            characterManager.SetFoodRation(character, false);

        if (character.receivesWaterToday)
            characterManager.SetWaterRation(character, false);

        activeExpeditions.Add(new ExpeditionRun
        {
            location = location,
            character = character,
            daysRemaining = Mathf.Max(1, location.durationDays)
        });

        message =
            $"{character.fullName} отправлен(а) в локацию «{location.locationName}».";

        OnExpeditionsChanged?.Invoke();
        return true;
    }

    // Вызывается один раз за завершённый игровой день.
    public void ProcessCompletedDay(NightReportData report)
    {
        if (activeExpeditions == null || activeExpeditions.Count == 0)
            return;

        bool changed = false;

        // Идём с конца, потому что завершённые вылазки удаляются из списка.
        for (int i = activeExpeditions.Count - 1; i >= 0; i--)
        {
            ExpeditionRun expedition = activeExpeditions[i];

            if (expedition == null)
            {
                activeExpeditions.RemoveAt(i);
                changed = true;
                continue;
            }

            expedition.daysRemaining--;
            changed = true;

            if (expedition.daysRemaining > 0)
                continue;

            string rewards = GiveRewards(expedition.location);

            string characterName = expedition.character != null
                ? expedition.character.fullName
                : "Персонаж";

            string locationName = expedition.location != null
                ? expedition.location.locationName
                : "неизвестную локацию";

            string returnMessage =
                $"{characterName} вернулся(лась) из локации «{locationName}». " +
                $"Награды: {rewards}.";

            if (report != null && report.expeditionReturns != null)
                report.expeditionReturns.Add(returnMessage);

            Debug.Log(returnMessage);
            activeExpeditions.RemoveAt(i);
        }

        if (changed)
            OnExpeditionsChanged?.Invoke();
    }

    private string GiveRewards(ExpeditionLocationData location)
    {
        if (location == null ||
            location.possibleRewards == null ||
            location.possibleRewards.Count == 0)
        {
            return "нет наград";
        }

        ResourceManager resources = ResourceManager.Instance;

        if (resources == null)
        {
            Debug.LogError("ResourceManager не найден: награды не выданы.");
            return "не удалось выдать награды";
        }

        StringBuilder result = new StringBuilder();

        foreach (ExpeditionRewardData reward in location.possibleRewards)
        {
            if (reward == null)
                continue;

            int minAmount = Mathf.Max(0, reward.minAmount);
            int maxAmount = Mathf.Max(minAmount, reward.maxAmount);

            // Верхняя граница Random.Range для int не включается,
            // поэтому прибавляем 1, чтобы maxAmount тоже мог выпасть.
            int amount = UnityEngine.Random.Range(minAmount, maxAmount + 1);

            if (amount <= 0)
                continue;

            switch (reward.resourceType)
            {
                case ExpeditionRewardType.Food:
                    resources.AddFood(amount);
                    break;

                case ExpeditionRewardType.Water:
                    resources.AddWater(amount);
                    break;

                case ExpeditionRewardType.Fuel:
                    resources.AddFuel(amount);
                    break;
            }

            if (result.Length > 0)
                result.Append(", ");

            result.Append(GetRewardName(reward.resourceType));
            result.Append(" +");
            result.Append(amount);
        }

        return result.Length > 0 ? result.ToString() : "нет наград";
    }

    private string GetRewardName(ExpeditionRewardType rewardType)
    {
        switch (rewardType)
        {
            case ExpeditionRewardType.Food:
                return "еда";

            case ExpeditionRewardType.Water:
                return "вода";

            case ExpeditionRewardType.Fuel:
                return "топливо";

            default:
                return "ресурс";
        }
    }
}