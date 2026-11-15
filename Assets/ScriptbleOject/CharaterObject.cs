using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CharaterObject", menuName = "Scriptable Objects/шаблон перса")]
public class CharaterObject : ScriptableObject
{
    public List<Stati> availableStats;
    //public List<disease> availableDiseases;
    public List<Trait> availableTraits;
    
    public int minAge;
    public int maxAge;
    public List<Sprite> avObliks = new List<Sprite>();
    //public string appearanceText;


    //public int minDiseases = 0;
    //public int maxDiseases;
    
    public NamePool namePool;
    
    
    

}
