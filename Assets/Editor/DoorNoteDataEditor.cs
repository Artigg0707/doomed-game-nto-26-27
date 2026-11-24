using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DoorNoteData))]
public class DoorNoteDataEditor : Editor
{
    SerializedProperty spawnTypeProp;
    SerializedProperty triggerDayProp;
    SerializedProperty randomChanceProp;

    private void OnEnable()
    {
        spawnTypeProp = serializedObject.FindProperty("spawnType");
        triggerDayProp = serializedObject.FindProperty("triggerDay");
        randomChanceProp = serializedObject.FindProperty("randomChance");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(spawnTypeProp);

        var spawnType = (DoorNoteData.SpawnType)spawnTypeProp.enumValueIndex;

        if (spawnType == DoorNoteData.SpawnType.Scheduled)
        {
            EditorGUILayout.PropertyField(triggerDayProp, new GUIContent("Trigger Day"));
        }
        else
        {
            EditorGUILayout.PropertyField(randomChanceProp, new GUIContent("Random Chance"));
        }

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("noteType"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("title"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("text"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("morningReportLine"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("options"));

        serializedObject.ApplyModifiedProperties();
    }
}
