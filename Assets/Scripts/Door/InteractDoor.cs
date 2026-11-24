using UnityEngine;

public class InteractDoor : MonoBehaviour, IInteractable, IHighlightable
{
    public GameObject noteObject;
    public DoorNoteUI noteUI;

    private Material _outlineMaterial;
    private bool _isInteracting;
    public float highlightValue = 0.01f;
    public float normalValue = 0f;
    public string propertyName = "_OutlineSize";

    void Start()
    {
        var renderer = GetComponentInChildren<Renderer>(true);
        if (renderer != null)
        {
            var mats = renderer.materials;
            if (mats.Length > 1) _outlineMaterial = mats[1];
            else if (mats.Length == 1) _outlineMaterial = mats[0];
        }

        if (DayCycleManager.Instance != null)
            DayCycleManager.Instance.OnDayChanged += OnNewDay;

        UpdateNoteVisibility();
    }

    void OnDestroy()
    {
        if (DayCycleManager.Instance != null)
            DayCycleManager.Instance.OnDayChanged -= OnNewDay;
    }

    void OnNewDay(int day)
    {
        UpdateNoteVisibility();
    }

    void UpdateNoteVisibility()
    {
        bool hasNote = EventManager.Instance != null && EventManager.Instance.HasActiveNote();
        if (noteObject != null)
            noteObject.SetActive(hasNote);
    }

    void Update()
    {
        if (_isInteracting && noteUI != null && noteUI.gameObject.activeInHierarchy == false)
        {
            _isInteracting = false;
            ApplyHighlightValue(normalValue);
        }
    }

    public void Interact()
    {
        if (EventManager.Instance == null || !EventManager.Instance.HasActiveNote()) return;

        _isInteracting = true;
        ApplyHighlightValue(normalValue);

        if (noteUI != null)
        {
            var note = EventManager.Instance.GetActiveNote();
            noteUI.Show(note);
        }
    }

    public void Highlight(bool state)
    {
        if (_isInteracting) return;
        ApplyHighlightValue(state ? highlightValue : normalValue);
    }

    private void ApplyHighlightValue(float value)
    {
        if (_outlineMaterial != null)
            _outlineMaterial.SetFloat(propertyName, value);
    }

    public void SetNoteActive(bool active)
    {
        if (noteObject != null) noteObject.SetActive(active);
    }
}