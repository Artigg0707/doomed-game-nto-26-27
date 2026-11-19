using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NightReportData
{
    public int completedDay;
    public int newDay;

    public bool fuelConsumed;
    public bool generatorWorked;
    public int temperatureBefore;
    public int temperatureAfter;

    public List<string> resourceChanges = new List<string>();
    public List<string> survivalChanges = new List<string>();
    public List<string> deaths = new List<string>();
    public List<string> expeditionReturns = new List<string>();
    public List<string> secretReports = new List<string>();

    public bool HasAnyEvents =>
        resourceChanges.Count > 0 ||
        survivalChanges.Count > 0 ||
        deaths.Count > 0 ||
        expeditionReturns.Count > 0 ||
        secretReports.Count > 0;

    public string GetSummaryLine()
    {
        if (deaths.Count > 0)
            return $"Потери: {string.Join(", ", deaths)}.";
        if (specialEvents.Count > 0)
            return $"Событий ночью: {specialEvents.Count}.";
        return "Все живы. Инцидентов за ночь не зафиксировано.";
    }

    public List<string> specialEvents = new List<string>();
}