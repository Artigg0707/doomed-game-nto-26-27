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

        ClearButtons();

        if (note.noteType == DoorNoteData.NoteType.External && note.options.Count > 0)
        {
            foreach (var option in note.options)
            {
                var btn = Instantiate(buttonPrefab, buttonContainer, false);
                var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = option.buttonText;

                var button = btn.GetComponent<Button>();
                var localOption = option;
                button.onClick.AddListener(() => OnOptionClicked(localOption));
            }
        }
        else
        {
            var btn = Instantiate(buttonPrefab, buttonContainer, false);
            var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = "\u041f\u043e\u043d\u044f\u0442\u043d\u043e";

            var button = btn.GetComponent<Button>();
            button.onClick.AddListener(OnCloseClicked);
        }
    }

    private void ClearButtons()
    {
        for (int i = buttonContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(buttonContainer.GetChild(i).gameObject);
        }
    }

    private void OnOptionClicked(DoorNoteData.Option option)
    {
        if (panel != null) panel.SetActive(false);

        EventManager.Instance.ApplyOption(option);
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