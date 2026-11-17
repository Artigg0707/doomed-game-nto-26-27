using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterCardUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text nameText;

    public GeneratedCharacter Character { get; private set; }

    private CharacterBioPanelUI bioPanel;

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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Character != null && bioPanel != null)
            bioPanel.ShowCharacter(Character);
    }
}