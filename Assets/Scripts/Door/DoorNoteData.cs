using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DoorNote", menuName = "Door Note")]
public class DoorNoteData : ScriptableObject
{
    public enum NoteType
    {
        External,
        Morning,
        Quiet
    }

    [System.Serializable]
    public class Option
    {
        public string buttonText;
        public int deltaFood;
        public int deltaWater;
        public int deltaFuel;
        public int deltaMorale;
        public int hpDamage;
        public int spawnCharacterChance;
    }

    public enum SpawnType
    {
        Scheduled,  // по конкретному дню (Trigger Day)
        Random      // по шансу в любой день без запланированной записи
    }

    public NoteType noteType;
    public SpawnType spawnType = SpawnType.Scheduled;
    public string title;
    [TextArea]
    public string text;
    public int triggerDay;
    [Range(0, 100)]
    public int randomChance; // шанс появиться в день, когда нет запланированной записи (0-100)
    public string morningReportLine;
    public List<Option> options = new List<Option>();
}