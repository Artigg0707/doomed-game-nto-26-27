using Unity.Cinemachine; 
using UnityEngine;

public class CinemachineCursorLook : MonoBehaviour
{
    private Vector3 _cursPosition;
    private float _normalizedX;
    private float _normalizedY;

    
    public float rotationSpeedX = 100f; 
    
    public float maxAngleY = 30f;       
    
    [Range(0f, 0.9f)]
    public float deadZone = 0.2f; 
    
    private float _currentYaw = 0f;   
    private float _currentPitch = 0f; 
    
    public float smoothSpeed = 5f;
    
    public CinemachineCamera vcam;

    void Update()
    {
        _cursPosition = Input.mousePosition;
        
        _normalizedX = (_cursPosition.x - Screen.width / 2f) / (Screen.width / 2f);
        _normalizedY = (_cursPosition.y - Screen.height / 2f) / (Screen.height / 2f);
        
        _normalizedX = ApplyDeadZone(_normalizedX, deadZone);
        _normalizedY = ApplyDeadZone(_normalizedY, deadZone);
        
        _currentYaw += _normalizedX * rotationSpeedX * Time.deltaTime;

        
        float targetPitch = -_normalizedY * maxAngleY;
        _currentPitch = Mathf.Lerp(_currentPitch, targetPitch, smoothSpeed * Time.deltaTime);
        
        transform.localRotation = Quaternion.Euler(_currentPitch, _currentYaw, 0);
    }

    float ApplyDeadZone(float value, float zone)
    {
        float absVal = Mathf.Abs(value);
        if (absVal < zone) return 0f;
        return Mathf.Sign(value) * ((absVal - zone) / (1f - zone));
    }
}