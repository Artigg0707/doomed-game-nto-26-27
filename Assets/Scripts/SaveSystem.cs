using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int version = 1;
    public int currentDay;
    public ResourceSaveData resources;
    public List<CharacterSaveData> characters;
    public List<ExpeditionSaveData> activeExpeditions;
    public List<LocationCooldownSaveData> locationCooldowns;
    public string pendingRepairCharacterName;
    public int pendingRepairDaySent;
    public string activeNoteId;
    public bool noteResolvedToday;
    public Vector3SaveData playerPosition;
    public List<CustomSaveEntry> custom = new List<CustomSaveEntry>();
}

[Serializable]
public class CustomSaveEntry
{
    public string key;
    public string value;
}

[Serializable]
public class ResourceSaveData
{
    public int foodCans;
    public int waterBottles;
    public int fuelCans;
    public int temperatureLevel;
    public bool generatorBroken;
    public bool generatorIsOn;
}

[Serializable]
public class CharacterSaveData
{
    public string fullName;
    public int gender;
    public int age;
    public int health;
    public int morale;
    public int daysWithoutFood;
    public int daysWithoutWater;
    public bool receivesFoodToday;
    public bool receivesWaterToday;
    public int consecutiveFoodDays;
    public int consecutiveWaterDays;
    public bool isDead;
    public bool isExiled;
    public bool isSecretRevealed;
    public List<string> traitNames;
    public string description;
    public string portraitName;
}

[Serializable]
public class ExpeditionSaveData
{
    public string locationId;
    public string characterName;
    public int daysRemaining;
}

[Serializable]
public class LocationCooldownSaveData
{
    public string locationId;
    public int availableOnDay;
}

[Serializable]
public class Vector3SaveData
{
    public float x;
    public float y;
    public float z;
}

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;
    public string saveFolder = "Saves";
    public string extension = ".json";
    public string currentSlot = "save1";
    public List<CustomSaveEntry> customEntries = new List<CustomSaveEntry>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public string GetPath(string slot)
    {
        return Path.Combine(Application.persistentDataPath, saveFolder, slot + extension);
    }

    public void SaveGame(string slot)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(GetPath(slot)));
        SaveData data = CollectData();
        foreach (var entry in customEntries)
        {
            data.custom.Add(new CustomSaveEntry { key = entry.key, value = entry.value });
        }
        File.WriteAllText(GetPath(slot), JsonUtility.ToJson(data, true));
    }

    public void SaveCurrentSlot()
    {
        SaveGame(currentSlot);
    }

    public void LoadGame(string slot)
    {
        string path = GetPath(slot);
        if (!File.Exists(path)) return;
        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        ApplyData(data);
    }

    public void LoadCurrentSlot()
    {
        LoadGame(currentSlot);
    }

    public void ResetGame(string slot)
    {
        string path = GetPath(slot);
        if (File.Exists(path)) File.Delete(path);
    }

    public void ResetCurrentSlot()
    {
        ResetGame(currentSlot);
    }

    public bool HasSave(string slot)
    {
        return File.Exists(GetPath(slot));
    }

    public void RefreshUI()
    {
        var rm = ResourceManager.Instance;
        rm?.NotifyResourcesChanged();

        var cm = CharacterManager.Instance;
        cm?.NotifyRationsChanged();

        var em = FindObjectOfType<ExpeditionManager>();
        em?.NotifyExpeditionsChanged();

        var epc = FindObjectOfType<ElectricalPanelController>();
        epc?.NotifyGeneratorStatusChanged();

        var dcm = DayCycleManager.Instance;
        dcm?.NotifyDayChanged();
    }

    public void AddCustomData(string key, string value)
    {
        customEntries.Add(new CustomSaveEntry { key = key, value = value });
    }

    private SaveData CollectData()
    {
        SaveData data = new SaveData();

        ResourceManager rm = ResourceManager.Instance;
        if (rm != null && rm.resourceData != null)
        {
            data.resources = new ResourceSaveData
            {
                foodCans = rm.resourceData.foodCans,
                waterBottles = rm.resourceData.waterBottles,
                fuelCans = rm.resourceData.fuelCans,
                temperatureLevel = rm.resourceData.temperatureLevel,
                generatorBroken = rm.resourceData.generatorBroken,
                generatorIsOn = rm.resourceData.generatorIsOn
            };
        }

        DayCycleManager dcm = DayCycleManager.Instance;
        if (dcm != null) data.currentDay = dcm.CurrentDay;

        CharacterManager cm = CharacterManager.Instance;
        if (cm != null && cm.characters != null)
        {
            data.characters = new List<CharacterSaveData>();
            foreach (var c in cm.characters)
            {
                if (c == null) continue;
                var cd = new CharacterSaveData
                {
                    fullName = c.fullName,
                    gender = (int)c.gender,
                    age = c.age,
                    health = c.health,
                    morale = c.morale,
                    daysWithoutFood = c.daysWithoutFood,
                    daysWithoutWater = c.daysWithoutWater,
                    receivesFoodToday = c.receivesFoodToday,
                    receivesWaterToday = c.receivesWaterToday,
                    consecutiveFoodDays = c.consecutiveFoodDays,
                    consecutiveWaterDays = c.consecutiveWaterDays,
                    isDead = c.isDead,
                    isExiled = c.isExiled,
                    isSecretRevealed = c.isSecretRevealed,
                    traitNames = new List<string>(),
                    description = c.description,
                    portraitName = c.portrait != null ? c.portrait.name : ""
                };
                if (c.traits != null)
                {
                    foreach (var t in c.traits)
                    {
                        if (t != null) cd.traitNames.Add(t.traitName);
                    }
                }
                data.characters.Add(cd);
            }
        }

        ExpeditionManager em = FindObjectOfType<ExpeditionManager>();
        if (em != null)
        {
            data.activeExpeditions = new List<ExpeditionSaveData>();
            var active = GetField<object>(em, "activeExpeditions") as IList;
            if (active != null)
            {
                foreach (var e in active)
                {
                    if (e == null) continue;
                    var loc = GetField<object>(e, "location");
                    var chr = GetField<object>(e, "character");
                    var days = GetField<int>(e, "daysRemaining");
                    data.activeExpeditions.Add(new ExpeditionSaveData
                    {
                        locationId = loc != null ? GetField<string>(loc, "locationId") : "",
                        characterName = chr != null ? GetField<string>(chr, "fullName") : "",
                        daysRemaining = days
                    });
                }
            }

            data.locationCooldowns = new List<LocationCooldownSaveData>();
            var cooldowns = GetField<object>(em, "locationCooldowns") as IList;
            if (cooldowns != null)
            {
                foreach (var c in cooldowns)
                {
                    if (c == null) continue;
                    var loc = GetField<object>(c, "location");
                    var avail = GetField<int>(c, "availableOnDay");
                    data.locationCooldowns.Add(new LocationCooldownSaveData
                    {
                        locationId = loc != null ? GetField<string>(loc, "locationId") : "",
                        availableOnDay = avail
                    });
                }
            }
        }

        ElectricalPanelController epc = FindObjectOfType<ElectricalPanelController>();
        if (epc != null)
        {
            data.pendingRepairDaySent = GetField<int>(epc, "pendingRepairDaySent");
            var prc = GetField<object>(epc, "pendingRepairCharacter");
            data.pendingRepairCharacterName = prc != null ? GetField<string>(prc, "fullName") : "";
        }

        EventManager ev = EventManager.Instance;
        if (ev != null)
        {
            data.noteResolvedToday = GetField<bool>(ev, "_noteResolvedToday");
            var note = GetField<object>(ev, "_activeNote");
            data.activeNoteId = note != null ? (note as UnityEngine.Object)?.name : "";
        }

        GameObject player = GameObject.Find("игрок");
        if (player != null)
        {
            var pos = player.transform.position;
            data.playerPosition = new Vector3SaveData { x = pos.x, y = pos.y, z = pos.z };
        }

        return data;
    }

    private void ApplyData(SaveData data)
    {
        ResourceManager rm = ResourceManager.Instance;
        if (rm != null && data.resources != null && rm.resourceData != null)
        {
            rm.resourceData.foodCans = data.resources.foodCans;
            rm.resourceData.waterBottles = data.resources.waterBottles;
            rm.resourceData.fuelCans = data.resources.fuelCans;
            rm.resourceData.temperatureLevel = data.resources.temperatureLevel;
            rm.resourceData.generatorBroken = data.resources.generatorBroken;
            rm.resourceData.generatorIsOn = data.resources.generatorIsOn;
        }

        DayCycleManager dcm = DayCycleManager.Instance;
        if (dcm != null)
        {
            SetField(dcm, "<CurrentDay>k__BackingField", data.currentDay);
        }

        CharacterManager cm = CharacterManager.Instance;
        if (cm != null && data.characters != null)
        {
            var loadedCharacters = new List<GeneratedCharacter>();
            foreach (var cd in data.characters)
            {
                var c = new GeneratedCharacter();
                c.fullName = cd.fullName;
                c.gender = (GeneratedCharacter.Gender)cd.gender;
                c.age = cd.age;
                c.health = cd.health;
                c.morale = cd.morale;
                c.daysWithoutFood = cd.daysWithoutFood;
                c.daysWithoutWater = cd.daysWithoutWater;
                c.receivesFoodToday = cd.receivesFoodToday;
                c.receivesWaterToday = cd.receivesWaterToday;
                c.consecutiveFoodDays = cd.consecutiveFoodDays;
                c.consecutiveWaterDays = cd.consecutiveWaterDays;
                c.isDead = cd.isDead;
                c.isExiled = cd.isExiled;
                c.isSecretRevealed = cd.isSecretRevealed;
                c.description = cd.description;
                c.traits = new List<Trait>();
                if (!string.IsNullOrEmpty(cd.portraitName))
                {
                    c.portrait = FindSpriteByName(cd.portraitName);
                }
                if (cd.traitNames != null)
                {
                    foreach (var tn in cd.traitNames)
                    {
                        var trait = FindTraitByName(tn);
                        if (trait != null) c.traits.Add(trait);
                    }
                }
                loadedCharacters.Add(c);
            }
            cm.ReloadCharacters(loadedCharacters);
        }

        ExpeditionManager em = FindObjectOfType<ExpeditionManager>();
        if (em != null)
        {
            if (data.activeExpeditions != null)
            {
                var listObj = GetField<object>(em, "activeExpeditions");
                var list = listObj as IList;
                if (list != null)
                {
                    list.Clear();
                    foreach (var ed in data.activeExpeditions)
                    {
                        var loc = FindLocationById(ed.locationId);
                        var chr = FindCharacterByName(ed.characterName);
                        if (loc != null && chr != null)
                        {
                            var run = new ExpeditionManager.ExpeditionRun();
                            SetField(run, "location", loc);
                            SetField(run, "character", chr);
                            SetField(run, "daysRemaining", ed.daysRemaining);
                            list.Add(run);
                        }
                    }
                }
            }
            if (data.locationCooldowns != null)
            {
                var listObj = GetField<object>(em, "locationCooldowns");
                var list = listObj as IList;
                if (list != null)
                {
                    list.Clear();
                    foreach (var cd in data.locationCooldowns)
                    {
                        var loc = FindLocationById(cd.locationId);
                        if (loc != null)
                        {
                            var cooldown = CreateInstanceOfPrivateType(em, "LocationCooldown");
                            SetField(cooldown, "location", loc);
                            SetField(cooldown, "availableOnDay", cd.availableOnDay);
                            list.Add(cooldown);
                        }
                    }
                }
            }
        }

        ElectricalPanelController epc = FindObjectOfType<ElectricalPanelController>();
        if (epc != null)
        {
            SetField(epc, "pendingRepairDaySent", data.pendingRepairDaySent);
            if (!string.IsNullOrEmpty(data.pendingRepairCharacterName))
            {
                SetField(epc, "pendingRepairCharacter", FindCharacterByName(data.pendingRepairCharacterName));
            }
        }

        EventManager ev = EventManager.Instance;
        if (ev != null)
        {
            SetField(ev, "_noteResolvedToday", data.noteResolvedToday);
            if (!string.IsNullOrEmpty(data.activeNoteId))
            {
                var note = FindNoteByName(data.activeNoteId);
                SetField(ev, "_activeNote", note);
            }
        }

        GameObject player = GameObject.Find("игрок");
        if (player != null && data.playerPosition != null)
        {
            player.transform.position = new Vector3(data.playerPosition.x, data.playerPosition.y, data.playerPosition.z);
        }

        var rm2 = ResourceManager.Instance;
        rm2?.NotifyResourcesChanged();

        var cm2 = CharacterManager.Instance;
        cm2?.NotifyRationsChanged();

        var em2 = FindObjectOfType<ExpeditionManager>();
        em2?.NotifyExpeditionsChanged();

        var epc2 = FindObjectOfType<ElectricalPanelController>();
        epc2?.NotifyGeneratorStatusChanged();

        var dcm2 = DayCycleManager.Instance;
        dcm2?.NotifyDayChanged();
    }

    private object CreateInstanceOfPrivateType(object context, string typeName)
    {
        var type = context.GetType().Assembly.GetType(context.GetType().FullName + "+" + typeName);
        if (type == null)
        {
            type = context.GetType().Assembly.GetType(typeName);
        }
        return Activator.CreateInstance(type, nonPublic: true);
    }

    private Trait FindTraitByName(string name)
    {
        var cm = CharacterManager.Instance;
        if (cm != null && cm.genCgaracter != null && cm.genCgaracter.profile != null && cm.genCgaracter.profile.availableTraits != null)
        {
            foreach (var t in cm.genCgaracter.profile.availableTraits)
            {
                if (t != null && t.traitName == name) return t;
            }
        }
        return null;
    }

    private ExpeditionLocationData FindLocationById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        var all = Resources.LoadAll<ExpeditionLocationData>("");
        foreach (var l in all)
        {
            if (l != null && l.locationId == id) return l;
        }
        return null;
    }

    private GeneratedCharacter FindCharacterByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        var cm = CharacterManager.Instance;
        if (cm != null && cm.characters != null)
        {
            foreach (var c in cm.characters)
            {
                if (c != null && c.fullName == name) return c;
            }
        }
        return null;
    }

    private Sprite FindSpriteByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        var cm = CharacterManager.Instance;
        if (cm != null && cm.genCgaracter != null && cm.genCgaracter.profile != null)
        {
            var profile = cm.genCgaracter.profile;
            if (profile.maleObliks != null)
            {
                foreach (var s in profile.maleObliks)
                {
                    if (s != null && s.name == name) return s;
                }
            }
            if (profile.femaleObliks != null)
            {
                foreach (var s in profile.femaleObliks)
                {
                    if (s != null && s.name == name) return s;
                }
            }
        }
        var allSprites = Resources.LoadAll<Sprite>("");
        foreach (var s in allSprites)
        {
            if (s != null && s.name == name) return s;
        }
        return null;
    }

    private DoorNoteData FindNoteByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        var ev = EventManager.Instance;
        if (ev != null && ev.notes != null)
        {
            foreach (var n in ev.notes)
            {
                if (n != null && n.name == name) return n;
            }
        }
        return null;
    }

    private T GetField<T>(object obj, string name)
    {
        var type = obj.GetType();
        var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        if (field != null) return (T)field.GetValue(obj);
        return default(T);
    }

    private void SetField(object obj, string name, object value)
    {
        var type = obj.GetType();
        var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        if (field != null) field.SetValue(obj, value);
    }
}
