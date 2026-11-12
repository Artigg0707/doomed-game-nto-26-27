using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    
    public float speed;
    
    public CharacterController controller;
    public CinemachineCamera cam;
    private Vector2 _moveInput;
    
    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnMove(InputValue val)
    {
        _moveInput = val.Get<Vector2>();
    }

    private void Update()
    {
        controller.Move((GetForward() * _moveInput.y + GetRight() * _moveInput.x) * speed * Time.deltaTime);
    }
    
    private Vector3 GetForward()
    {
        Vector3 forward = cam.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    private Vector3 GetRight()
    {
        Vector3 right = cam.transform.right;
        right.y = 0;
        return right.normalized;
    }
}
