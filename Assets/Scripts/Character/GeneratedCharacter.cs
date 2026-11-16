using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GeneratedCharacter
{
    public enum Gender
    {
        Male, Female
    }

    [SerializeField] public Gender gender;

    [SerializeField] public string fullName;
    [SerializeField] public int age;
    [SerializeField] public Sprite portrait;

    [Header("Основные параметры")]
    [SerializeField] [Range(0, 100)] public int health = 100;
    [SerializeField] [Range(0, 100)] public int morale = 100;

    [Header("Статусы выживания")]
    [SerializeField] public int daysWithoutFood = 0;
    [SerializeField] public int daysWithoutWater = 0;

    [Header("Состояние")]
    [SerializeField] public bool isDead = false;
    [SerializeField] public bool isExiled = false;
    [SerializeField] public bool isSecretRevealed = false;

    [SerializeField] public List<Trait> traits = new List<Trait>();
    [SerializeField] public string description;
}