using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterCardUI : MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Карточка")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text nameText;

    [Header("Подсветка")]
    [SerializeField] private Color hoverColor =
        new Color(1f, 0.85f, 0.55f, 1f);

    [SerializeField] private Color pressedColor =
        new Color(0.7f, 0.85f, 1f, 1f);

    public GeneratedCharacter Character { get; private set; }

    private CharacterBioPanelUI bioPanel;
    private CharacterCardDrag dragHandler;

    private Color normalColor = Color.white;
    private bool isHovered;
    private bool isPressed;

    private void Awake()
    {
        if (cardBackground == null)
            cardBackground = GetComponent<Image>();

        dragHandler = GetComponent<CharacterCardDrag>();

        if (cardBackground != null)
            normalColor = cardBackground.color;
    }

    public void Setup(
        GeneratedCharacter character,
        CharacterBioPanelUI panel = null)
    {
        Character = character;
        bioPanel = panel;

        if (character == null)
            return;

        if (nameText != null)
            nameText.text = character.fullName;

        if (portraitImage != null)
        {
            portraitImage.sprite = character.portrait;
            portraitImage.enabled = character.portrait != null;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateCardColor();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateCardColor();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        UpdateCardColor();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        UpdateCardColor();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Если карточку двигали, не открываем биографию.
        if (dragHandler != null && dragHandler.WasDragged)
            return;

        if (Character != null && bioPanel != null)
            bioPanel.ShowCharacter(Character);
    }

    private void UpdateCardColor()
    {
        if (cardBackground == null)
            return;

        if (isPressed)
            cardBackground.color = pressedColor;
        else if (isHovered)
            cardBackground.color = hoverColor;
        else
            cardBackground.color = normalColor;
    }
}