using UnityEngine;

public class InteractNote : MonoBehaviour, IInteractable, IHighlightable
{
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

        if (_outlineMaterial == null)
            Debug.LogWarning($"[InteractNote] Материал обводки не найден на объекте {gameObject.name}. Назначьте Out Line Mat.mat на рендерер записи.", this);
    }

    void OnDestroy()
    {
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
}
