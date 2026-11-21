using TMPro;
using UnityEngine;

public class ActiveExpeditionEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text locationNameText;
    [SerializeField] private TMP_Text daysRemainingText;

    public void SetData(
        string characterName,
        string locationName,
        int daysRemaining)
    {
        if (characterNameText != null)
            characterNameText.text = characterName;

        if (locationNameText != null)
            locationNameText.text = $"Локация: {locationName}";

        if (daysRemainingText != null)
        {
            daysRemainingText.text =
                $"Осталось: {FormatDays(daysRemaining)}";
        }
    }

    private string FormatDays(int days)
    {
        days = Mathf.Max(0, days);

        int lastTwoDigits = days % 100;
        int lastDigit = days % 10;

        if (lastTwoDigits >= 11 && lastTwoDigits <= 14)
            return $"{days} дней";

        if (lastDigit == 1)
            return $"{days} день";

        if (lastDigit >= 2 && lastDigit <= 4)
            return $"{days} дня";

        return $"{days} дней";
    }
}