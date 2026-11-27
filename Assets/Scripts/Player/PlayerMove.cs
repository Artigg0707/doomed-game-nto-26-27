
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

}