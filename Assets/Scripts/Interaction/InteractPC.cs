using Unity.Cinemachine;
using UnityEngine;

public class InteractPC : MonoBehaviour, IInteractable, IHighlightable
{
    public RationMenuUI rationMenu;
    private Material _outlineMaterial;
    public CinemachineCamera playerCamera;
    public CinemachineCamera objCam;
    public float highlightValue = 1.1f; 
    public float normalValue = 0f;
    public string propertyName = "_OutlineSize";
    private bool _isInteracting = false;
    
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Material[] mats = renderer.materials;
            if (mats.Length > 1)
            {
                _outlineMaterial = mats[1];
            }
            else
            {
                _outlineMaterial = mats[0];
            }
        }
        objCam.Priority = 0;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            objCam.Priority = 0;
            _isInteracting = false;
            ApplyHighlightValue(normalValue);
        }
    }
    public void Interact()
    {
        objCam.Priority = 20;
        _isInteracting = true;
        ApplyHighlightValue(normalValue);

        if (rationMenu != null)
            rationMenu.Open();
    }
    public void outCam()
    {
        objCam.Priority = 0;
        
    }
    public void Highlight(bool state)
    {
        if (_isInteracting) return;
        float targetValue = state ? highlightValue : normalValue;
        ApplyHighlightValue(targetValue);
    }
    private void ApplyHighlightValue(float value)
    {
        if (_outlineMaterial != null)
        {
            _outlineMaterial.SetFloat(propertyName, value);
        }
    }
}