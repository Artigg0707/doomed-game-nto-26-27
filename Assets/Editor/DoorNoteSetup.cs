#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class DoorNoteSetup
{
    [MenuItem("Tools/Setup Door Notes")]
    public static void Setup()
    {
        string notesPath = "Assets/Door/Notes";
        if (!AssetDatabase.IsValidFolder("Assets/Door"))
        {
            AssetDatabase.CreateFolder("Assets", "Door");
        }
        if (!AssetDatabase.IsValidFolder(notesPath))
        {
            AssetDatabase.CreateFolder("Assets/Door", "Notes");
        }

        var note1 = ScriptableObject.CreateInstance<DoorNoteData>();
        note1.noteType = DoorNoteData.NoteType.External;
        note1.title = "Стук у двери";
        note1.text = "Кто-то стучит снаружи. Обещает место с топливом, если откроешь дверь.";
        note1.triggerDay = 2;
        note1.morningReportLine = "Под дверью оставили записку";

        var opt1 = new DoorNoteData.Option { buttonText = "Открыть дверь", deltaFood = 0, deltaWater = 0, deltaFuel = 2, deltaMorale = 5, hpDamage = 10, spawnCharacterChance = 25, movesToNextDay = false };
        var opt2 = new DoorNoteData.Option { buttonText = "Не открывать", deltaFood = 0, deltaWater = 0, deltaFuel = 0, deltaMorale = -5, hpDamage = 0, spawnCharacterChance = 0, movesToNextDay = false };
        var opt3 = new DoorNoteData.Option { buttonText = "Потребовать доказательства", deltaFood = 0, deltaWater = 0, deltaFuel = 0, deltaMorale = 0, hpDamage = 0, spawnCharacterChance = 0, movesToNextDay = true };
        note1.options.Add(opt1);
        note1.options.Add(opt2);
        note1.options.Add(opt3);

        var note2 = ScriptableObject.CreateInstance<DoorNoteData>();
        note2.noteType = DoorNoteData.NoteType.Quiet;
        note2.title = "Шуршание у двери";
        note2.text = "Ночью за дверью кто-то дышал в ритм вентиляции. Утром вы нашли отпечатки пальцев на гермодвери — со стороны коридора, где никого быть не должно.";
        note2.triggerDay = 3;
        note2.morningReportLine = "Под дверью ночью кто-то шуршал. Возможно там записка, но вы не решились выглянуть.";

        var opt4 = new DoorNoteData.Option { buttonText = "Понятно", deltaFood = 2, deltaWater = 0, deltaFuel = 0, deltaMorale = 0, hpDamage = 0, spawnCharacterChance = 0, movesToNextDay = false };

        var note3 = ScriptableObject.CreateInstance<DoorNoteData>();
        note3.noteType = DoorNoteData.NoteType.Morning;
        note3.title = "Конверт у двери";
        note3.text = "Незнакомец, kterого впустили вчера, ушёл, но оставил координаты склада. На складе +2 банки еды.";
        note3.triggerDay = 4;
        note3.morningReportLine = "Утром у двери нашли конверт";
        note3.options.Add(opt4);

        AssetDatabase.CreateAsset(note1, notesPath + "/DoorNote_Day2_Knock.asset");
        AssetDatabase.CreateAsset(note2, notesPath + "/DoorNote_Day3_Quiet.asset");
        AssetDatabase.CreateAsset(note3, notesPath + "/DoorNote_Day4_Morning.asset");

        var manager = GameObject.Find("Manager");
        if (manager != null)
        {
            var eventManager = manager.GetComponent<EventManager>();
            if (eventManager != null)
            {
                eventManager.notes = new List<DoorNoteData> { note1, note2, note3 };
                EditorUtility.SetDirty(eventManager);
            }
        }

        var door = GameObject.Find("Door");
        if (door != null)
        {
            var interactDoor = door.GetComponent<InteractDoor>();
            if (interactDoor == null)
            {
                interactDoor = door.AddComponent<InteractDoor>();
            }
            EditorUtility.SetDirty(door);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Door notes setup complete");
    }
}
#endif