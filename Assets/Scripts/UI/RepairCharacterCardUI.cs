using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RepairCharacterCardUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text nameText;

    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor =
        new Color(1f, 0.8f, 0.25f, 1f);

    private GeneratedCharacter character;
    private Action<GeneratedCharacter> onSelected;

    public GeneratedCharacter Character => character;

    public void Setup(
        GeneratedCharacter characterData,
        Action<GeneratedCharacter> selectedCallback)
    {
        character = characterData;
        onSelected = selectedCallback;

        if (button == null)
            button = GetComponent<Button>();

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (button == null)
        {
            Debug.LogError("На карточке починки нет компонента Button.", this);
            return;
        }

        if (character == null)
        {
            Debug.LogError("Для карточки починки не назначен персонаж.", this);
            button.interactable = false;
            return;
        }

        button.transition = Selectable.Transition.None;
        button.onClick.RemoveListener(OnClicked);
        button.onClick.AddListener(OnClicked);

        if (portraitImage != null)
        {
            portraitImage.sprite = character.portrait;
            portraitImage.enabled = character.portrait != null;
            portraitImage.raycastTarget = false;
        }

        if (nameText != null)
        {
            nameText.text = character.fullName;
            nameText.raycastTarget = false;
        }

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (backgroundImage != null)
            backgroundImage.color = selected ? selectedColor : normalColor;
    }

    private void OnClicked()
    {
        if (character != null)
            onSelected?.Invoke(character);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClicked);
    }
}
