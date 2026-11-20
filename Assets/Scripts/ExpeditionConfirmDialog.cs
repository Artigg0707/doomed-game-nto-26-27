using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExpeditionConfirmDialog : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private Action _onConfirm;

    public void Show(ExpeditionLocation loc, List<GeneratedCharacter> participants, Action onConfirm)
    {
        _onConfirm = onConfirm;
        if (messageText != null)
            messageText.text = $"Отправить {participants.Count} чел. на {loc.displayName}? Они вернутся утром. Риск: {loc.riskLevel}.";
        gameObject.SetActive(true);
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() => { gameObject.SetActive(false); _onConfirm?.Invoke(); });
        }
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() => gameObject.SetActive(false));
        }
    }
}