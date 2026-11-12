using Unity.Cinemachine;
using UnityEngine;

public class InteractPC : MonoBehaviour, IInteractable
{
    public CinemachineCamera playerCamera;
    public CinemachineCamera PCCamera;
    
    public CinemachineBrain  brain;
    public float changeSpeed;
    
    public void Interact()
    {
        PCCamera.Priority = 20;
        
    }

    void Start()
    {
        PCCamera.Priority = 0;
    }
    void Update()
    {
        brain.DefaultBlend.Time = changeSpeed;
        if (Input.GetKeyDown(KeyCode.N))
        {
            PCCamera.Priority = 0;
        }
    }
}
