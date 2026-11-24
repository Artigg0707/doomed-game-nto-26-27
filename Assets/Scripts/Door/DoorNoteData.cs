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
        public bool movesToNextDay;
    }

    public NoteType noteType;
    public string title;
    [TextArea]
    public string text;
    public int triggerDay;
    public string morningReportLine;
    public List<Option> options = new List<Option>();
}