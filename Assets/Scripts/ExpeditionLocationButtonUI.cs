using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExpeditionLocationButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text titleText;

    private ExpeditionLocationData location;
    private Action<ExpeditionLocationData> onSelected;

    public void Setup(
        ExpeditionLocationData locationData,
        Action<ExpeditionLocationData> selectedCallback)
    {
        location = locationData;
        onSelected = selectedCallback;

        if (button == null)
            button = GetComponent<Button>();

        if (titleText == null)
            titleText = GetComponentInChildren<TMP_Text>(true);

        if (button == null)
        {
            Debug.LogError("На prefab кнопки локации не найден компонент Button.", this);
            return;
        }

        if (titleText != null && location != null)
            titleText.text = location.locationName;

        button.onClick.RemoveListener(OnButtonClicked);
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (location != null)
            onSelected?.Invoke(location);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnButtonClicked);
    }
}