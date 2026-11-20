using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ExpeditionInstance
{
    public string locationId;
    public List<string> participantNames = new List<string>();
    public int startDay;
    public int returnDay;
    public bool resolved;
}