using UnityEngine;
using System.Collections.Generic;
    
    
[CreateAssetMenu(fileName = "NamePool", menuName = "Scriptable Objects/список имён")]
public class NamePool : ScriptableObject
{
    public List<string> maleNames;
    public List<string> maleLastNames;
    
    public List<string> femaleNames;
    public List<string> femaleLastNames;
    
}
