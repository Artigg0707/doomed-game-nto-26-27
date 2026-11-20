using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ExpeditionLocationCatalog",
    menuName = "Game/Expeditions/Location Catalog")]
public class ExpeditionLocationCatalog : ScriptableObject
{
    public List<ExpeditionLocationData> locations =
        new List<ExpeditionLocationData>();
}