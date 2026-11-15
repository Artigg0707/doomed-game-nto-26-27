using UnityEngine;
using System.Collections.Generic;

public class GeneratedCharacter
{
    // Пол
    public enum Gender
    {
        Male, Female
    }
    public Gender gender;
    
    public string fullName;
    public int age;
    public Sprite portrait;
    
    public Dictionary<Stati, float> stats = new Dictionary<Stati, float>();
    
    public List<Trait> traits = new List<Trait>();
    
    public string description;
    
    public bool isRevealed = false;
}