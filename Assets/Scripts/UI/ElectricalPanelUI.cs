using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElectricalPanelUI : MonoBehaviour, IOpenablePanel
{
    [Header("Data")]
    [SerializeField] private ElectricalPanelController panelController;
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private ExpeditionManager expeditionManager;
    [SerializeField] private RepairCharacterCardUI cardPrefab;

    [Header("UI — информация")]
    [SerializeField] private TMP_Text temperatureText;
    [SerializeField] private TMP_Text fuelCountText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text messageText;

    [Header("UI — цвета температуры")]
    [SerializeField] private Color temperatureColorNormal = Color.white;
    [SerializeField] private Color temperatureColorCold = new Color(0.5f, 0.8f, 1f, 1f);
    [SerializeField] private Color temperatureColorFreezing = new Color(1f, 0.6f, 0.6f, 1f);
    [SerializeField] private Color temperatureColorCritical = Color.red;

    [Header("UI — управление")]
    [SerializeField] private Button toggleGeneratorButton;
    [SerializeField] private TMP_Text toggleButtonText;

    [Header("UI — ремонт (показывается при поломке)")]
    [SerializeField] private GameObject repairSection;
    [SerializeField] private RectTransform cardsContainer;
    [SerializeField] private Button sendRepairButton;
    [SerializeField] private TMP_Text sendButtonText;

    private GeneratedCharacter selectedRepairCharacter;
    private readonly List<RepairCharacterCardUI> characterCards = new();

    private void OnEnable()
    {
        SubscribeToEvents();
        Refresh();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    public void Open()
    {
        gameObject.SetActive(true);
        if (messageText != null)
            messageText.text = "";
        Refresh();
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    public void ShowMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }

    private void SubscribeToEvents()
    {
        if (panelController != null)
        {
            panelController.OnResourcesChanged -= Refresh;
            panelController.OnResourcesChanged += Refresh;
            panelController.OnGeneratorStatusChanged -= Refresh;
            panelController.OnGeneratorStatusChanged += Refresh;
        }

        if (expeditionManager != null)
        {
            expeditionManager.OnExpeditionsChanged -= HandleExpeditionsChanged;
            expeditionManager.OnExpeditionsChanged += HandleExpeditionsChanged;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (panelController != null)
        {
            panelController.OnResourcesChanged -= Refresh;
            panelController.OnGeneratorStatusChanged -= Refresh;
        }

        if (expeditionManager != null)
            expeditionManager.OnExpeditionsChanged -= HandleExpeditionsChanged;
    }

    private void HandleExpeditionsChanged()
    {
        if (!gameObject.activeInHierarchy)
            return;

        RebuildRepairCards();
        Refresh();
    }

    public void Refresh()
    {
        RefreshResourceInfo();
        RefreshToggleButton();
        RefreshRepairSection();
    }

    private void RefreshResourceInfo()
    {
        ResourceManager resources = ResourceManager.Instance;

        if (temperatureText != null)
        {
            temperatureText.text = resources != null
                ? $" {resources.TemperatureLevel}/3"
                : " —";

            if (resources != null)
                temperatureText.color = GetTemperatureColor(resources.TemperatureLevel);
        }

        if (fuelCountText != null)
            fuelCountText.text = resources != null
                ? $" {resources.FuelCans}"
                : " —";

        if (statusText != null && panelController != null)
            statusText.text = $"Генератор: {panelController.GeneratorStatusText}";
    }

    private Color GetTemperatureColor(int level)
    {
        switch (level)
        {
            case 3: return temperatureColorNormal;
            case 2: return temperatureColorCold;
            case 1: return temperatureColorFreezing;
            case 0: return temperatureColorCritical;
            default: return Color.white;
        }
    }

    private void RefreshToggleButton()
    {
        if (toggleGeneratorButton != null)
        {
            bool canInteract = panelController != null
                && ResourceManager.Instance != null
                && !ResourceManager.Instance.GeneratorBroken;
            toggleGeneratorButton.interactable = canInteract;
        }

        if (toggleButtonText != null && panelController != null)
        {
            bool isOn = ResourceManager.Instance != null && ResourceManager.Instance.GeneratorIsOn;
            toggleButtonText.text = isOn ? "Выключить генератор" : "Включить генератор";
        }
    }

    private void RefreshRepairSection()
    {
        bool showRepair = panelController != null && ResourceManager.Instance != null
            && ResourceManager.Instance.GeneratorBroken;

        if (repairSection != null)
            repairSection.SetActive(showRepair);

        if (!showRepair)
            return;

        RebuildRepairCards();

        if (sendRepairButton != null)
            sendRepairButton.interactable = selectedRepairCharacter != null;

        if (sendButtonText != null)
            sendButtonText.text = selectedRepairCharacter != null
                ? "Отправить на ремонт"
                : "Выберите персонажа для починки";
    }

    private void RebuildRepairCards()
    {
        if (cardsContainer == null || cardPrefab == null || panelController == null)
            return;

        
        for (int i = cardsContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = cardsContainer.GetChild(i);
            if (child.GetComponent<RepairCharacterCardUI>() != null)
                Destroy(child.gameObject);
        }

        characterCards.Clear();

        var available = panelController.GetAvailableRepairCharacters();
        foreach (GeneratedCharacter character in available)
        {
            RepairCharacterCardUI card = Instantiate(cardPrefab, cardsContainer, false);
            card.Setup(character, OnRepairCharacterSelected);
            card.SetSelected(character == selectedRepairCharacter);
            characterCards.Add(card);
        }

        
        if (selectedRepairCharacter != null && !available.Contains(selectedRepairCharacter))
        {
            selectedRepairCharacter = null;
        }
    }

    private void OnRepairCharacterSelected(GeneratedCharacter character)
    {
        selectedRepairCharacter = character;

        foreach (RepairCharacterCardUI card in characterCards)
        {
            if (card != null)
                card.SetSelected(card.Character == selectedRepairCharacter);
        }

        RefreshRepairSection();
    }

    public void OnToggleGeneratorClicked()
    {
        panelController?.ToggleGenerator();
        ShowMessage("");
    }

    public void OnSendRepairClicked()
    {
        if (panelController == null || selectedRepairCharacter == null)
            return;

        if (panelController.SendCharacterForRepair(selectedRepairCharacter, out string message))
        {
            ShowMessage(message);
            selectedRepairCharacter = null;
            Refresh();
        }
        else
        {
            ShowMessage(message);
        }
    }
}
