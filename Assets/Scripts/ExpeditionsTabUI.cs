using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExpeditionsTabUI : MonoBehaviour
{
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
            sendExpeditionButton.onClick.RemoveListener(OnSendExpeditionClicked);
            sendExpeditionButton.onClick.AddListener(OnSendExpeditionClicked);
        }

        if (expeditionManager != null)
        {
            expeditionManager.OnExpeditionsChanged -= HandleExpeditionsChanged;
            expeditionManager.OnExpeditionsChanged += HandleExpeditionsChanged;
        }

        if (!locationButtonsCreated)
            BuildLocationButtons();

        RefreshSendButton();
    }

    private void OnDisable()
    {
        if (sendExpeditionButton != null)
            sendExpeditionButton.onClick.RemoveListener(OnSendExpeditionClicked);

        if (expeditionManager != null)
            expeditionManager.OnExpeditionsChanged -= HandleExpeditionsChanged;
    }

    public void RefreshLocations()
    {
        locationButtonsCreated = false;
        BuildLocationButtons();
    }

    private void BuildLocationButtons()
    {
        if (locationCatalog == null)
        {
            Debug.LogError("В ExpeditionsTabUI не назначен Location Catalog.", this);
            return;
        }

        if (locationButtonPrefab == null)
        {
            Debug.LogError("В ExpeditionsTabUI не назначен Location Button Prefab.", this);
            return;
        }

        if (locationButtonsContent == null)
        {
            Debug.LogError("В ExpeditionsTabUI не назначен Location Buttons Content.", this);
            return;
        }

        ClearChildren(locationButtonsContent);
        ShowEmptyState();

        if (locationCatalog.locations != null)
        {
            foreach (ExpeditionLocationData location in locationCatalog.locations)
            {
                if (location == null)
                    continue;

                ExpeditionLocationButtonUI button =
                    Instantiate(locationButtonPrefab, locationButtonsContent, false);

                button.Setup(location, SelectLocation);
            }
        }

        locationButtonsCreated = true;
    }

    private void SelectLocation(ExpeditionLocationData location)
    {
        if (location == null)
            return;

        SelectedLocation = location;
        SelectedCharacter = null;

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
            string risk = string.IsNullOrWhiteSpace(location.riskDescription)
                ? "не указан"
                : location.riskDescription;

            riskText.text = $"Риск: {risk}";
        }

        if (rewardsText != null)
            rewardsText.text = BuildRewardsText(location);

        if (durationText != null)
            durationText.text = $"Длительность: {FormatDays(location.durationDays)}";

        if (locationImage != null)
        {
            locationImage.sprite = location.locationImage;
            locationImage.enabled = location.locationImage != null;
            locationImage.raycastTarget = false;
        }

        BuildCharacterCards();
        RefreshSendButton();
    }

    private void BuildCharacterCards()
    {
        if (characterCardsContent == null)
        {
            Debug.LogError("В ExpeditionsTabUI не назначен Character Cards Content.", this);
            return;
        }

        if (characterCardPrefab == null)
        {
            Debug.LogError("В ExpeditionsTabUI не назначен Character Card Prefab.", this);
            return;
        }

        ClearChildren(characterCardsContent);
        characterCards.Clear();

        if (characterManager == null || characterManager.characters == null)
        {
            Debug.LogError(
                "В ExpeditionsTabUI не назначен CharacterManager " +
                "или список characters.",
                this);
            return;
        }

        foreach (GeneratedCharacter character in characterManager.characters)
        {
            if (character == null || character.isDead || character.isExiled)
                continue;

            if (expeditionManager != null &&
                expeditionManager.IsCharacterOnExpedition(character))
            {
                continue;
            }

            ExpeditionCharacterCardUI card =
                Instantiate(characterCardPrefab, characterCardsContent, false);

            card.Setup(character, SelectCharacter);
            card.SetSelected(character == SelectedCharacter);
            characterCards.Add(card);
        }
    }

    private void SelectCharacter(GeneratedCharacter character)
    {
        if (character == null || character.isDead || character.isExiled)
            return;

        if (expeditionManager != null &&
            expeditionManager.IsCharacterOnExpedition(character))
        {
            return;
        }

        SelectedCharacter = character;

        foreach (ExpeditionCharacterCardUI card in characterCards)
        {
            if (card != null)
                card.SetSelected(card.Character == SelectedCharacter);
        }

        RefreshSendButton();
    }

    private void HandleExpeditionsChanged()
    {
        if (SelectedCharacter != null &&
            (SelectedCharacter.isDead ||
             SelectedCharacter.isExiled ||
             (expeditionManager != null &&
              expeditionManager.IsCharacterOnExpedition(SelectedCharacter))))
        {
            SelectedCharacter = null;
        }

        if (SelectedLocation != null && gameObject.activeInHierarchy)
            BuildCharacterCards();

        RefreshSendButton();
    }

    private void OnSendExpeditionClicked()
    {
        if (SelectedLocation == null || SelectedCharacter == null)
            return;

        if (expeditionManager == null)
        {
            Debug.LogError("В ExpeditionsTabUI не назначен ExpeditionManager.", this);
            return;
        }

        if (!expeditionManager.TryStartExpedition(
                SelectedLocation,
                SelectedCharacter,
                out string message))
        {
            Debug.LogWarning(message, this);
            return;
        }

        Debug.Log(message, this);

        // ExpeditionManager вызывает OnExpeditionsChanged:
        // этот обработчик обновит список и сбросит уже отправленного персонажа.
        SelectedCharacter = null;
        RefreshSendButton();
    }

    private void RefreshSendButton()
    {
        if (sendExpeditionButton == null)
            return;

        bool characterAvailable =
            SelectedCharacter != null &&
            !SelectedCharacter.isDead &&
            !SelectedCharacter.isExiled &&
            (expeditionManager == null ||
             !expeditionManager.IsCharacterOnExpedition(SelectedCharacter));

        sendExpeditionButton.interactable =
            SelectedLocation != null &&
            characterAvailable &&
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

        foreach (ExpeditionRewardData reward in location.possibleRewards)
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