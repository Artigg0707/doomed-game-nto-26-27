using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExpeditionsTabUI : MonoBehaviour
{
    private const int MaxActiveExpeditions = 2;

    [Header("Location data")]
    [SerializeField] private ExpeditionLocationCatalog locationCatalog;
    [SerializeField] private ExpeditionLocationButtonUI locationButtonPrefab;
    [SerializeField] private Transform locationButtonsContent;

    [Header("Character selection")]
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private ExpeditionManager expeditionManager;
    [SerializeField] private ExpeditionCharacterCardUI characterCardPrefab;
    [SerializeField] private Transform characterCardsContent;

    [Header("Location details")]
    [SerializeField] private GameObject locationContent;
    [SerializeField] private TMP_Text emptyStateText;
    [SerializeField] private TMP_Text locationNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text riskText;
    [SerializeField] private TMP_Text rewardsText;
    [SerializeField] private TMP_Text durationText;
    [SerializeField] private Image locationImage;

    [Header("Launch")]
    [SerializeField] private Button sendExpeditionButton;

    private readonly List<ExpeditionCharacterCardUI> characterCards =
        new List<ExpeditionCharacterCardUI>();

    private bool locationButtonsCreated;

    public ExpeditionLocationData SelectedLocation { get; private set; }
    public GeneratedCharacter SelectedCharacter { get; private set; }

    private void OnEnable()
    {
        if (sendExpeditionButton != null)
        {
            sendExpeditionButton.onClick.RemoveListener(
                OnSendExpeditionClicked);

            sendExpeditionButton.onClick.AddListener(
                OnSendExpeditionClicked);
        }

        if (expeditionManager != null)
        {
            expeditionManager.OnExpeditionsChanged -=
                HandleExpeditionsChanged;

            expeditionManager.OnExpeditionsChanged +=
                HandleExpeditionsChanged;
        }
        
        BuildLocationButtons(locationButtonsCreated);
    }

    private void OnDisable()
    {
        if (sendExpeditionButton != null)
        {
            sendExpeditionButton.onClick.RemoveListener(
                OnSendExpeditionClicked);
        }

        if (expeditionManager != null)
        {
            expeditionManager.OnExpeditionsChanged -=
                HandleExpeditionsChanged;
        }
    }

    public void RefreshLocations()
    {
        BuildLocationButtons(SelectedLocation != null);
    }

    private void BuildLocationButtons(bool preserveSelection)
    {
        if (locationCatalog == null)
        {
            Debug.LogError(
                "В ExpeditionsTabUI не назначен Location Catalog.",
                this);
            return;
        }

        if (locationButtonPrefab == null)
        {
            Debug.LogError(
                "В ExpeditionsTabUI не назначен Location Button Prefab.",
                this);
            return;
        }

        if (locationButtonsContent == null)
        {
            Debug.LogError(
                "В ExpeditionsTabUI не назначен Location Buttons Content.",
                this);
            return;
        }

        ExpeditionLocationData previousLocation =
            preserveSelection ? SelectedLocation : null;

        GeneratedCharacter previousCharacter =
            preserveSelection ? SelectedCharacter : null;

        ClearChildren(locationButtonsContent);

        if (locationCatalog.locations != null)
        {
            foreach (ExpeditionLocationData location
                     in locationCatalog.locations)
            {
                if (location == null)
                    continue;

                ExpeditionLocationButtonUI button =
                    Instantiate(
                        locationButtonPrefab,
                        locationButtonsContent,
                        false);

                button.Setup(
                    location,
                    SelectLocation,
                    expeditionManager);
            }
        }

        locationButtonsCreated = true;

        if (previousLocation != null &&
            CatalogContains(previousLocation))
        {
            SelectedLocation = previousLocation;

            SelectedCharacter =
                IsCharacterAvailable(previousCharacter)
                    ? previousCharacter
                    : null;

            ShowLocationDetails(SelectedLocation);
            BuildCharacterCards();
        }
        else
        {
            ShowEmptyState();
        }

        RefreshSendButton();
    }

    private bool CatalogContains(ExpeditionLocationData location)
    {
        if (locationCatalog == null ||
            locationCatalog.locations == null ||
            location == null)
        {
            return false;
        }

        foreach (ExpeditionLocationData catalogLocation
                 in locationCatalog.locations)
        {
            if (catalogLocation == location)
                return true;
        }

        return false;
    }

    private void SelectLocation(ExpeditionLocationData location)
    {
        if (location == null)
            return;

        SelectedLocation = location;
        SelectedCharacter = null;

        ShowLocationDetails(location);
        BuildCharacterCards();
        RefreshSendButton();
    }

    private void ShowLocationDetails(ExpeditionLocationData location)
    {
        if (location == null)
            return;

        if (emptyStateText != null)
            emptyStateText.gameObject.SetActive(false);

        if (locationContent != null)
            locationContent.SetActive(true);

        if (locationNameText != null)
            locationNameText.text = location.locationName;

        if (descriptionText != null)
            descriptionText.text = location.description;

        if (riskText != null)
        {
            string risk = string.IsNullOrWhiteSpace(
                    location.riskDescription)
                ? "не указан"
                : location.riskDescription;

            riskText.text = $"Риск: {risk}";
        }

        if (rewardsText != null)
            rewardsText.text = BuildRewardsText(location);

        if (durationText != null)
        {
            durationText.text =
                $"Длительность: {FormatDays(location.durationDays)}";
        }

        if (locationImage != null)
        {
            locationImage.sprite = location.locationImage;
            locationImage.enabled = location.locationImage != null;
            locationImage.raycastTarget = false;
        }
    }

    private void BuildCharacterCards()
    {
        ClearCharacterCards();

        if (SelectedLocation == null)
            return;

        if (characterCardsContent == null)
        {
            Debug.LogError(
                "В ExpeditionsTabUI не назначен Character Cards Content.",
                this);
            return;
        }

        if (characterCardPrefab == null)
        {
            Debug.LogError(
                "В ExpeditionsTabUI не назначен Character Card Prefab.",
                this);
            return;
        }

        if (characterManager == null ||
            characterManager.characters == null)
        {
            Debug.LogError(
                "В ExpeditionsTabUI не назначен CharacterManager " +
                "или список characters.",
                this);
            return;
        }

        foreach (GeneratedCharacter character
                 in characterManager.characters)
        {
            if (!IsCharacterAvailable(character))
                continue;

            ExpeditionCharacterCardUI card =
                Instantiate(
                    characterCardPrefab,
                    characterCardsContent,
                    false);

            card.Setup(character, SelectCharacter);
            card.SetSelected(character == SelectedCharacter);

            characterCards.Add(card);
        }
    }

    private void SelectCharacter(GeneratedCharacter character)
    {
        if (!IsCharacterAvailable(character))
            return;

        SelectedCharacter = character;

        foreach (ExpeditionCharacterCardUI card in characterCards)
        {
            if (card != null)
            {
                card.SetSelected(
                    card.Character == SelectedCharacter);
            }
        }

        RefreshSendButton();
    }

    private bool IsCharacterAvailable(GeneratedCharacter character)
    {
        if (character == null ||
            character.isDead ||
            character.isExiled)
        {
            return false;
        }

        return expeditionManager == null ||
               !expeditionManager.IsCharacterOnExpedition(character);
    }

    private void HandleExpeditionsChanged()
    {
        BuildLocationButtons(true);
    }

    private void OnSendExpeditionClicked()
    {
        if (SelectedLocation == null ||
            !IsCharacterAvailable(SelectedCharacter))
        {
            RefreshSendButton();
            return;
        }

        if (expeditionManager == null)
        {
            Debug.LogError(
                "В ExpeditionsTabUI не назначен ExpeditionManager.",
                this);
            return;
        }

        if (GetActiveExpeditionCount() >= MaxActiveExpeditions)
        {
            Debug.LogWarning(
                $"Одновременно можно отправить не больше " +
                $"{MaxActiveExpeditions} вылазок.",
                this);

            RefreshSendButton();
            return;
        }

        if (!expeditionManager.TryStartExpedition(
                SelectedLocation,
                SelectedCharacter,
                out string message))
        {
            Debug.LogWarning(message, this);
            RefreshSendButton();
            return;
        }

        Debug.Log(message, this);

        SelectedCharacter = null;
        
        BuildCharacterCards();
        RefreshSendButton();
    }

    private int GetActiveExpeditionCount()
    {
        if (expeditionManager == null ||
            characterManager == null ||
            characterManager.characters == null)
        {
            return 0;
        }
        
        int count = 0;

        foreach (GeneratedCharacter character
                 in characterManager.characters)
        {
            if (character != null &&
                expeditionManager.IsCharacterOnExpedition(character))
            {
                count++;
            }
        }

        return count;
    }

    private void RefreshSendButton()
    {
        if (sendExpeditionButton == null)
            return;

        bool characterAvailable =
            IsCharacterAvailable(SelectedCharacter);

        bool hasFreeExpeditionSlot =
            GetActiveExpeditionCount() < MaxActiveExpeditions;

        sendExpeditionButton.interactable =
            SelectedLocation != null &&
            characterAvailable &&
            hasFreeExpeditionSlot &&
            expeditionManager != null;
    }

    private void ShowEmptyState()
    {
        SelectedLocation = null;
        SelectedCharacter = null;

        if (emptyStateText != null)
        {
            emptyStateText.text = "Выберите локацию";
            emptyStateText.gameObject.SetActive(true);
        }

        if (locationContent != null)
            locationContent.SetActive(false);

        if (locationImage != null)
        {
            locationImage.sprite = null;
            locationImage.enabled = false;
        }

        ClearCharacterCards();
        RefreshSendButton();
    }

    private void ClearCharacterCards()
    {
        if (characterCardsContent != null)
            ClearChildren(characterCardsContent);

        characterCards.Clear();
    }

    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);
    }

    private string BuildRewardsText(ExpeditionLocationData location)
    {
        if (location.possibleRewards == null ||
            location.possibleRewards.Count == 0)
        {
            return "Возможные награды: не указаны";
        }

        StringBuilder result = new StringBuilder();

        foreach (ExpeditionRewardData reward
                 in location.possibleRewards)
        {
            if (reward == null)
                continue;

            if (result.Length > 0)
                result.Append(", ");

            result.Append(GetRewardName(reward.resourceType));
            result.Append(": ");

            int minAmount = Mathf.Max(0, reward.minAmount);
            int maxAmount = Mathf.Max(minAmount, reward.maxAmount);

            if (minAmount == maxAmount)
                result.Append(minAmount);
            else
                result.Append(minAmount).Append("–").Append(maxAmount);
        }

        return result.Length == 0
            ? "Возможные награды: не указаны"
            : $"Возможные награды: {result}";
    }

    private string GetRewardName(ExpeditionRewardType rewardType)
    {
        switch (rewardType)
        {
            case ExpeditionRewardType.Food:
                return "Еда";

            case ExpeditionRewardType.Water:
                return "Вода";

            case ExpeditionRewardType.Fuel:
                return "Топливо";

            default:
                return "Ресурс";
        }
    }

    private string FormatDays(int days)
    {
        days = Mathf.Max(1, days);

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