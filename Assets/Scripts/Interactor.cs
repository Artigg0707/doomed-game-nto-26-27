using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IInteractable
{
    public void Interact();
}

    

public class Interactor : MonoBehaviour
{
    public Transform InteractorSource;
    [SerializeField] private float InteractRange;

    void Start() {
        
    }

    void Update() {
        
            Ray r = new Ray(InteractorSource.position, InteractorSource.forward);
            Debug.DrawRay(r.origin, r.direction * InteractRange, Color.red);
            if (Physics.Raycast(r, out RaycastHit hitInfo, InteractRange)) {
                if (Input.GetKeyDown(KeyCode.E)) {
                    if (hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactObj)) {
                        interactObj.Interact();
                    }
                }
            }
        
    }
}