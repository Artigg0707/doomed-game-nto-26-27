using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterBioPanelUI : MonoBehaviour
{
    [Header("Основная информация")]
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text ageAndGenderText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Здоровье")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthValueText;

    [Header("Мораль")]
    [SerializeField] private Slider moraleSlider;
    [SerializeField] private TMP_Text moraleValueText;

    [Header("Статусы выживания")]
    [SerializeField] private TMP_Text daysWithoutFoodText;
    [SerializeField] private TMP_Text daysWithoutWaterText;

    [Header("Особенность")]
    [SerializeField] private Image traitIcon;
    [SerializeField] private TMP_Text traitNameText;
    [SerializeField] private TMP_Text traitDescriptionText;

    [Header("Статус персонажа")]
    [SerializeField] private TMP_Text statusText;

    public void ShowCharacter(GeneratedCharacter character)
    {
        if (character == null)
            return;

        gameObject.SetActive(true);

        if (characterNameText != null)
            characterNameText.text = character.fullName;

        if (portraitImage != null)
        {
            portraitImage.sprite = character.portrait;
            portraitImage.enabled = character.portrait != null;
        }

        string genderText = character.gender == GeneratedCharacter.Gender.Male
            ? "Мужчина"
            : "Женщина";

        if (ageAndGenderText != null)
            ageAndGenderText.text = $"Возраст: {character.age} • {genderText}";

        if (descriptionText != null)
        {
            descriptionText.text = string.IsNullOrWhiteSpace(character.description)
                ? "Описание пока не заполнено."
                : character.description;
        }

        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = 100;
            healthSlider.value = Mathf.Clamp(character.health, 0, 100);
        }

        if (healthValueText != null)
            healthValueText.text = character.health.ToString();

        if (moraleSlider != null)
        {
            moraleSlider.minValue = 0;
            moraleSlider.maxValue = 100;
            moraleSlider.value = Mathf.Clamp(character.morale, 0, 100);
        }

        if (moraleValueText != null)
            moraleValueText.text = character.morale.ToString();

        if (daysWithoutFoodText != null)
            daysWithoutFoodText.text = $"Дней без еды: {character.daysWithoutFood}";

        if (daysWithoutWaterText != null)
            daysWithoutWaterText.text = $"Дней без воды: {character.daysWithoutWater}";

        Trait trait = character.traits != null && character.traits.Count > 0
            ? character.traits[0]
            : null;

        if (traitIcon != null)
        {
            traitIcon.sprite = trait != null ? trait.traitSprite : null;
            traitIcon.enabled = trait != null && trait.traitSprite != null;
        }

        if (traitNameText != null)
            traitNameText.text = trait != null ? trait.traitName : "Нет особенности";

        if (traitDescriptionText != null)
            traitDescriptionText.text = trait != null ? trait.traitDesc : "";

        if (statusText != null)
        {
            if (character.isDead)
                statusText.text = "Статус: мёртв";
            else if (character.isExiled)
                statusText.text = "Статус: изгнан";
            else
                statusText.text = "Статус: жив";
        }
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}