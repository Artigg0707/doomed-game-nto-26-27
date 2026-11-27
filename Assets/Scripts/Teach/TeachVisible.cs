using UnityEngine;

public class TeachVisible : MonoBehaviour
{
    private DayCycleManager _dayCycleManager;
    void Start()
    {
        _dayCycleManager = FindFirstObjectByType<DayCycleManager>();
    }

    void Update()
    {
        if(_dayCycleManager.CurrentDay == 0)
        {
            this.gameObject.SetActive(true);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }
}
