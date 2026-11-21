using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public CharacterGenerator genCgaracter;
    public List<GeneratedCharacter> characters = new List<GeneratedCharacter>();

    public event System.Action OnRationsChanged;

    [Header("Карточки персонажей")]
    [SerializeField] private CharacterCardUI cardPrefab;
    [SerializeField] private RectTransform cardContainer;
    [SerializeField, Min(0)] private int initialCharacterCount = 5;

    [SerializeField] private float edgePadding = 10f;
    [SerializeField] private float cardSpacing = 12f;
    [SerializeField] private int positionAttempts = 80;
    [SerializeField] private CharacterBioPanelUI bioPanel;

    private void Start()
    {
        GenerateInitialCharacters();
        SubscribeToNightEvents();
    }

    private void OnEnable()
    {
        SubscribeToNightEvents();
    }

    private void OnDisable()
    {
        if (DayCycleManager.Instance != null)
            DayCycleManager.Instance.OnNightComplete -= OnNightComplete;
    }

    private void OnNightComplete(NightReportData report)
    {
        RemoveInactiveCharacterCards();
    }

    private void SubscribeToNightEvents()
    {
        if (DayCycleManager.Instance == null)
            return;

        DayCycleManager.Instance.OnNightComplete -= OnNightComplete;
        DayCycleManager.Instance.OnNightComplete += OnNightComplete;
    }

    private void GenerateInitialCharacters()
    {
        if (characters.Count > 0)
            return;

        if (genCgaracter != null)
            genCgaracter.ResetAvailablePortraits();

        for (int i = 0; i < initialCharacterCount; i++)
        {
            CreateCharacter();
        }
    }

    public GeneratedCharacter CreateCharacter()
    {
        if (genCgaracter == null)
        {
            Debug.LogError("В CharacterManager не назначен CharacterGenerator.", this);
            return null;
        }

        if (genCgaracter.profile == null)
        {
            Debug.LogError("В CharacterGenerator не назначен CharaterObject в поле Profile.", this);
            return null;
        }

        GeneratedCharacter newCharacter = genCgaracter.Generate();

        if (newCharacter == null)
            return null;

        characters.Add(newCharacter);
        CreateCard(newCharacter);

        return newCharacter;
    }

    public bool SetFoodRation(GeneratedCharacter character, bool receivesFood)
    {
        if (!CanChangeRation(character))
            return false;

        if (character.receivesFoodToday == receivesFood)
            return true;

        if (receivesFood)
        {
            if (ResourceManager.Instance == null ||
                !ResourceManager.Instance.ConsumeFood(1))
            {
                return false;
            }
        }
        else
        {
            if (ResourceManager.Instance != null)
                ResourceManager.Instance.AddFood(1);
        }

        character.receivesFoodToday = receivesFood;
        OnRationsChanged?.Invoke();
        return true;
    }

    public bool SetWaterRation(GeneratedCharacter character, bool receivesWater)
    {
        if (!CanChangeRation(character))
            return false;

        if (character.receivesWaterToday == receivesWater)
            return true;

        if (receivesWater)
        {
            if (ResourceManager.Instance == null ||
                !ResourceManager.Instance.ConsumeWater(1))
            {
                return false;
            }
        }
        else
        {
            if (ResourceManager.Instance != null)
                ResourceManager.Instance.AddWater(1);
        }

        character.receivesWaterToday = receivesWater;
        OnRationsChanged?.Invoke();
        return true;
    }

    public void ResetDailyRations()
    {
        foreach (GeneratedCharacter character in characters)
        {
            if (character == null)
                continue;

            character.receivesFoodToday = false;
            character.receivesWaterToday = false;
        }

        OnRationsChanged?.Invoke();
    }

    public bool HasLivingCharacters()
    {
        if (characters == null)
            return false;

        foreach (GeneratedCharacter character in characters)
        {
            if (character != null &&
                !character.isDead &&
                !character.isExiled)
            {
                return true;
            }
        }

        return false;
    }

    public void SetAllFoodRations(bool receivesFood)
    {
        foreach (GeneratedCharacter character in characters)
            SetFoodRation(character, receivesFood);
    }

    public void SetAllWaterRations(bool receivesWater)
    {
        foreach (GeneratedCharacter character in characters)
            SetWaterRation(character, receivesWater);
    }

    private bool CanChangeRation(GeneratedCharacter character)
    {
        return character != null &&
               !character.isDead &&
               !character.isExiled &&
               characters.Contains(character);
    }

    private void CreateCard(GeneratedCharacter character)
    {
        if (cardPrefab == null)
        {
            Debug.LogError("В CharacterManager не назначен префаб карточки.", this);
            return;
        }

        if (cardContainer == null)
        {
            Debug.LogError("В CharacterManager не назначен контейнер карточек.", this);
            return;
        }

        CharacterCardUI card = Instantiate(cardPrefab, cardContainer, false);

        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.localScale = Vector3.one;

        card.Setup(character, bioPanel);
        card.transform.SetAsLastSibling();

        if (TryFindPosition(cardRect, out Vector2 position))
        {
            cardRect.anchoredPosition3D = new Vector3(position.x, position.y, 0f);
        }
        else
        {
            Vector2 gridPosition = FindGridFallbackPosition(cardRect);
            cardRect.anchoredPosition3D =
                new Vector3(gridPosition.x, gridPosition.y, 0f);
        }
    }

    private Vector2 FindGridFallbackPosition(RectTransform newCard)
    {
        Rect area = cardContainer.rect;
        float cardWidth = Mathf.Max(newCard.rect.width, 100f);
        float cardHeight = Mathf.Max(newCard.rect.height, 100f);
        float stepX = cardWidth + cardSpacing;
        float stepY = cardHeight + cardSpacing;
        int columns = Mathf.Max(
            1,
            Mathf.FloorToInt(
                (area.width - edgePadding * 2f + cardSpacing) / stepX));

        CharacterCardUI[] cards =
            cardContainer.GetComponentsInChildren<CharacterCardUI>(true);
        int index = Mathf.Max(0, cards.Length - 1);
        int column = index % columns;
        int row = index / columns;

        float startX =
            -area.width * 0.5f +
            edgePadding +
            cardWidth * 0.5f;
        float startY =
            area.height * 0.5f -
            edgePadding -
            cardHeight * 0.5f;

        return new Vector2(
            startX + column * stepX,
            startY - row * stepY);
    }

    private void RemoveInactiveCharacterCards()
    {
        if (cardContainer == null)
            return;

        bool removedAnyCard = false;
        CharacterCardUI[] cards = cardContainer.GetComponentsInChildren<CharacterCardUI>(true);

        foreach (CharacterCardUI card in cards)
        {
            GeneratedCharacter character = card.Character;
            if (character == null || (!character.isDead && !character.isExiled))
                continue;

            Destroy(card.gameObject);
            removedAnyCard = true;
        }

        if (removedAnyCard && bioPanel != null)
            bioPanel.ClosePanel();
    }

    private bool TryFindPosition(RectTransform newCard, out Vector2 position)
    {
        position = Vector2.zero;

        Rect area = cardContainer.rect;

        float halfWidth = newCard.rect.width * 0.5f;
        float halfHeight = newCard.rect.height * 0.5f;

        float minX = area.xMin + halfWidth + edgePadding;
        float maxX = area.xMax - halfWidth - edgePadding;
        float minY = area.yMin + halfHeight + edgePadding;
        float maxY = area.yMax - halfHeight - edgePadding;

        if (minX > maxX || minY > maxY)
            return false;

        CharacterCardUI[] cards = cardContainer.GetComponentsInChildren<CharacterCardUI>(true);

        for (int attempt = 0; attempt < positionAttempts; attempt++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY)
            );

            Rect candidateBounds = GetPaddedBounds(
                newCard,
                candidate
            );

            bool overlaps = false;

            foreach (CharacterCardUI otherCard in cards)
            {
                if (otherCard == null || otherCard.transform == newCard.transform)
                    continue;

                RectTransform otherRect = otherCard.GetComponent<RectTransform>();

                if (otherRect == null)
                    continue;

                Rect otherBounds = GetPaddedBounds(
                    otherRect,
                    otherRect.anchoredPosition
                );

                if (candidateBounds.Overlaps(otherBounds))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                position = candidate;
                return true;
            }
        }

        return false;
    }

    private Rect GetPaddedBounds(RectTransform rectTransform, Vector2 center)
    {
        float padding = cardSpacing * 0.5f;

        float width = rectTransform.rect.width + padding * 2f;
        float height = rectTransform.rect.height + padding * 2f;

        return new Rect(
            center.x - width * 0.5f,
            center.y - height * 0.5f,
            width,
            height
        );
    }
}