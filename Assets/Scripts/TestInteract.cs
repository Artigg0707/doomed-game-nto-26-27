using UnityEngine;

public class TestInteract : MonoBehaviour, IInteractable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void Interact()
    {
        Debug.Log("Успех! Объект отреагировал на нажатие E.");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
