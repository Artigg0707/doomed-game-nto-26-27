using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExpeditionManager : MonoBehaviour
{
    public static ExpeditionManager Instance { get; private set; }

    public List<ExpeditionLocation> locations = new List<ExpeditionLocation>();
    public List<ExpeditionInstance> active = new List<ExpeditionInstance>();

    public CharacterManager characterManager;
    public ResourceManager resourceManager;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool CanSend(GeneratedCharacter c)
    {
        if (c == null) return false;
        if (c.isDead || c.isExiled) return false;
        if (c.status != CharacterStatus.Free) return false;
        return true;
    }

    public ExpeditionInstance Send(ExpeditionLocation loc, List<GeneratedCharacter> participants, int currentDay)
    {
        if (loc == null || participants == null || participants.Count == 0 || participants.Count > 2)
            return null;
        foreach (var p in participants)
            if (!CanSend(p)) return null;

        var inst = new ExpeditionInstance
        {
            locationId = loc.id,
            startDay = currentDay,
            returnDay = currentDay + 1
        };
        foreach (var p in participants)
        {
            inst.participantNames.Add(p.fullName);
            p.status = CharacterStatus.OnExpedition;
            p.expeditionLocationId = loc.id;
            p.expeditionReturnDay = inst.returnDay;
        }
        active.Add(inst);
        return inst;
    }

    public void ResolveReturns(int currentDay, NightReportData report)
    {
        foreach (var inst in active)
        {
            if (inst.resolved || inst.returnDay != currentDay) continue;
            var loc = locations.FirstOrDefault(l => l.id == inst.locationId);
            if (loc != null)
            {
                if (resourceManager != null)
                {
                    resourceManager.AddFood(loc.rewardFood);
                    resourceManager.AddWater(loc.rewardWater);
                    resourceManager.AddFuel(loc.rewardFuel);
                }
                if (report != null)
                    report.expeditionReturns.Add($"Экспедиция ({loc.displayName}): +{loc.rewardFood} еды, +{loc.rewardWater} воды, +{loc.rewardFuel} топлива");
            }
            foreach (var name in inst.participantNames)
            {
                var c = characterManager != null ? characterManager.characters.Find(x => x.fullName == name) : null;
                if (c != null)
                {
                    c.status = CharacterStatus.Free;
                    c.expeditionLocationId = "";
                    c.expeditionReturnDay = -1;
                }
            }
            inst.resolved = true;
        }
        active.RemoveAll(i => i.resolved);
    }
}