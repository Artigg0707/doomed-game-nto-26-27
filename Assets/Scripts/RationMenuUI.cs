using TMPro;
using UnityEngine;

public class RationMenuUI : MonoBehaviour, IOpenablePanel
{
    [Header("Data")]
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private RationCharacterCardUI cardPrefab;

    [Header("UI")]
    [SerializeField] private RectTransform cardsContainer;
    [SerializeField] private TMP_Text foodCountText;
    [SerializeField] private TMP_Text waterCountText;
    [SerializeField] private TMP_Text messageText;

    private ResourceManager subscribedResourceManager;

    private void OnEnable()
    {
        SubscribeToEvents();
        RefreshResourceCounts();
    }

    private void OnDisable()
    {
        if (characterManager != null)
            characterManager.OnRationsChanged -= RefreshResourceCounts;

        if (subscribedResourceManager != null)
            subscribedResourceManager.OnResourcesChanged -= RefreshResourceCounts;

        subscribedResourceManager = null;
    }

    public void Open()
    {
        gameObject.SetActive(true);

        SubscribeToEvents();

        if (messageText != null)
            messageText.text = "";

        RebuildCards();
        RefreshResourceCounts();
    }

    public void ClosePanel()
    {
        // Назначения сохраняются. Здесь ничего не сбрасываем.
        gameObject.SetActive(false);
    }

    public void ShowMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }

    private void SubscribeToEvents()
    {
        if (characterManager != null)
        {
            characterManager.OnRationsChanged -= RefreshResourceCounts;
            characterManager.OnRationsChanged += RefreshResourceCounts;
        }

        ResourceManager currentResourceManager = ResourceManager.Instance;

        if (subscribedResourceManager != null &&
            subscribedResourceManager != currentResourceManager)
        {
            subscribedResourceManager.OnResourcesChanged -= RefreshResourceCounts;
        }

        subscribedResourceManager = currentResourceManager;

        if (subscribedResourceManager != null)
        {
            subscribedResourceManager.OnResourcesChanged -= RefreshResourceCounts;
            subscribedResourceManager.OnResourcesChanged += RefreshResourceCounts;
        }
    }

    private void RefreshResourceCounts()
    {
        ResourceManager resources = ResourceManager.Instance;

        if (resources == null)
        {
            if (foodCountText != null)
                foodCountText.text = "Еда: доступно —";

            if (waterCountText != null)
                waterCountText.text = "Вода: доступно —";

            return;
        }

        if (foodCountText != null)
            foodCountText.text = $"Еда: доступно {resources.FoodCans}";

        if (waterCountText != null)
            waterCountText.text = $"Вода: доступно {resources.WaterBottles}";
    }

    private void RebuildCards()
    {
        if (characterManager == null)
        {
            Debug.LogError("В RationMenuUI не назначен CharacterManager.", this);
            return;
        }

        if (cardPrefab == null)
        {
            Debug.LogError("В RationMenuUI не назначен prefab карточки пайка.", this);
            return;
        }

        if (cardsContainer == null)
        {
            Debug.LogError("В RationMenuUI не назначен Content для карточек.", this);
            return;
        }

        // Удаляем ранее созданные карточки, но оставляем другие объекты Content.
        for (int i = cardsContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = cardsContainer.GetChild(i);

            if (child.GetComponent<RationCharacterCardUI>() != null)
                Destroy(child.gameObject);
        }

        if (characterManager.characters == null)
            return;

        foreach (GeneratedCharacter character in characterManager.characters)
        {
            if (character == null || character.isDead || character.isExiled)
                continue;

            RationCharacterCardUI card =
                Instantiate(cardPrefab, cardsContainer, false);

            card.Setup(character, characterManager, ShowMessage);
        }
    }
}