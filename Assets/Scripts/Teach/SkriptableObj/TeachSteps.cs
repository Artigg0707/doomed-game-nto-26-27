using UnityEngine;

[System.Serializable]
public class TeachStep
{
    public string stepName;

    [TextArea(3,10)]
    public string stepDescription;
    public int requirement;
}

[CreateAssetMenu(fileName = "TeachSteps", menuName = "Scriptable Objects/TeachSteps")]
public class TeachSteps : ScriptableObject
{
    public TeachStep[] steps;
}
