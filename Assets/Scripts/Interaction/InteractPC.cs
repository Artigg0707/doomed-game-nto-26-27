using Unity.Cinemachine;
using UnityEngine;

public class InteractPC : MonoBehaviour, IInteractable, IHighlightable
{
    private Material _outlineMaterial;
    private IOpenablePanel _panelOpener;

    public CinemachineCamera playerCamera;
    public CinemachineCamera objCam;

    [Header("Panel to open")]
    [SerializeField] private GameObject panelToOpen;

    public float highlightValue = 0.01f;
    public float normalValue = 0f;
    public string propertyName = "_OutlineSize";

    private bool _isInteracting = false;

    void Start()
    {
        Renderer renderer = GetComponentInChildren<Renderer>(true);
        if (panelToOpen != null)
            panelToOpen.SetActive(false);
        if (renderer != null)
        {
            Material[] mats = renderer.materials;

            if (mats.Length > 1)
                _outlineMaterial = mats[1];
            else if (mats.Length == 1)
                _outlineMaterial = mats[0];
        }

        // ������ �������������: ��� ������� ������ � ����� �� ���������.
        if (objCam != null)
            objCam.Priority = 0;

        FindPanelOpener();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
            outCam();

        // ������� ���������� ��������� ����� �������� ������.
        if (_isInteracting &&
            panelToOpen != null &&
            !panelToOpen.activeInHierarchy)
        {
            _isInteracting = false;
            ApplyHighlightValue(normalValue);
        }
    }

    public void Interact()
    {
        if (objCam != null)
            objCam.Priority = 20;

        _isInteracting = true;
        ApplyHighlightValue(normalValue);

        if (panelToOpen == null)
        {
            Debug.LogWarning(
                "InteractPC: panelToOpen не назначена. Взаимодействие продолжит работу без панели.",
                this);
            return;
        }

        if (panelToOpen.activeInHierarchy)
            return;

        if (_panelOpener != null)
            _panelOpener.Open();
        else
            panelToOpen.SetActive(true);
    }

    public void Highlight(bool state)
    {
        if (_isInteracting)
            return;

        float targetValue = state ? highlightValue : normalValue;
        ApplyHighlightValue(targetValue);
    }

    private void FindPanelOpener()
    {
        if (panelToOpen == null)
            return;

        MonoBehaviour[] components =
            panelToOpen.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour component in components)
        {
            if (component is IOpenablePanel opener)
            {
                _panelOpener = opener;
                return;
            }
        }
    }
    
    public void outCam()
    {
        if (objCam != null)
            objCam.Priority = 0;

        if (panelToOpen != null)
            panelToOpen.SetActive(false);
        _isInteracting = false;
        ApplyHighlightValue(normalValue);
    }
    private void ApplyHighlightValue(float value)
    {
        if (_outlineMaterial != null)
        {
            _outlineMaterial.SetFloat(propertyName, value);
            _outlineMaterial.SetColor(
                "_OutLine_Color",
                value == 0 ? Color.black : Color.white);
        }
    }
}