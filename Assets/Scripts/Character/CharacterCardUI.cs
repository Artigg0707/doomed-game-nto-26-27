using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCardUI : MonoBehaviour
{
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text nameText;

    public GeneratedCharacter Character { get; private set; }

    public void Setup(GeneratedCharacter character)
    {
        Character = character;

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
}