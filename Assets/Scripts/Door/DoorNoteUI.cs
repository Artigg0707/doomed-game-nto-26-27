using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DoorNoteUI : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bodyText;
    public Transform buttonContainer;
    public GameObject buttonPrefab;

    private DoorNoteData _currentNote;

    void Start()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void Show(DoorNoteData note)
    {
        _currentNote = note;
        if (panel != null) panel.SetActive(true);

        if (titleText != null) titleText.text = note.title;
        if (bodyText != null) bodyText.text = note.text;

        foreach (Transform child in buttonContainer)
        {
            if (child.gameObject != buttonPrefab) Destroy(child.gameObject);
        }

        if (note.noteType == DoorNoteData.NoteType.External && note.options.Count > 0)
        {
            foreach (var option in note.options)
            {
                var btn = Instantiate(buttonPrefab, buttonContainer);
                var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = option.buttonText;

                var button = btn.GetComponent<Button>();
                var localOption = option;
                button.onClick.AddListener(() => OnOptionClicked(localOption));
            }
        }
        else
        {
            var btn = Instantiate(buttonPrefab, buttonContainer);
            var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = "Понятно";

            var button = btn.GetComponent<Button>();
            button.onClick.AddListener(OnCloseClicked);
        }
    }

    private void OnOptionClicked(DoorNoteData.Option option)
    {
        if (panel != null) panel.SetActive(false);

        EventManager.Instance.ApplyOption(option);

        if (option.movesToNextDay)
        {
            return;
        }

        EventManager.Instance.ResolveNote();

        var door = FindObjectOfType<InteractDoor>();
        if (door != null) door.SetNoteActive(false);
    }

    private void OnCloseClicked()
    {
        if (panel != null) panel.SetActive(false);
        EventManager.Instance.ResolveNote();

        var door = FindObjectOfType<InteractDoor>();
        if (door != null) door.SetNoteActive(false);
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
    }
}