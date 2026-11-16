using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public class GeneratedCharacter
{
    // Пол
    public enum Gender
    {
        Male, Female
    }
    [SerializeField]public Gender gender;
    
    [SerializeField]public string fullName;
    [SerializeField]public int age;
    [SerializeField]public Sprite portrait;
    
    [SerializeField]public Dictionary<Stati, float> stats = new Dictionary<Stati, float>();
    
    [SerializeField]public List<Trait> traits = new List<Trait>();
    
    [SerializeField]public string description;
    
    [SerializeField]public bool isRevealed = false;
}