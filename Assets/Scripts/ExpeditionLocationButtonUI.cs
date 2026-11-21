using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExpeditionLocationButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text titleText;

    private ExpeditionLocationData location;
    private ExpeditionManager expeditionManager;
    private Action<ExpeditionLocationData> onSelected;

    public void Setup(
        ExpeditionLocationData locationData,
        Action<ExpeditionLocationData> selectedCallback,
        ExpeditionManager manager)
    {
        location = locationData;
        onSelected = selectedCallback;
        expeditionManager = manager;

        if (button == null)
            button = GetComponent<Button>();

        if (titleText == null)
            titleText = GetComponentInChildren<TMP_Text>(true);

        if (button == null)
        {
            Debug.LogError("На кнопке локации нет компонента Button.", this);
            return;
        }

        button.onClick.RemoveListener(OnButtonClicked);
        button.onClick.AddListener(OnButtonClicked);

        RefreshAvailability();
    }

    public void RefreshAvailability()
    {
        if (button == null || location == null)
            return;

        bool available = expeditionManager == null ||
                         expeditionManager.IsLocationAvailable(location);

        button.interactable = available;

        if (titleText == null)
            return;

        string title = location.locationName;

        if (!available && expeditionManager != null)
        {
            string reason =
                expeditionManager.GetLocationUnavailableReason(location);

            if (!string.IsNullOrEmpty(reason))
                title += $"\n{reason}";
        }

        titleText.text = title;
    }

    private void OnButtonClicked()
    {
        if (button != null &&
            button.interactable &&
            location != null)
        {
            onSelected?.Invoke(location);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnButtonClicked);
    }
}