using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExpeditionsTab : MonoBehaviour, IPCTab
{
    [Header("Локации")]
    [SerializeField] private Transform locationListParent;
    [SerializeField] private GameObject locationButtonPrefab;

    [Header("Персонажи")]
    [SerializeField] private Transform characterListParent;
    [SerializeField] private GameObject characterButtonPrefab;

    [Header("Инфо и кнопка")]
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private Button sendButton;
    [SerializeField] private ExpeditionConfirmDialog confirmDialog;

    [Header("Ссылки")]
    [SerializeField] private CharacterManager characterManager;

    private ExpeditionLocation _selectedLocation;
    private readonly List<GeneratedCharacter> _selected = new List<GeneratedCharacter>();

    private void Awake()
    {
        if (characterManager == null)
            characterManager = FindObjectOfType<CharacterManager>();
    }

    public void Show() { gameObject.SetActive(true); Refresh(); }
    public void Hide() { gameObject.SetActive(false); }

    public void Refresh()
    {
        if (ExpeditionManager.Instance == null) return;

        Clear(locationListParent);
        foreach (var loc in ExpeditionManager.Instance.locations)
        {
            var go = Instantiate(locationButtonPrefab, locationListParent);
            var btn = go.GetComponentInChildren<Button>();
            var txt = go.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.text = $"{loc.displayName} (риск {loc.riskLevel})";
            var l = loc;
            if (btn != null) btn.onClick.AddListener(() => SelectLocation(l));
        }

        Clear(characterListParent);
        if (characterManager != null)
        {
            foreach (var c in characterManager.characters)
            {
                if (c == null || c.isDead || c.isExiled || c.status == CharacterStatus.OnExpedition) continue;
                var go = Instantiate(characterButtonPrefab, characterListParent);
                var btn = go.GetComponentInChildren<Button>();
                var txt = go.GetComponentInChildren<TMP_Text>();
                if (txt != null) txt.text = c.fullName + (c.status != CharacterStatus.Free ? " (занят)" : "");
                var ch = c;
                if (btn != null)
                {
                    btn.interactable = c.status == CharacterStatus.Free;
                    btn.onClick.AddListener(() => ToggleCharacter(ch));
                }
                var image = go.GetComponent<Image>();
                if (image != null)
                {
                    image.color = _selected.Contains(ch) ? Color.yellow : Color.white;
                    if (ch.portrait != null)
                        image.sprite = ch.portrait;
                }
            }
        }

        UpdateInfo();
        UpdateSendButton();
    }

    private void Clear(Transform parent)
    {
        if (parent == null) return;
        foreach (Transform child in parent) Destroy(child.gameObject);
    }

    public void SelectLocation(ExpeditionLocation loc)
    {
        _selectedLocation = loc;
        Refresh();
    }

    public void ToggleCharacter(GeneratedCharacter c)
    {
        if (c.status != CharacterStatus.Free) return;
        if (_selected.Contains(c)) _selected.Remove(c);
        else if (_selected.Count < 2) _selected.Add(c);
        Refresh();
    }

    private void UpdateInfo()
    {
        if (infoText == null) return;
        if (_selectedLocation == null)
        {
            infoText.text = "Выберите локацию";
            return;
        }
        var loc = _selectedLocation;
        infoText.text = $"{loc.displayName}\nРиск: {loc.riskLevel}\nНаграда: +{loc.rewardFood} еды, +{loc.rewardWater} воды, +{loc.rewardFuel} топлива";
    }

    private void UpdateSendButton()
    {
        if (sendButton == null) return;
        sendButton.interactable = _selectedLocation != null && _selected.Count >= 1 && _selected.Count <= 2;
    }

    public void Send()
    {
        if (_selectedLocation == null || _selected.Count == 0) return;
        if (ExpeditionManager.Instance == null) return;
        int day = DayCycleManager.Instance != null ? DayCycleManager.Instance.CurrentDay : 1;

        if (confirmDialog != null)
        {
            confirmDialog.Show(_selectedLocation, new List<GeneratedCharacter>(_selected), () =>
            {
                ExpeditionManager.Instance.Send(_selectedLocation, new List<GeneratedCharacter>(_selected), day);
                _selected.Clear();
                _selectedLocation = null;
                Refresh();
            });
        }
        else
        {
            ExpeditionManager.Instance.Send(_selectedLocation, new List<GeneratedCharacter>(_selected), day);
            _selected.Clear();
            _selectedLocation = null;
            Refresh();
        }
    }
}