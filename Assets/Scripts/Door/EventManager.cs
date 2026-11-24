using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    public List<DoorNoteData> notes = new List<DoorNoteData>();

    private DoorNoteData _activeNote;
    private bool _noteResolvedToday;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        DayCycleManager.Instance.OnDayChanged += OnNewDay;
    }

    private void OnDestroy()
    {
        if (DayCycleManager.Instance != null)
            DayCycleManager.Instance.OnDayChanged -= OnNewDay;
    }

    private void OnNewDay(int day)
    {
        _noteResolvedToday = false;
        _activeNote = null;

        foreach (var note in notes)
        {
            if (note.triggerDay == day)
            {
                _activeNote = note;
                break;
            }
        }
    }

    public bool HasActiveNote()
    {
        return _activeNote != null && !_noteResolvedToday;
    }

    public DoorNoteData GetActiveNote()
    {
        return _activeNote;
    }

    public void ResolveNote()
    {
        _noteResolvedToday = true;
        _activeNote = null;
    }

    public string GetQuietLineForDay(int day)
    {
        foreach (var note in notes)
        {
            if (note.triggerDay == day && note.noteType == DoorNoteData.NoteType.Quiet)
            {
                return note.morningReportLine;
            }
        }
        return null;
    }

    public void ApplyOption(DoorNoteData.Option option)
    {
        var resources = ResourceManager.Instance;
        if (resources != null)
        {
            if (option.deltaFood > 0) resources.AddFood(option.deltaFood);
            else if (option.deltaFood < 0) resources.ConsumeFood(Mathf.Abs(option.deltaFood));

            if (option.deltaWater > 0) resources.AddWater(option.deltaWater);
            else if (option.deltaWater < 0) resources.ConsumeWater(Mathf.Abs(option.deltaWater));

            if (option.deltaFuel > 0) resources.AddFuel(option.deltaFuel);
            else if (option.deltaFuel < 0) resources.ConsumeFuel(Mathf.Abs(option.deltaFuel));
        }

        var characterManager = CharacterManager.Instance;
        if (characterManager != null)
        {
            foreach (var character in characterManager.characters)
            {
                if (character != null && !character.isDead && !character.isExiled)
                {
                    character.morale = Mathf.Clamp(character.morale + option.deltaMorale, 0, 100);
                }
            }

            if (option.hpDamage > 0)
            {
                var valid = new List<GeneratedCharacter>();
                foreach (var character in characterManager.characters)
                {
                    if (character != null && !character.isDead && !character.isExiled)
                        valid.Add(character);
                }
                if (valid.Count > 0)
                {
                    var target = valid[UnityEngine.Random.Range(0, valid.Count)];
                    target.health = Mathf.Max(0, target.health - option.hpDamage);
                    if (target.health <= 0) target.isDead = true;
                }
            }

            if (UnityEngine.Random.Range(0, 100) < option.spawnCharacterChance)
            {
                characterManager.CreateCharacter();
            }
        }
    }
}