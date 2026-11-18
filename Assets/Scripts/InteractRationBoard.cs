using UnityEngine;

public class InteractRationBoard : MonoBehaviour, IInteractable, IHighlightable
{
    private Material _outlineMaterial;

    public RationMenuUI rationMenu;
    public float highlightValue = 1.1f;
    public float normalValue = 0f;
    public string propertyName = "_OutlineSize";

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();

        if (renderer != null)
        {
            Material[] mats = renderer.materials;

            if (mats.Length > 1)
                _outlineMaterial = mats[1];
            else if (mats.Length == 1)
                _outlineMaterial = mats[0];
        }
    }

    public void Interact()
    {
        if (rationMenu != null)
            rationMenu.Open();
        else
            Debug.LogError("В InteractRationBoard не назначен RationMenuUI.", this);
    }

    public void Highlight(bool state)
    {
        float targetValue = state ? highlightValue : normalValue;
        ApplyHighlightValue(targetValue);
    }

    private void ApplyHighlightValue(float value)
    {
        if (_outlineMaterial != null)
            _outlineMaterial.SetFloat(propertyName, value);
    }
}