using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TeachManager : MonoBehaviour
{
    [Header("Шаги обучения")]
    [SerializeField] private TeachSteps teachSteps;

    [Header("UI обучения")]
    [SerializeField] private TextMeshProUGUI textDiscriptionStep;
    [SerializeField] private TextMeshProUGUI textProgress;

    private float correctProgressStep = 0;
    private int correctProgressTeach = 0;
    private int maxStepTeach;
    void Start()
    {
        maxStepTeach = teachSteps.steps.Length;
    }

    void Update()
    {
        textDiscriptionStep.text = teachSteps.steps[correctProgressTeach].stepDescription;
        textProgress.text = "Статус: " + (int)correctProgressStep + "/" + teachSteps.steps[correctProgressTeach].requirement;

        if (Input.GetKeyDown(KeyCode.P))
        {
            correctProgressTeach++;
            correctProgressStep = 0;
        }
    }

    public void UpdateStepOnce(string key)
    {
        UpdateStep(key,1);
    }
    public void UpdateStep(string key,float pointsProgress)
    {
        if(teachSteps.steps[correctProgressTeach].stepName != key) return;

        correctProgressStep += pointsProgress;


        if(correctProgressStep >= teachSteps.steps[correctProgressTeach].requirement)
        {
            correctProgressStep = 0;
            correctProgressTeach++;
        }
    }
}
