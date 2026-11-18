using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RationCharacterCardUI : MonoBehaviour
{
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Toggle foodToggle;
    [SerializeField] private Toggle waterToggle;

    private GeneratedCharacter character;
    private CharacterManager characterManager;
    private System.Action<string> messageCallback;

    public void Setup(
        GeneratedCharacter characterData,
        CharacterManager manager,
        System.Action<string> onMessage = null)
    {
        character = characterData;
        characterManager = manager;
        messageCallback = onMessage;

        if (character == null || characterManager == null)
        {
            Debug.LogError("Для карточки пайка не назначен персонаж или CharacterManager.", this);
            return;
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = character.portrait;
            portraitImage.enabled = character.portrait != null;
        }

        if (nameText != null)
            nameText.text = character.fullName;

        if (foodToggle == null || waterToggle == null)
        {
            Debug.LogError("В карточке пайка не назначены Toggle еды или воды.", this);
            return;
        }

        // Удаляем старые подписки, чтобы при повторном Setup они не дублировались.
        foodToggle.onValueChanged.RemoveListener(OnFoodToggleChanged);
        waterToggle.onValueChanged.RemoveListener(OnWaterToggleChanged);

        foodToggle.onValueChanged.AddListener(OnFoodToggleChanged);
        waterToggle.onValueChanged.AddListener(OnWaterToggleChanged);

        bool canReceiveRations = !character.isDead && !character.isExiled;
        foodToggle.interactable = canReceiveRations;
        waterToggle.interactable = canReceiveRations;

        // Устанавливаем сохранённое состояние, не вызывая обработчики и не списывая ресурсы.
        RefreshToggles();
    }

    private void OnFoodToggleChanged(bool isOn)
    {
        if (character == null || characterManager == null)
            return;

        if (isOn && !character.receivesFoodToday)
        {
            if (ResourceManager.Instance == null)
            {
                RejectFoodToggle("Не найден ResourceManager.");
                return;
            }

            if (ResourceManager.Instance.FoodCans <= 0)
            {
                RejectFoodToggle("Недостаточно еды.");
                return;
            }
        }

        characterManager.SetFoodRation(character, isOn);
        SyncFoodToggle();

        // Проверяем, принял ли CharacterManager назначение.
        if (isOn && !character.receivesFoodToday)
            messageCallback?.Invoke("Не удалось назначить еду.");
    }

    private void OnWaterToggleChanged(bool isOn)
    {
        if (character == null || characterManager == null)
            return;

        if (isOn && !character.receivesWaterToday)
        {
            if (ResourceManager.Instance == null)
            {
                RejectWaterToggle("Не найден ResourceManager.");
                return;
            }

            if (ResourceManager.Instance.WaterBottles <= 0)
            {
                RejectWaterToggle("Недостаточно воды.");
                return;
            }
        }

        characterManager.SetWaterRation(character, isOn);
        SyncWaterToggle();

        if (isOn && !character.receivesWaterToday)
            messageCallback?.Invoke("Не удалось назначить воду.");
    }

    private void RejectFoodToggle(string message)
    {
        SyncFoodToggle();
        messageCallback?.Invoke(message);
    }

    private void RejectWaterToggle(string message)
    {
        SyncWaterToggle();
        messageCallback?.Invoke(message);
    }

    private void RefreshToggles()
    {
        SyncFoodToggle();
        SyncWaterToggle();
    }

    private void SyncFoodToggle()
    {
        if (foodToggle != null && character != null)
            foodToggle.SetIsOnWithoutNotify(character.receivesFoodToday);
    }

    private void SyncWaterToggle()
    {
        if (waterToggle != null && character != null)
            waterToggle.SetIsOnWithoutNotify(character.receivesWaterToday);
    }

    private void OnDestroy()
    {
        if (foodToggle != null)
            foodToggle.onValueChanged.RemoveListener(OnFoodToggleChanged);

        if (waterToggle != null)
            waterToggle.onValueChanged.RemoveListener(OnWaterToggleChanged);
    }
}