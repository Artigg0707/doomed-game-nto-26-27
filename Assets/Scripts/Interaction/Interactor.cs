using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

interface IInteractable
{
    public void Interact();
}

interface IHighlightable
{
    public void Highlight(bool state);
}

public class Interactor : MonoBehaviour
{
    public Transform InteractorSource;
    public Camera mainCamera;
    [SerializeField] private float InteractRange = 5f;
    private IHighlightable _currentHighlighted;
    void Start() 
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        if (mainCamera == null) return;

        Ray r = mainCamera.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(r.origin, r.direction * InteractRange, Color.red);

        IHighlightable targetHighlight = null;

        bool pointerOverUI =
            EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject();

        if (!pointerOverUI &&
            Physics.Raycast(r, out RaycastHit hitInfo, InteractRange))
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (hitInfo.collider.gameObject.TryGetComponent(
                        out IInteractable interactObj))
                {
                    interactObj.Interact();
                }
            }

            if (hitInfo.collider.gameObject.TryGetComponent(
                    out IHighlightable highlightObj))
            {
                targetHighlight = highlightObj;
            }
        }

        if (_currentHighlighted != targetHighlight)
        {
            if (_currentHighlighted != null)
                _currentHighlighted.Highlight(false);

            _currentHighlighted = targetHighlight;

            if (_currentHighlighted != null)
                _currentHighlighted.Highlight(true);
        }
    }
}